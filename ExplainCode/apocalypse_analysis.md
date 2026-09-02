# Phân tích Chi tiết: Thư mục `TheLastStand.Controller.Apocalypse`

Thư mục này quản lý hệ thống **Apocalypse Mode**. Trong The Last Stand, Apocalypse không chỉ là một thanh kéo độ khó (Easy/Normal/Hard) mà là một hệ thống **Custom Modifiers (Mutators)** giống như cơ chế "Heat" trong *Hades* hoặc "Ascension" trong *Slay the Spire*. Người chơi có thể tự do bật/tắt các "lời nguyền" (Modifiers) để tăng độ khó và nhận thêm phần thưởng.

Thư mục này chứa 3 class chính, mỗi class đảm nhiệm một vai trò chuyên biệt.

---

## 1. [`ApocalypseController.cs`](file:///d:/Game_Complier/TheLastStand.Controller.Apocalypse/ApocalypseController.cs) — Trái tim của chế độ Khải Huyền

Đây là Controller cốt lõi, chịu trách nhiệm theo dõi xem người chơi đang chọn những Modifier nào và áp dụng chúng vào luật chơi.

### Cơ chế hoạt động:
- **Lưu trữ Modifier:** Nó chứa một danh sách các `ApocalypseModifierStepDefinition` (các bước/level của từng modifier). Ví dụ: Modifier tăng máu quái có 3 step (+10%, +25%, +50%).
- **Tính toán Apocalypse Level tổng:** Gọi hàm `ComputeCurrentLevel()`. Mỗi modifier step sẽ có một điểm số (ApocalypseLevel). Tổng điểm của tất cả modifier đang bật chính là Apocalypse Level hiện tại của run đó.
- **Áp dụng Effects (Global Side-effects):** Đây là phần quan trọng nhất. Thay vì bắt các hệ thống khác phải đi hỏi `ApocalypseController` xem luật có gì thay đổi, Controller này sẽ tính toán sẵn các "biến tổng" và lưu trữ vào Model `TheLastStand.Model.Apocalypse.Apocalypse`.
  
  Hàm `ComputeEffectsModifiers()` sẽ lặp qua tất cả Modifier đang bật và tính toán các ảnh hưởng toàn cục, ví dụ:
  - `ComputeEnemiesStatsBaseValueModifiers()`: Tổng hợp chỉ số cộng thêm cho quái.
  - `ComputePanicGainMultiplier()`: Tính hệ số nhân lượng Panic nhận được (VD: +20% Panic).
  - `ComputeBuildingsNotDemolishable()`: Khóa chức năng phá hủy của một số nhà cửa.
  - `ComputeFogSpawnersMultiplier()`: Tăng số lượng điểm đẻ sương mù.
  - `ComputePlayableUnitBlockLineOfSight()`: Kích hoạt luật "Hero cản tầm nhìn của nhau" (Friendly Fire LoS).

> **Pattern:** Tiền xử lý (Pre-computation). `ApocalypseController` xử lý hết các phép cộng dồn từ Modifier ngay từ đầu (hoặc khi đổi cấu hình), các hệ thống khác trong game chỉ việc vào Model lấy con số cuối cùng (ví dụ: `Apocalypse.PanicGainMultiplier`) để tính toán.

---

## 2. [`ApocalypseCodeGenerator.cs`](file:///d:/Game_Complier/TheLastStand.Controller.Apocalypse/ApocalypseCodeGenerator.cs) — Hệ thống chia sẻ độ khó

The Last Stand cho phép người chơi copy một đoạn mã (Code) đại diện cho cấu hình Apocalypse hiện tại để chia sẻ cho bạn bè hoặc cộng đồng. Class này đảm nhiệm việc **Encode** (Tạo mã) và **Decode** (Giải mã).

