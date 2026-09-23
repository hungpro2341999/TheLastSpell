
# Lộ trình tìm hiểu Source Code "The Last Spell"

Với một dự án game đồ sộ sử dụng kiến trúc MVC và Data-driven khổng lồ như *The Last Spell*, việc đọc code từ trên xuống dưới theo bảng chữ cái sẽ khiến bạn rất dễ bị "lạc trôi". 

Dưới đây là lộ trình (Roadmap) chuẩn xác nhất để bạn có thể nắm bắt hệ thống từ bao quát đến chi tiết:

---

## 🌻 Bước 1: Hiểu bức tranh tổng thể (The Entry Point & Managers)
Đừng vội nhảy vào các file xử lý chi tiết (như đánh nhau hay xây nhà). Hãy bắt đầu từ phần "Đầu não" điều khiển vòng lặp của game.

**Thư mục cần đọc:** `TheLastStand.Manager`
1. **`ApplicationManager.cs` / `GameManager.cs`**: Nơi quản lý vòng lặp chính của game. Hãy đọc kỹ các trạng thái (States) của game như: `Management` (Lúc xây nhà ban ngày), `Deployment` (Lúc dàn trận binh lính), `Night` (Lúc thủ thành).
2. **`TileMapManager.cs`**: Bản đồ grid vuông là linh hồn của game chiến thuật. Hãy xem cách họ quản lý mảng lưới gạch (Grid) và tọa độ.
3. **`BuildingManager.cs` & `UnitManager.cs`**: Nơi khởi tạo, lưu trữ danh sách và quản lý vòng đời chung của toàn bộ Nhà cửa và Lính/Quái.

---

## 🌻 Bước 2: Nắm vững hệ thống Dữ liệu (Data-Driven Design)
Game này được thiết kế theo hướng **Data-driven**, tức là cấu hình quái/nhà/kỹ năng nằm ở file Text bên ngoài. Bạn cần hiểu cách dữ liệu chảy vào game trước khi xem cách game xử lý chúng.

**Thứ tự đọc:**
1. Mở thư mục **`TextAsset`**: Xem thử một file txt (như `BuildingDefinitions.txt` hoặc `SkillDefinitions_Enemies.txt`) để biết mặt mũi dữ liệu gốc.
2. Thư mục **`TheLastStand.Definition`**: Đây là các class C# dùng để "hứng" dữ liệu từ file txt (Chứa các biến tĩnh như `Id`, `Cost`, `Damage`...).
3. Thư mục **`TheLastStand.Database`**: Nơi lưu trữ bộ nhớ đệm (Cache) của toàn bộ Definitions. (Ví dụ: `BuildingDatabase.BuildingDefinitions` là một Dictionary chứa mọi loại nhà).

---

## 🌻 Bước 3: Tìm hiểu mô hình MVC thông qua 1 tính năng nhỏ (Vertical Slice)
Game chia rạch ròi 3 tầng: **Model** (Dữ liệu), **Controller** (Logic), và **View** (Hình ảnh, UI Unity). Để hiểu cách 3 tầng này giao tiếp, hãy chọn 1 hệ thống nhỏ và dễ hiểu nhất: **Item (Vật phẩm)**.

**Thứ tự đọc:**
1. **`TheLastStand.Model.Item.Item.cs`**: Class thuần dữ liệu, không kế thừa MonoBehaviour. Chứa độ bền, tên, chỉ số của món đồ.
2. **`TheLastStand.Controller.Item.ItemController.cs`**: Chứa logic xử lý (Trang bị đồ, Cởi đồ, Sinh ra đồ mới ngẫu nhiên). 
3. **`TheLastStand.View.Item.ItemView.cs`** (Hoặc `ItemSlotView`): Component gắn trên Unity GameObject, chịu trách nhiệm đổi Sprite hình cái gươm/giáp và lắng nghe Event từ Model.

> **💡 Nguyên tắc bắt buộc:** Model KHÔNG BAO GIỜ biết Controller hay View là ai. Controller thay đổi Model. Model bắn Event. View nghe Event để cập nhật hình ảnh.

---

## 🌻 Bước 4: Đào sâu vào 2 Hệ thống lõi (Entities)
Sau khi hiểu MVC, hãy bắt đầu nhảy vào 2 vật thể to nhất và phức tạp nhất game. Ở đây họ sử dụng **Component-based Architecture** (như bạn đã thấy ở các bài phân tích trước).

1. **Hệ thống Xây dựng (Building):**
   - Đọc theo lộ trình: `TheLastStand.Model.Building` $\rightarrow$ `TheLastStand.Controller.Building.Module`
   - Hiểu cách một cái nhà được "lắp ráp" từ nhiều linh kiện (Modules) như thế nào.
2. **Hệ thống Đơn vị (Unit - Lính & Quái):**
   - Đọc thư mục: `TheLastStand.Controller.Unit`
   - Hiểu cách họ tách biệt logic của Hero (PlayableUnit) và Quái (Enemy). Xem cách `Pathfinding` (tìm đường) hoạt động.

---

## 🌻 Bước 5: Tìm hiểu Hệ thống Chiến đấu & AI (Combat & Skills)
Đây là phần tinh túy nhất của một game chiến thuật Turn-based.

1. **Hệ thống Kỹ năng (Skill):** `TheLastStand.Controller.Skill`
   - Cách tính toán tầm đánh (Line of Sight).
   - Cơ chế diện rộng (AoE).
   - Tương tác với hệ thống hiệu ứng `CastFx` (chính là file bạn đang mở).
2. **AI của Quái (Enemy AI):** `TheLastStand.Controller.Unit.Enemy.GoalCondition`
   - Game này có hàng ngàn con quái cùng lúc, AI của chúng phải rất tối ưu. Đọc các file Goal và Condition để xem cách một con quái quyết định ưu tiên đập nhà hay đập người.

---

## 🌻 Bước 6: Các hệ thống Phụ trợ & Tiện ích (Cuối cùng)
Chỉ đọc các phần này khi bạn cần làm tính năng liên quan:
- **Lưu/Tải Game (Save/Load):** Thư mục `TheLastStand.Serialization`.
- **Sương mù (Fog):** `TheLastStand.Controller.Fog`.
- **Thành tích & Meta (Oraculum):** Quản lý những chỉ số mở khóa vĩnh viễn ngoài sảnh chờ (`TheLastStand.Manager.Meta`).

---

## 🗺️ Sơ đồ Tổng quát Hệ thống
```mermaid
graph TD
    %% Tầng Dữ liệu (Load từ Text)
    subgraph Data Layer
        TXT(TextAsset Files) --> DEF(Definition Classes)
        DEF --> DB(Databases)
    }

    %% Tầng Quản lý Tổng
    subgraph Global Managers
        DB --> BM(BuildingManager)
        DB --> UM(UnitManager)
        DB --> SM(SkillManager)
        GM(GameManager) --> BM
        GM --> UM
    }

    %% Tầng MVC Core
    subgraph MVC Architecture
        Controller(Controllers) -->|Tính toán logic| Model(Models)
        Model -.->|Bắn Event Cập nhật| View(Views / Unity UI)
    }

    BM --> Controller
    UM --> Controller
```
