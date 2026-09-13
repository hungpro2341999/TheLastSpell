import os
import re
import tkinter as tk
from tkinter import ttk, filedialog, messagebox
from pathlib import Path
try:
    from PIL import Image, ImageTk
except ImportError:
    print("Vui lòng cài đặt thư viện Pillow bằng lệnh: pip install Pillow")
    import sys
    sys.exit(1)

def natural_sort_key(s):
    """ Hàm hỗ trợ sắp xếp theo thứ tự số tự nhiên (vd: img_2 < img_10) """
    return [int(text) if text.isdigit() else text.lower() for text in re.split('([0-9]+)', str(s))]

class ImageViewerApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Image & Animation Viewer")
        self.root.geometry("1100x700")
        
        # Thư mục mặc định của bạn
        self.default_dir = r"D:\Project Source Code\TheLastStand\AssetRipper_export_20260901_035218\ExportedProject\Assets"
        self.all_images = []
        self.filtered_images = []
        self.image_extensions = {'.png', '.jpg', '.jpeg', '.gif', '.bmp', '.webp', '.tga'}
        
        self.animating = False
        self.anim_index = 0
        self.anim_images = [] 
        self.current_image_obj = None # Giữ reference để không bị thu hồi bộ nhớ
        
        self.setup_ui()
        
        # Load ngay lần đầu
        self.root.after(100, lambda: self.load_images(self.default_dir))
        
    def setup_ui(self):
        # Top Frame: Search and controls
        top_frame = ttk.Frame(self.root, padding="10")
        top_frame.pack(side=tk.TOP, fill=tk.X)
        
        ttk.Label(top_frame, text="Thư mục:").pack(side=tk.LEFT)
        self.folder_var = tk.StringVar(value=self.default_dir)
        self.folder_entry = ttk.Entry(top_frame, textvariable=self.folder_var, width=60)
        self.folder_entry.pack(side=tk.LEFT, padx=5)
        
        ttk.Button(top_frame, text="Duyệt...", command=self.browse_folder).pack(side=tk.LEFT, padx=5)
        ttk.Button(top_frame, text="Tải lại", command=lambda: self.load_images(self.folder_var.get())).pack(side=tk.LEFT, padx=5)
        
        # Search Frame
        search_frame = ttk.Frame(self.root, padding="10")
        search_frame.pack(side=tk.TOP, fill=tk.X)
        
        ttk.Label(search_frame, text="Lọc theo tên:").pack(side=tk.LEFT)
        self.search_var = tk.StringVar()
        self.search_var.trace("w", self.filter_images)
        self.search_entry = ttk.Entry(search_frame, textvariable=self.search_var, width=40)
        self.search_entry.pack(side=tk.LEFT, padx=5)
        self.search_entry.focus()
        
        ttk.Label(search_frame, text="  (Ví dụ: gõ 'Attack' sẽ lọc ra chuỗi Attack_0, Attack_1...)").pack(side=tk.LEFT)
        
        # Anim Frame
        anim_frame = ttk.Frame(self.root, padding="10")
        anim_frame.pack(side=tk.TOP, fill=tk.X)
        
        ttk.Button(anim_frame, text="▶ Chạy chuỗi ảnh (Anim)", command=self.play_animation).pack(side=tk.LEFT)
        ttk.Button(anim_frame, text="⏹ Dừng", command=self.stop_animation).pack(side=tk.LEFT, padx=5)
        
        ttk.Label(anim_frame, text="Tốc độ (ms/frame):").pack(side=tk.LEFT, padx=10)
        self.speed_var = tk.IntVar(value=100)
        self.speed_entry = ttk.Entry(anim_frame, textvariable=self.speed_var, width=6)
        self.speed_entry.pack(side=tk.LEFT)
        
        # Main Frame: List and Canvas
        main_frame = ttk.Frame(self.root, padding="10")
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # Left side: Listbox
        list_frame = ttk.Frame(main_frame)
        list_frame.pack(side=tk.LEFT, fill=tk.Y)
        
        self.status_label = ttk.Label(list_frame, text="Đang tải...")
        self.status_label.pack(side=tk.TOP, anchor=tk.W)
        
        scrollbar = ttk.Scrollbar(list_frame)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        
        self.listbox = tk.Listbox(list_frame, yscrollcommand=scrollbar.set, width=50)
        self.listbox.pack(side=tk.LEFT, fill=tk.Y, expand=True)
        scrollbar.config(command=self.listbox.yview)
        
        self.listbox.bind("<<ListboxSelect>>", self.on_select)
        
        # Right side: Canvas
        canvas_frame = ttk.Frame(main_frame)
        canvas_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True, padx=10)
        
        self.canvas = tk.Canvas(canvas_frame, bg="#2e2e2e")
        self.canvas.pack(fill=tk.BOTH, expand=True)
        
        # Buộc canvas cập nhật kích thước
        self.root.update()
        
    def browse_folder(self):
        folder = filedialog.askdirectory(initialdir=self.default_dir)
        if folder:
            self.folder_var.set(folder)
            self.load_images(folder)
            
    def load_images(self, directory):
        path = Path(directory)
        if not path.exists() or not path.is_dir():
            messagebox.showerror("Lỗi", f"Thư mục '{directory}' không tồn tại.")
            self.status_label.config(text="Lỗi thư mục")
            return
            
        self.status_label.config(text="Đang tìm ảnh...")
        self.root.update()
        
        self.all_images = []
        for file_path in path.rglob('*'):
            if file_path.is_file() and file_path.suffix.lower() in self.image_extensions:
                self.all_images.append(str(file_path))
                
        # Sort naturally để Sprite_2 đứng trước Sprite_10
        self.all_images.sort(key=natural_sort_key)
        self.filter_images()
        
    def filter_images(self, *args):
        query = self.search_var.get().lower()
        if not query:
            self.filtered_images = self.all_images
        else:
            self.filtered_images = [img for img in self.all_images if query in Path(img).name.lower()]
        
        self.listbox.delete(0, tk.END)
        for img in self.filtered_images:
            p = Path(img)
            # Hiện thư mục cha và tên file
            display_name = f"{p.parent.name}/{p.name}"
            self.listbox.insert(tk.END, display_name)
            
        self.status_label.config(text=f"Tìm thấy: {len(self.filtered_images)} ảnh")
        self.stop_animation()
            
    def on_select(self, event):
        if self.animating:
            self.stop_animation()
            
        selection = self.listbox.curselection()
        if not selection:
            return
            
        index = selection[0]
        img_path = self.filtered_images[index]
        self.show_image(img_path)
        
    def show_image(self, path):
        try:
            img = Image.open(path)
            
            # Cố gắng fit hình vào canvas nếu hình quá to
            canvas_w = self.canvas.winfo_width()
            canvas_h = self.canvas.winfo_height()
            
            if canvas_w > 10 and canvas_h > 10:
                # Tính tỷ lệ thu nhỏ (chỉ thu nhỏ nếu to hơn canvas)
                img_w, img_h = img.size
                if img_w > canvas_w or img_h > canvas_h:
                    ratio = min(canvas_w/img_w, canvas_h/img_h)
                    new_size = (int(img_w * ratio), int(img_h * ratio))
                    img = img.resize(new_size, Image.Resampling.LANCZOS)
                
            self.current_image_obj = ImageTk.PhotoImage(img)
            self.canvas.delete("all")
            self.canvas.create_image(canvas_w//2, canvas_h//2, anchor=tk.CENTER, image=self.current_image_obj)
        except Exception as e:
            print(f"Lỗi tải ảnh {path}: {e}")
            
    def play_animation(self):
        if not self.filtered_images:
            return
            
        self.animating = True
        self.anim_index = 0
        # Lấy chính các file đang được lọc để chạy
        self.anim_images = self.filtered_images 
        self.animate_frame()
        
    def stop_animation(self):
        self.animating = False
        
    def animate_frame(self):
        if not self.animating:
            return
            
        if self.anim_index >= len(self.anim_images):
            self.anim_index = 0 # loop lại từ đầu
            
        img_path = self.anim_images[self.anim_index]
        self.show_image(img_path)
        
        # Cập nhật listbox cho dễ theo dõi
        self.listbox.selection_clear(0, tk.END)
        self.listbox.selection_set(self.anim_index)
        self.listbox.see(self.anim_index)
        
        self.anim_index += 1
        
        try:
            speed = self.speed_var.get()
        except tk.TclError:
            speed = 100
        speed = max(10, speed)
        
        self.root.after(speed, self.animate_frame)

if __name__ == "__main__":
    root = tk.Tk()
    app = ImageViewerApp(root)
    root.mainloop()
