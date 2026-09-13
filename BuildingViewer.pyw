import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import xml.etree.ElementTree as ET
import xml.dom.minidom
import os
import re
from pathlib import Path
import threading
import queue

try:
    from PIL import Image, ImageTk
except ImportError:
    Image = None
    ImageTk = None
    print("Vui lòng cài đặt thư viện Pillow bằng lệnh: pip install Pillow")

def natural_sort_key(s):
    """Hàm hỗ trợ sắp xếp theo thứ tự số tự nhiên"""
    return [int(text) if text.isdigit() else text.lower() for text in re.split('([0-9]+)', str(s))]

class ImageViewerTab(tk.Frame):
    """Tab chứa UI tìm ảnh, lọc và chạy animation, được tối ưu hiệu năng.
    - Quét thư mục trong thread nền.
    - Lọc theo tên có debounce 300ms.
    - Listbox ảo (virtual) chỉ hiển thị phần dùng được để giảm lag.
    """
    def __init__(self, parent, default_dir=None):
        super().__init__(parent, bg="#1e1e1e")
        self.default_dir = default_dir or r"D:\Project Source Code\TheLastStand\AssetRipper_export_20260901_035218\ExportedProject\Assets"
        self.image_extensions = {'.png', '.jpg', '.jpeg', '.gif', '.bmp', '.webp', '.tga'}
        self.all_images = []          # full list of absolute paths
        self.filtered_images = []     # after applying search query (absolute paths)
        self.filtered_display = []    # strings displayed in listbox
        self.animating = False
        self.anim_index = 0
        self.anim_images = []
        self.current_image_obj = None
        # Thread/queue for background scan
        self._scan_queue = queue.Queue()
        self._scan_thread = None
        # Debounce job id
        self._filter_job = None
        # Virtual list parameters
        self._visible_start = 0
        self._visible_count = 80  # adjust if needed
        if Image is None:
            tk.Label(self, text="Thư viện Pillow chưa được cài đặt. Vui lòng chạy: pip install Pillow", fg="red", bg="#1e1e1e").pack(pady=20)
            return
        self.setup_ui()
        # Do NOT start scanning automatically – will load when user types a query
        self.scanned = False
        self.current_folder = self.default_dir
        # Prompt will be set after status_label is created in setup_ui


    # ---------------------------------------------------------------------
    # UI layout
    # ---------------------------------------------------------------------
    def setup_ui(self):
        # Top controls (folder selection)
        top_frame = tk.Frame(self, bg="#252526")
        top_frame.pack(side=tk.TOP, fill=tk.X, padx=5, pady=5)
        tk.Label(top_frame, text="Thư mục:", bg="#252526", fg="#d4d4d4").pack(side=tk.LEFT, padx=5)
        self.folder_var = tk.StringVar(value=self.default_dir)
        self.folder_entry = ttk.Entry(top_frame, textvariable=self.folder_var, width=50)
        self.folder_entry.pack(side=tk.LEFT, padx=5)
        ttk.Button(top_frame, text="Duyệt...", command=self.browse_folder).pack(side=tk.LEFT, padx=5)
        ttk.Button(top_frame, text="Tải lại", command=self.reload_folder).pack(side=tk.LEFT, padx=5)

        # Search & animation controls
        search_frame = tk.Frame(self, bg="#252526")
        search_frame.pack(side=tk.TOP, fill=tk.X, padx=5, pady=0)
        tk.Label(search_frame, text="Lọc tên ảnh:", bg="#252526", fg="#d4d4d4").pack(side=tk.LEFT, padx=5)
        self.search_var = tk.StringVar()
        self.search_var.trace("w", self.filter_images)  # debounce inside method
        self.search_entry = ttk.Entry(search_frame, textvariable=self.search_var, width=30)
        self.search_entry.pack(side=tk.LEFT, padx=5)
        ttk.Button(search_frame, text="▶ Chạy chuỗi ảnh (Anim)", command=self.play_animation).pack(side=tk.LEFT, padx=10)
        ttk.Button(search_frame, text="⏹ Dừng", command=self.stop_animation).pack(side=tk.LEFT, padx=5)
        tk.Label(search_frame, text="Tốc độ (ms/frame):", bg="#252526", fg="#d4d4d4").pack(side=tk.LEFT, padx=10)
        self.speed_var = tk.IntVar(value=100)
        self.speed_entry = ttk.Entry(search_frame, textvariable=self.speed_var, width=6)
        self.speed_entry.pack(side=tk.LEFT)

        # Main area: listbox on left, canvas on right (resizable via drag)
        main_frame = tk.PanedWindow(self, orient=tk.HORIZONTAL, bg="#3e3e42",
                                     sashwidth=5, sashrelief=tk.RAISED)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)

        # Listbox section (virtual list)
        list_frame = tk.Frame(main_frame, bg="#1e1e1e")
        self.status_label = tk.Label(list_frame, text="Nhập tên để tìm ảnh", bg="#1e1e1e", fg="#4EC9B0")
        self.status_label.pack(side=tk.TOP, anchor=tk.W)
        scrollbar = tk.Scrollbar(list_frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.listbox = tk.Listbox(list_frame, yscrollcommand=scrollbar.set,
                                  bg="#252526", fg="#d4d4d4",
                                  selectbackground="#062f4f",
                                  borderwidth=0, highlightthickness=1,
                                  highlightcolor="#3e3e42",
                                  activestyle='none')
        self.listbox.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        scrollbar.config(command=self._on_scrollbar)  # custom handler
        self.listbox_scrollbar = scrollbar  # keep reference for scroll updates
        # Bind mouse wheel & key navigation to virtual refresh
        self.listbox.bind("<MouseWheel>", self._on_scroll)
        self.listbox.bind("<Button-4>", self._on_scroll)   # Linux scroll up
        self.listbox.bind("<Button-5>", self._on_scroll)   # Linux scroll down
        self.listbox.bind("<Key-Up>", self._on_key_arrow)
        self.listbox.bind("<Key-Down>", self._on_key_arrow)
        self.listbox.bind("<<ListboxSelect>>", self.on_select)
        main_frame.add(list_frame, minsize=150, width=300)

        # Canvas section (image display)
        canvas_frame = tk.Frame(main_frame, bg="#1e1e1e")
        self.canvas = tk.Canvas(canvas_frame, bg="#2e2e2e", highlightthickness=0)
        self.canvas.pack(fill=tk.BOTH, expand=True)
        main_frame.add(canvas_frame, minsize=200)

    # ---------------------------------------------------------------------
    # Folder handling & background scanning
    # ---------------------------------------------------------------------
    def reload_folder(self):
        """Reload the folder stored in the entry and reset scan flag."""
        self.current_folder = self.folder_var.get()
        self.scanned = False
        self.load_images(self.current_folder)

    def browse_folder(self):
        folder = filedialog.askdirectory(initialdir=self.default_dir)
        if folder:
            self.folder_var.set(folder)
            self.load_images(folder)

    def load_images(self, directory):
        # Reset state
        self.all_images.clear()
        self.filtered_images.clear()
        self.filtered_display.clear()
        self.listbox.delete(0, tk.END)
        self.status_label.config(text="Đang quét…")
        # Cancel previous thread if still running
        if self._scan_thread and self._scan_thread.is_alive():
            # Clear queued items – thread will finish naturally
            while not self._scan_queue.empty():
                try: self._scan_queue.get_nowait()
                except: break
        # Start new scan thread
        self._scan_thread = threading.Thread(target=self._scan_worker,
                                            args=(directory,), daemon=True)
        self._scan_thread.start()
        # Begin polling queue for results
        self.after(100, self._process_scan_queue)

    def _scan_worker(self, directory):
        path = Path(directory)
        for fp in path.rglob('*'):
            if fp.is_file() and fp.suffix.lower() in self.image_extensions:
                # Only include files that are inside a folder named "Texture2D"
                if "Texture2D" in fp.parts:
                    self._scan_queue.put(str(fp))

    def _process_scan_queue(self):
        updated = False
        while not self._scan_queue.empty():
            self.all_images.append(self._scan_queue.get())
            updated = True
        if updated:
            # Show intermediate count (optional) – keep UI responsive
            self.status_label.config(text=f"Đang quét… {len(self.all_images)} ảnh")
        if self._scan_thread.is_alive():
            self.after(100, self._process_scan_queue)
        else:
            # Scan finished – final sort & apply current filter
            self.all_images.sort(key=natural_sort_key)
            self._apply_filter()
            self.status_label.config(text=f"Quét xong – {len(self.all_images)} ảnh")

    # ---------------------------------------------------------------------
    # Search (debounced) and virtual list handling
    # ---------------------------------------------------------------------
    def filter_images(self, *args):
        # Cancel previous debounced call
        if self._filter_job:
            self.after_cancel(self._filter_job)
        self._filter_job = self.after(300, self._apply_filter)  # 300ms debounce

    def _apply_filter(self):
        # If we have never scanned the folder, start scanning now
        if not getattr(self, "scanned", False):
            self.scanned = True
            # Use the current folder (set during init or later changes)
            self.load_images(self.current_folder)
            # After scanning finishes, _apply_filter will be called again automatically
            return
        query = self.search_var.get().lower()
        if query:
            self.filtered_images = [p for p in self.all_images if query in Path(p).name.lower()]
        else:
            # When query is empty after initial scan, we show nothing (or could show all)
            self.filtered_images = []
        # Build display strings once
        self.filtered_display = [f"{Path(p).parent.name}/{Path(p).name}" for p in self.filtered_images]
        self.status_label.config(text=f"Tìm thấy: {len(self.filtered_images)} ảnh")
        # Reset virtual scroll to top
        self._visible_start = 0
        self._refresh_virtual_list(reset=True)

    def _on_scrollbar(self, *args):
        """Handle scrollbar commands: moveto fraction / scroll N units|pages."""
        total = len(self.filtered_display)
        if total == 0:
            return
        action = args[0]
        if action == "moveto":
            frac = float(args[1])
            self._visible_start = max(0, min(int(frac * total), total - 1))
        elif action == "scroll":
            delta = int(args[1])
            kind = args[2]
            if kind == "units":
                self._visible_start += delta
            elif kind == "pages":
                self._visible_start += delta * self._visible_count
            self._visible_start = max(0, min(self._visible_start, total - 1))
        self._refresh_virtual_list()

    def _on_scroll(self, event=None):
        """Handle mouse wheel and keyboard scroll."""
        total = len(self.filtered_display)
        if total == 0:
            return "break"
        if event and hasattr(event, "delta") and event.delta:
            # Windows mouse wheel: delta is +/-120 per notch
            step = -1 if event.delta > 0 else 1
            self._visible_start += step * 3
        elif event and event.num == 4:
            self._visible_start -= 3
        elif event and event.num == 5:
            self._visible_start += 3
        self._visible_start = max(0, min(self._visible_start, total - 1))
        self._refresh_virtual_list()
        return "break"

    def _on_key_arrow(self, event):
        """Handle Up/Down arrow keys for virtual list navigation."""
        total = len(self.filtered_display)
        if total == 0:
            return "break"

        sel = self.listbox.curselection()
        if not sel:
            # Nothing selected — select first visible item
            self.listbox.selection_set(0)
            self.on_select(event)
            return "break"

        local_idx = sel[0]
        real_idx = self._visible_start + local_idx

        if event.keysym == "Down":
            new_real = real_idx + 1
            if new_real >= total:
                return "break"  # already at the end
            if local_idx >= self._visible_count - 1:
                # At bottom edge of visible window — scroll down
                self._visible_start += 1
                self._visible_start = min(self._visible_start, total - 1)
                self._refresh_virtual_list()
                # Keep selection at the bottom
                new_local = min(self._visible_count - 1, new_real - self._visible_start)
                self.listbox.selection_clear(0, tk.END)
                self.listbox.selection_set(new_local)
                self.listbox.see(new_local)
            else:
                # Normal move within visible area
                self.listbox.selection_clear(0, tk.END)
                self.listbox.selection_set(local_idx + 1)
                self.listbox.see(local_idx + 1)

        elif event.keysym == "Up":
            new_real = real_idx - 1
            if new_real < 0:
                return "break"  # already at the top
            if local_idx <= 0:
                # At top edge of visible window — scroll up
                self._visible_start -= 1
                self._visible_start = max(0, self._visible_start)
                self._refresh_virtual_list()
                self.listbox.selection_clear(0, tk.END)
                self.listbox.selection_set(0)
                self.listbox.see(0)
            else:
                # Normal move within visible area
                self.listbox.selection_clear(0, tk.END)
                self.listbox.selection_set(local_idx - 1)
                self.listbox.see(local_idx - 1)

        # Show the selected image
        self.on_select(event)
        return "break"

    def _refresh_virtual_list(self, reset=False):
        # Clear and insert only the slice that should be visible
        self.listbox.delete(0, tk.END)
        start = self._visible_start
        end = min(start + self._visible_count, len(self.filtered_display))
        for item in self.filtered_display[start:end]:
            self.listbox.insert(tk.END, item)
        # Update scrollbar to reflect full size
        if len(self.filtered_display) > 0:
            first = start / len(self.filtered_display)
            last = end / len(self.filtered_display)
            self.listbox_scrollbar.set(first, last)
        else:
            self.listbox_scrollbar.set(0, 1)
        if reset:
            # Ensure the view starts at top
            self.listbox.yview_moveto(0)

    # ---------------------------------------------------------------------
    # Image display & animation
    # ---------------------------------------------------------------------
    def on_select(self, event):
        if self.animating:
            self.stop_animation()
        sel = self.listbox.curselection()
        if not sel:
            return
        idx = sel[0]
        # Translate virtual index to real filtered list index
        real_idx = self._visible_start + idx
        if real_idx < len(self.filtered_images):
            self.show_image(self.filtered_images[real_idx])

    def show_image(self, path):
        if not Image:
            return
        try:
            img = Image.open(path)
            w, h = self.canvas.winfo_width(), self.canvas.winfo_height()
            if w > 10 and h > 10:
                iw, ih = img.size
                if iw > w or ih > h:
                    ratio = min(w / iw, h / ih)
                    img = img.resize((int(iw * ratio), int(ih * ratio)), Image.Resampling.LANCZOS)
            self.current_image_obj = ImageTk.PhotoImage(img)
            self.canvas.delete("all")
            self.canvas.create_image(w // 2, h // 2, anchor=tk.CENTER, image=self.current_image_obj)
        except Exception as e:
            print(f"Lỗi tải ảnh {path}: {e}")

    def play_animation(self):
        if not self.filtered_images:
            return
        self.animating = True
        self.anim_index = 0
        self.anim_images = self.filtered_images
        self._animate_frame()

    def stop_animation(self):
        self.animating = False

    def _animate_frame(self):
        if not self.animating:
            return
        if self.anim_index >= len(self.anim_images):
            self.anim_index = 0
        path = self.anim_images[self.anim_index]
        self.show_image(path)
        # Update selection to reflect current frame
        self.listbox.selection_clear(0, tk.END)
        # Ensure the selected index is within the currently displayed slice
        relative_idx = self.anim_index - self._visible_start
        if 0 <= relative_idx < self._visible_count:
            self.listbox.selection_set(relative_idx)
            self.listbox.see(relative_idx)
        else:
            # If out of view, scroll to make it visible
            fraction = self.anim_index / max(1, len(self.filtered_images))
            self.listbox.yview_moveto(fraction)
        self.anim_index += 1
        try:
            speed = self.speed_var.get()
        except tk.TclError:
            speed = 100
        speed = max(10, speed)
        self.after(speed, self._animate_frame)

# -------------------------------------------------------------------------
# BuildingViewer – unchanged XML viewer with added Image tab
# -------------------------------------------------------------------------
class BuildingViewer(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("The Last Stand - Universal XML & Image Viewer")
        self.geometry("1200x700")
        style = ttk.Style()
        style.theme_use('clam')
        self.bg_color = "#1e1e1e"
        self.panel_color = "#252526"
        self.text_color = "#d4d4d4"
        self.accent_color = "#4EC9B0"
        self.value_color = "#ce9178"
        self.configure(bg=self.bg_color)
        self.buildings = {}
        self.create_widgets()
        self.show_welcome()

    def create_widgets(self):
        # Toolbar
        toolbar = tk.Frame(self, bg="#333333")
        toolbar.pack(side=tk.TOP, fill=tk.X)
        open_btn = tk.Button(toolbar, text="📂 Mở File XML (Open)", bg="#0e639c", fg="white",
                             font=("Segoe UI", 10, "bold"), relief=tk.FLAT, command=self.open_file)
        open_btn.pack(side=tk.LEFT, padx=10, pady=5)
        self.file_label = tk.Label(toolbar, text="Chưa chọn file nào...", fg="#808080", bg="#333333",
                                font=("Segoe UI", 10, "italic"))
        self.file_label.pack(side=tk.LEFT, padx=10)

        # Main notebook containing three tabs
        main_frame = tk.Frame(self, bg=self.bg_color)
        main_frame.pack(fill=tk.BOTH, expand=True)
        self.main_nb = ttk.Notebook(main_frame)
        self.main_nb.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

        # ---------- XML Viewer Tab (keeps original layout) ----------
        self.tab_xml = tk.Frame(self.main_nb, bg=self.bg_color)
        self.main_nb.add(self.tab_xml, text=" 📄 XML Viewer ")
        # Left pane – object list
        left = tk.Frame(self.tab_xml, bg=self.bg_color)
        left.pack(side=tk.LEFT, fill=tk.Y, padx=10, pady=10)
        right = tk.Frame(self.tab_xml, bg=self.bg_color)
        right.pack(side=tk.RIGHT, fill=tk.BOTH, expand=True, padx=10, pady=10)
        tk.Label(left, text="Danh sách Đối tượng", fg=self.accent_color, bg=self.bg_color,
                 font=("Segoe UI", 12, "bold")).pack(anchor=tk.W, pady=(0, 10))
        list_frame = tk.Frame(left)
        list_frame.pack(fill=tk.Y, expand=True)
        self.listbox_xml = tk.Listbox(list_frame, width=40, bg=self.panel_color, fg=self.text_color,
                                      selectbackground="#062f4f", font=("Segoe UI", 11),
                                      borderwidth=0, highlightthickness=1, highlightcolor="#3e3e42")
        self.listbox_xml.pack(side=tk.LEFT, fill=tk.Y, expand=True)
        self.listbox_xml.bind('<<ListboxSelect>>', self.on_select)
        sb = tk.Scrollbar(list_frame, command=self.listbox_xml.yview)
        sb.pack(side=tk.RIGHT, fill=tk.Y)
        self.listbox_xml.config(yscrollcommand=sb.set)
        # Right pane – sub‑notebook for Parsed / Raw XML
        self.xml_nb = ttk.Notebook(right)
        self.xml_nb.pack(fill=tk.BOTH, expand=True)
        # Parsed UI tab
        self.tab_parsed = tk.Frame(self.xml_nb, bg=self.panel_color)
        self.xml_nb.add(self.tab_parsed, text=" Phân tích (Parsed UI) ")
        self.parsed_text_area = tk.Text(self.tab_parsed, wrap=tk.NONE, bg=self.panel_color,
                                        fg=self.text_color, borderwidth=0, padx=10, pady=10)
        self.parsed_text_area.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        ps_y = tk.Scrollbar(self.tab_parsed, command=self.parsed_text_area.yview)
        ps_y.pack(side=tk.RIGHT, fill=tk.Y)
        ps_x = tk.Scrollbar(self.tab_parsed, orient=tk.HORIZONTAL, command=self.parsed_text_area.xview)
        ps_x.pack(side=tk.BOTTOM, fill=tk.X)
        self.parsed_text_area.config(yscrollcommand=ps_y.set, xscrollcommand=ps_x.set)
        self.parsed_text_area.tag_config("header", font=("Segoe UI", 14, "bold"), foreground="white",
                                         background="#4EC9B0", spacing1=10, spacing3=10)
        self.parsed_text_area.tag_config("section", font=("Segoe UI", 11, "bold"), foreground="#569cd6", spacing1=10)
        self.parsed_text_area.tag_config("key", font=("Segoe UI", 10), foreground="#9cdcfe")
        self.parsed_text_area.tag_config("value", font=("Consolas", 10), foreground=self.value_color)
        self.parsed_text_area.tag_config("italic", font=("Segoe UI", 11, "italic"), foreground="#808080")
        # Raw XML tab
        self.tab_raw = tk.Frame(self.xml_nb, bg=self.panel_color)
        self.xml_nb.add(self.tab_raw, text=" Mã nguồn (Raw XML) ")
        self.raw_text_area = tk.Text(self.tab_raw, wrap=tk.NONE, bg="#1e1e1e", fg="#569cd6",
                                    font=("Consolas", 11), borderwidth=0, padx=10, pady=10)
        self.raw_text_area.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        rs_y = tk.Scrollbar(self.tab_raw, command=self.raw_text_area.yview)
        rs_y.pack(side=tk.RIGHT, fill=tk.Y)
        rs_x = tk.Scrollbar(self.tab_raw, orient=tk.HORIZONTAL, command=self.raw_text_area.xview)
        rs_x.pack(side=tk.BOTTOM, fill=tk.X)
        self.raw_text_area.config(yscrollcommand=rs_y.set, xscrollcommand=rs_x.set)

        # ---------- Image Viewer Tab (new, optimized) ----------
        self.tab_images = ImageViewerTab(self.main_nb)
        self.main_nb.add(self.tab_images, text=" 🖼️ Image Viewer ")
        self.main_nb.select(self.tab_images)  # show Image Viewer on launch

    # ---------------------------------------------------------------------
    # XML handling (unchanged apart from tab references)
    # ---------------------------------------------------------------------
    def open_file(self):
        file_path = filedialog.askopenfilename(title="Chọn file XML The Last Stand",
                                               filetypes=[("Text/XML files", "*.txt *.xml"), ("All files", "*.*")])
        if file_path:
            self.load_data(file_path)
            self.main_nb.select(self.tab_xml)  # switch to XML tab after load

    def load_data(self, file_path):
        self.listbox_xml.delete(0, tk.END)
        self.buildings.clear()
        self.parsed_text_area.config(state=tk.NORMAL)
        self.parsed_text_area.delete(1.0, tk.END)
        self.parsed_text_area.config(state=tk.DISABLED)
        self.file_label.config(text=file_path)
        try:
            tree = ET.parse(file_path)
            root = tree.getroot()
            for el in list(root):
                e_id = el.get("Id") or el.get("Value") or el.tag
                base = e_id
                cnt = 1
                while e_id in self.buildings:
                    e_id = f"{base} ({cnt})"
                    cnt += 1
                self.buildings[e_id] = el
                self.listbox_xml.insert(tk.END, e_id)
            self.show_welcome()
            self.title(f"XML & Image Viewer - {os.path.basename(file_path)}")
        except Exception as e:
            messagebox.showerror("Lỗi", f"Lỗi đọc XML: {e}")

    def show_welcome(self):
        self.parsed_text_area.config(state=tk.NORMAL)
        self.parsed_text_area.delete(1.0, tk.END)
        self.parsed_text_area.insert(tk.END, "Hãy mở file hoặc chọn một phần tử bên cột trái để xem thông số.", "italic")
        self.parsed_text_area.config(state=tk.DISABLED)
        self.update_raw_xml(None)

    def add_text_field(self, key, value, indent_level=0, is_section=False):
        spaces = " " * (indent_level * 4)
        if is_section:
            self.parsed_text_area.insert(tk.END, f"{spaces}{key}\n", "section")
        else:
            padded = f"{key}".ljust(max(15, 35 - (indent_level * 2)))
            self.parsed_text_area.insert(tk.END, f"{spaces}{padded} ", "key")
            self.parsed_text_area.insert(tk.END, f"{value}\n" if value else "\n", "value")

    def render_generic_xml(self, element, depth=0):
        if len(element) > 0 or depth == 0:
            if depth > 0:
                self.add_text_field(f"▼ {element.tag}", "", indent_level=depth, is_section=True)
            for k, v in element.attrib.items():
                if depth == 0 and k == "Id":
                    continue
                self.add_text_field(f"@{k}:", v, indent_level=depth + 1)
            if element.text and element.text.strip():
                self.add_text_field("Giá trị:", element.text.strip(), indent_level=depth + 1)
            for child in element:
                self.render_generic_xml(child, depth + 1)
        else:
            val = (element.text or "").strip()
            attr = " | ".join(f"{k}='{v}'" for k, v in element.attrib.items())
            key = f"▶ {element.tag}" + (f" [{attr}]" if attr else "")
            self.add_text_field(f"{key}:", val if val else "(Trống)", indent_level=depth)

    def update_raw_xml(self, xml_element):
        self.raw_text_area.config(state=tk.NORMAL)
        self.raw_text_area.delete(1.0, tk.END)
        if xml_element:
            raw = ET.tostring(xml_element, encoding="unicode", method="xml")
            try:
                pretty = xml.dom.minidom.parseString(raw).toprettyxml(indent="    ")
                pretty = os.linesep.join([l for l in pretty.splitlines() if l.strip()])
                self.raw_text_area.insert(tk.END, pretty)
            except Exception:
                self.raw_text_area.insert(tk.END, raw)
        self.raw_text_area.config(state=tk.DISABLED)

    def on_select(self, event):
        sel = event.widget.curselection()
        if not sel:
            return
        b_id = event.widget.get(sel[0])
        el = self.buildings.get(b_id)
        if not el:
            return
        self.parsed_text_area.config(state=tk.NORMAL)
        self.parsed_text_area.delete(1.0, tk.END)
        self.parsed_text_area.insert(tk.END, f" {b_id} \n", "header")
        self.render_generic_xml(el, depth=0)
        self.parsed_text_area.config(state=tk.DISABLED)
        self.update_raw_xml(el)

if __name__ == "__main__":
    app = BuildingViewer()
    app.mainloop()
