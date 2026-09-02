import tkinter as tk
from tkinter import ttk, filedialog, messagebox
import xml.etree.ElementTree as ET
import xml.dom.minidom
import os

class BuildingViewer(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("The Last Stand - Universal XML Viewer")
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
        # Top Toolbar
        toolbar = tk.Frame(self, bg="#333333")
        toolbar.pack(side=tk.TOP, fill=tk.X)
        
        open_btn = tk.Button(toolbar, text="📂 Mở File XML (Open)", bg="#0e639c", fg="white", font=("Segoe UI", 10, "bold"), relief=tk.FLAT, command=self.open_file)
        open_btn.pack(side=tk.LEFT, padx=10, pady=5)
        
        self.file_label = tk.Label(toolbar, text="Chưa chọn file nào...", fg="#808080", bg="#333333", font=("Segoe UI", 10, "italic"))
        self.file_label.pack(side=tk.LEFT, padx=10)

        # Khung dưới
        main_frame = tk.Frame(self, bg=self.bg_color)
        main_frame.pack(fill=tk.BOTH, expand=True)

        left_frame = tk.Frame(main_frame, bg=self.bg_color)
        left_frame.pack(side=tk.LEFT, fill=tk.Y, padx=10, pady=10)

        right_frame = tk.Frame(main_frame, bg=self.bg_color)
        right_frame.pack(side=tk.RIGHT, fill=tk.BOTH, expand=True, padx=10, pady=10)

        # Cột trái
        tk.Label(left_frame, text="Danh sách Đối tượng", fg=self.accent_color, bg=self.bg_color, font=("Segoe UI", 12, "bold")).pack(anchor=tk.W, pady=(0, 10))
        
        list_frame = tk.Frame(left_frame)
        list_frame.pack(fill=tk.Y, expand=True)
        
        self.listbox = tk.Listbox(list_frame, width=40, bg=self.panel_color, fg=self.text_color, selectbackground="#062f4f", font=("Segoe UI", 11), borderwidth=0, highlightthickness=1, highlightcolor="#3e3e42")
        self.listbox.pack(side=tk.LEFT, fill=tk.Y, expand=True)
        self.listbox.bind('<<ListboxSelect>>', self.on_select)
        
        scrollbar = tk.Scrollbar(list_frame, command=self.listbox.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.listbox.config(yscrollcommand=scrollbar.set)

        # Cột phải (Sử dụng Notebook/Tabs)
        self.notebook = ttk.Notebook(right_frame)
        self.notebook.pack(fill=tk.BOTH, expand=True)
        
        # Tab 1: Phân tích UI (Giờ dùng Text widget để cho phép bôi đen)
        self.tab_parsed = tk.Frame(self.notebook, bg=self.panel_color)
        self.notebook.add(self.tab_parsed, text=" Phân tích (Parsed UI) ")
        
        self.parsed_text_area = tk.Text(self.tab_parsed, wrap=tk.NONE, bg=self.panel_color, fg=self.text_color, borderwidth=0, padx=10, pady=10)
        self.parsed_text_area.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        parsed_scroll_y = tk.Scrollbar(self.tab_parsed, command=self.parsed_text_area.yview)
        parsed_scroll_y.pack(side=tk.RIGHT, fill=tk.Y)
        parsed_scroll_x = tk.Scrollbar(self.tab_parsed, orient=tk.HORIZONTAL, command=self.parsed_text_area.xview)
        parsed_scroll_x.pack(side=tk.BOTTOM, fill=tk.X)
        self.parsed_text_area.config(yscrollcommand=parsed_scroll_y.set, xscrollcommand=parsed_scroll_x.set)
        
        # Cấu hình các style (Tags) cho Parsed UI
        self.parsed_text_area.tag_config("header", font=("Segoe UI", 14, "bold"), foreground="white", background="#4EC9B0", spacing1=10, spacing3=10)
        self.parsed_text_area.tag_config("section", font=("Segoe UI", 11, "bold"), foreground="#569cd6", spacing1=10)
        self.parsed_text_area.tag_config("key", font=("Segoe UI", 10), foreground="#9cdcfe")
        self.parsed_text_area.tag_config("value", font=("Consolas", 10), foreground=self.value_color)
        self.parsed_text_area.tag_config("italic", font=("Segoe UI", 11, "italic"), foreground="#808080")

        # Tab 2: Raw XML
        self.tab_raw = tk.Frame(self.notebook, bg=self.panel_color)
        self.notebook.add(self.tab_raw, text=" Mã nguồn (Raw XML) ")
        
        self.raw_text_area = tk.Text(self.tab_raw, wrap=tk.NONE, bg="#1e1e1e", fg="#569cd6", font=("Consolas", 11), borderwidth=0, padx=10, pady=10)
        self.raw_text_area.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
        
        raw_scroll_y = tk.Scrollbar(self.tab_raw, command=self.raw_text_area.yview)
        raw_scroll_y.pack(side=tk.RIGHT, fill=tk.Y)
        raw_scroll_x = tk.Scrollbar(self.tab_raw, orient=tk.HORIZONTAL, command=self.raw_text_area.xview)
        raw_scroll_x.pack(side=tk.BOTTOM, fill=tk.X)
        self.raw_text_area.config(yscrollcommand=raw_scroll_y.set, xscrollcommand=raw_scroll_x.set)

    def open_file(self):
        file_path = filedialog.askopenfilename(
            title="Chọn file XML The Last Stand",
            filetypes=[("Text/XML files", "*.txt *.xml"), ("All files", "*.*")]
        )
        if file_path:
            self.load_data(file_path)

    def load_data(self, file_path):
        self.listbox.delete(0, tk.END)
        self.buildings.clear()
        
        self.parsed_text_area.config(state=tk.NORMAL)
        self.parsed_text_area.delete(1.0, tk.END)
        self.parsed_text_area.config(state=tk.DISABLED)
        
        self.file_label.config(text=file_path)
            
        try:
            tree = ET.parse(file_path)
            root = tree.getroot()
            
            elements = list(root)
            
            for el in elements:
                e_id = el.get("Id")
                if not e_id:
                    e_id = el.get("Value") or el.tag
                
                base_id = e_id
                counter = 1
                while e_id in self.buildings:
                    e_id = f"{base_id} ({counter})"
                    counter += 1
                    
                self.buildings[e_id] = el
                self.listbox.insert(tk.END, e_id)
                
            self.show_welcome()
            self.title(f"XML Viewer - {os.path.basename(file_path)}")
        except Exception as e:
            messagebox.showerror("Lỗi", f"Lỗi đọc XML: {e}")

    def show_welcome(self):
        self.parsed_text_area.config(state=tk.NORMAL)
        self.parsed_text_area.delete(1.0, tk.END)
        self.parsed_text_area.insert(tk.END, "Hãy mở file hoặc chọn một phần tử bên cột trái để xem thông số.", "italic")
        self.parsed_text_area.config(state=tk.DISABLED)
        self.update_raw_xml(None)

    def add_text_field(self, key, value, indent_level=0, is_section=False):
        spaces = " " * (indent_level * 4) # 4 spaces per indent level
        
        if is_section:
            self.parsed_text_area.insert(tk.END, f"{spaces}{key}\n", "section")
        else:
            # Padding the key to align values
            padded_key = f"{key}".ljust(max(15, 35 - (indent_level * 2)))
            self.parsed_text_area.insert(tk.END, f"{spaces}{padded_key} ", "key")
            if value:
                self.parsed_text_area.insert(tk.END, f"{value}\n", "value")
            else:
                self.parsed_text_area.insert(tk.END, "\n", "value")

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
            attr_str = ""
            if element.attrib:
                attr_str = " | ".join(f"{k}='{v}'" for k, v in element.attrib.items())
            
            key_text = f"▶ {element.tag}"
            if attr_str:
                key_text += f" [{attr_str}]"
            
            self.add_text_field(f"{key_text}:", val if val else "(Trống)", indent_level=depth)

    def update_raw_xml(self, xml_element):
        self.raw_text_area.config(state=tk.NORMAL)
        self.raw_text_area.delete(1.0, tk.END)
        
        if xml_element is not None and xml_element != "":
            raw_str = ET.tostring(xml_element, encoding="unicode", method="xml")
            try:
                parsed = xml.dom.minidom.parseString(raw_str)
                pretty_xml = parsed.toprettyxml(indent="    ")
                pretty_xml = os.linesep.join([s for s in pretty_xml.splitlines() if s.strip()])
                self.raw_text_area.insert(tk.END, pretty_xml)
            except:
                self.raw_text_area.insert(tk.END, raw_str)
        
        self.raw_text_area.config(state=tk.DISABLED)

    def on_select(self, event):
        selection = event.widget.curselection()
        if not selection: return
        
        b_id = event.widget.get(selection[0])
        b_def = self.buildings.get(b_id)
        if b_def is None: return
        
        # 1. Update Parsed UI
        self.parsed_text_area.config(state=tk.NORMAL)
        self.parsed_text_area.delete(1.0, tk.END)
        
        self.parsed_text_area.insert(tk.END, f" {b_id} \n", "header")
        
        self.render_generic_xml(b_def, depth=0)
        
        self.parsed_text_area.config(state=tk.DISABLED)
        
        # 2. Update Raw XML
        self.update_raw_xml(b_def)

if __name__ == "__main__":
    app = BuildingViewer()
    app.mainloop()