### Cấu trúc Mã (Code Format):
- Dựa vào regex `"([A-Z]+[0-9]+)"`, ta thấy mã có dạng chữ + số liền nhau. 
- Ví dụ: `HP2DMG1FOG3` 
  - `HP` (CodeSharingId) + `2` (StepIndex) -> Máu quái cấp 2.
  - `DMG` + `1` -> Đam quái cấp 1.
  - `FOG` + `3` -> Sương mù cấp 3.

### Logic Giải mã (`TryDecodeCode`):
Khi người chơi paste một đoạn code vào, hệ thống thực hiện kiểm tra bảo mật rất chặt chẽ:
1. Regex check xem format có đúng không (`E_FailureReason.InvalidCodeFormat`).
2. Tách từng cụm (VD: `HP2`).
3. Dò `HP` xem có tồn tại trong Database không (`InvalidModifierCodeSharingId`).
4. Kiểm tra xem người chơi này **đã unlock (mở khóa)** Modifier `HP` chưa thông qua `ApocalypseManager.IsModifierUnlocked()`. (Chống gian lận: không thể mượn code của người khác để chơi độ khó mình chưa mở khóa).
5. Kiểm tra xem Step `2` có hợp lệ không (`InvalidStepIndex`).
6. Kiểm tra xem có bị trùng lặp không (VD: `HP1HP2` -> `DuplicateModifier`).

---

## 3. [`ApocalypseRetroCompatibilityController.cs`](file:///d:/Game_Complier/TheLastStand.Controller.Apocalypse/ApocalypseRetroCompatibilityController.cs) — Khả năng tương thích ngược (Migration)

Nếu nhìn vào logic của file này, chúng ta có thể đoán được lịch sử phát triển của The Last Stand:
- **Ngày xưa:** Apocalypse là một hệ thống tuyến tính. Người chơi cứ vượt qua Level 1 thì mở Level 2, vượt Level 2 mở Level 3 (lên đến 6). Ở mỗi cấp, game tự động nhét thêm độ khó cố định.
- **Bây giờ:** Đổi thành hệ thống Custom Modifiers (chọn món tự do).

Vậy chuyện gì xảy ra với những file Save cũ của người chơi từ phiên bản trước? Class này sinh ra để xử lý việc đó (Migration).

### Cơ chế Migration:
- Nếu `saveVersion <= 23` (cho save trận đấu) hoặc `<= 13` (cho save tiến trình thành phố), nó sẽ kích hoạt.
- Hàm `InitializeIfNeeded()` chứa "Bảng phiên dịch":
  - Cấp độ cũ 1 = Tương đương Modifier "EnemyHealthModifier" (Máu quái) cấp 0.
  - Cấp độ cũ 2 = Máu quái cấp 0 + "WaveSizeModifier" (Số lượng quái) cấp 1.
  - Cấp độ cũ 4 = ... + "ProductionCostModifier" cấp 1 + "DefenseCostModifier" cấp 0.
- Sau khi phiên dịch, nó sẽ âm thầm tự động bật các Modifier tương ứng cho người chơi để họ giữ nguyên mức thử thách đang chơi dở mà không bị lỗi game.

---

## Tóm tắt

Thư mục `Apocalypse` là một ví dụ mẫu mực về thiết kế hệ thống **Mutators** trong game:
1. **Controller Tính toán (`ApocalypseController`)**: Gom nhóm hiệu ứng, tính sẵn hệ số, giúp game tối ưu hiệu năng (các hệ thống khác không cần phải lặp qua danh sách Modifier mỗi frame).
2. **Hệ thống Xã hội (`ApocalypseCodeGenerator`)**: Cho phép chuyển đổi cấu hình thành chuỗi String ngắn gọn để chia sẻ, đi kèm Validation chống cheat.
3. **Hệ thống Kế thừa (`ApocalypseRetroCompatibilityController`)**: Đảm bảo trải nghiệm xuyên suốt qua các bản cập nhật lớn của game mà không làm hỏng file save của người chơi.
