# Phân tích Chi tiết: Thư mục gốc `TheLastStand.Controller`

Thư mục này không chứa thư mục con (sub-namespaces) mà chỉ tập trung vào **14 file trực tiếp**. Đây là những class nền tảng, điều phối các hệ thống chung nhất (Global Systems) của The Last Stand, hoặc định nghĩa các interface cốt lõi cho các Controller khác kế thừa.

Có thể chia 14 file này thành 4 nhóm chính:

---

## 1. Root Application & Game Flow (Quản lý luồng chính)

Đây là 2 Controller khổng lồ điều hành toàn bộ vòng đời của game.

### [`ApplicationController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/ApplicationController.cs)
- **Chức năng:** Trái tim của toàn bộ ứng dụng (chưa phải trong trận đấu).
- **Pattern:** State Machine (Automaton).
- **Hoạt động:** Quản lý việc chuyển đổi giữa các màn hình lớn của game: "GameLobby" (Main Menu), "SplashScreen", "NewGame", "WorldMap", "MetaShops" (Cửa hàng Oraculum), và "Game" (Vào trận).
- **Giao tiếp:** Gọi `CanvasFadeManager` để làm màn hình tối đi khi chuyển cảnh, sau đó kích hoạt các đoạn code load data tương ứng với State đó.

### [`GameController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/GameController.cs)
- **Chức năng:** Khi ứng dụng ở State "Game", Controller này sẽ nắm quyền. Nó điều hành vòng lặp thời gian của trò chơi (Day / Night cycle).
- **Hoạt động:**
  - `StartTurn()` / `EndTurn()`: Bắt đầu và kết thúc các Phase (Production -> Deployment -> Night Enemy -> Night Player).
  - `EndNightIfNeeded()`: Kiểm tra nếu đêm đã kết thúc -> Gọi `GameManager.ExileAllEnemies()` (Xóa sổ quái còn sót), tính toán Panic, tính Trophies (Thành tựu), tính Night Report, và chuyển sang bình minh.
  - `TriggerGameOver(cause)`: Xử lý logic khi nhà chính nổ hoặc thắng game, gọi tới màn hình GameOver, tính Exp mang về Meta, và xoá file Save.
- **Giao tiếp:** Nó giống như một Nhạc trưởng. Ở mỗi đầu/cuối Turn, nó sẽ phát ra sự kiện toàn cục: `EffectTimeEventManager.Instance.InvokeEvent(E_EffectTime.OnStartTurn)`.

---

## 2. Core Game Systems (Các hệ thống độc lập trong trận)

Đây là các Controller quản lý những tính năng riêng biệt, không thuộc Unit hay Skill.

### [`FogController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/FogController.cs)
- **Chức năng:** Quản lý cơ chế Sương Mù (Fog of War) - đặc trưng của game.
- **Hoạt động:**
  - `IncreaseDensity()` / `DecreaseDensity()`: Tăng giảm độ dày của sương theo cơ chế game (ví dụ dùng Seer).
  - `SetFogTiles()`: Tính toán toán học xem ô (Tile) nào cách xa nhà chính quá giới hạn -> Đánh dấu ô đó có `HazardDefinition.E_HazardType.Fog`.
  - Quản lý **Light Fog** (Sương mù ánh sáng/ma thuật) do các trụ đèn hoặc quái đặc biệt tạo ra (`LightFogSuppliers`).

### [`NightReportController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/NightReportController.cs)
- **Chức năng:** Tính toán thành tích sau mỗi đêm phòng thủ để trả về màn hình tổng kết (S, A, B, C, D).
- **Hoạt động:**
  - Đọc `NightReport.TonightHpLost` (Lượng máu bị mất của cả team).
  - So sánh tỷ lệ máu mất / máu tối đa với bảng `GameDatabase.NightReportRankDefinitions` để ra `BattleRank`.
  - Lấy `PanicManager.Panic.Level` để ra `PanicRank`.
  - Tính trung bình cộng để ra `TonightRank` (Rank tổng kết đêm), từ đó quyết định lượng Gold/Vật liệu thưởng cho người chơi.

### [`CursorController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/CursorController.cs)
- **Chức năng:** Quản lý vị trí con trỏ chuột trên bản đồ ô vuông.
- **Hoạt động:**
  - Lưu trữ `PreviousTile` và `CurrentTile` của chuột.
  - Tự động thay đổi màu/hướng của vùng chọn (Indicator) khi người chơi đang định cast một Skill (hỗ trợ CanRotate, CanFlip).

---

## 3. Core Interfaces (Giao diện chuẩn mực)

Đây là các bản thiết kế (blueprint) bắt buộc các thực thể trong game (Unit, Building) phải tuân theo nếu muốn tham gia vào combat.

### [`IDamageableController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/IDamageableController.cs)
- **Mô tả:** Bất cứ thứ gì có máu và có thể bị đánh (Hero, Quái, Tường thành, Nhà dân) đều phải implement interface này.
- **Hàm bắt buộc:**
  - `LoseHealth()`, `LoseArmor()`, `GainHealth()`.
  - `OnAttackDataComputed()`: Nhận dữ liệu của cú đánh trước khi trừ máu để tính giáp/né tránh.

### [`ISkillCasterController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/ISkillCasterController.cs)
- **Mô tả:** Bất cứ thứ gì có thể tung chiêu (Hero, Quái, Súng Ballista tự động) đều phải implement interface này.

### [`ITileObjectController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/ITileObjectController.cs)
- **Mô tả:** Mọi vật thể chiếm chỗ trên bản đồ Grid (Tile) đều phải có controller này để game biết đường tính toán che khuất (Line of Sight) hoặc va chạm.

### [`IBehaviorController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/IBehaviorController.cs)
- **Mô tả:** Định nghĩa cho AI. Kẻ thù, hoặc tháp canh tự bắn, sẽ dùng controller này để quyết định "hành động tiếp theo là gì".

---

## 4. Systems & Loaders (Công cụ nạp dữ liệu / Xác suất)

Các class hỗ trợ tải cảnh hoặc tính toán toán học nền tảng.

### [`MainMenuSceneLoader.cs`](file:///d:/Game_Complier/TheLastStand.Controller/MainMenuSceneLoader.cs) & [`SceneByTypeLoader.cs`](file:///d:/Game_Complier/TheLastStand.Controller/SceneByTypeLoader.cs)
- Dùng để bất đồng bộ (async load) các Scene của Unity.

### [`LevelProbabilitiesTreeController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/LevelProbabilitiesTreeController.cs) & [`RarityProbabilitiesTreeController.cs`](file:///d:/Game_Complier/TheLastStand.Controller/RarityProbabilitiesTreeController.cs)
- **Chức năng:** Thuật toán Gacha/RNG của game.
- **Hoạt động:** Xây dựng một cây xác suất (Probability Tree) dựa trên XML config để quyết định xem: Đêm nay rơi ra vũ khí Level mấy? Tỷ lệ ra đồ Tím (Epic) hay đồ Cam (Legendary) là bao nhiêu dựa trên số ngày đã sống sót và các Meta Upgrades.

---

## Tóm tắt
Nếu các sub-namespaces (Unit, Skill, Perk) là "từng cá nhân, từng binh lính", thì thư mục gốc `TheLastStand.Controller` chứa những "Vị thần" của trò chơi: **Quản lý thời gian, quản lý không gian (sương mù), quản lý luật lệ vật lý (IDamageable, Caster), và phân xử kết quả (NightReport).**
