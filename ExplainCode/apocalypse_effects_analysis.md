# Phân tích Chi tiết: Thư mục `TheLastStand.Controller.Apocalypse.ApocalypseEffects`

Thư mục này là một thư mục con bên trong `Apocalypse`, chuyên dùng để chứa các Controller cho từng hiệu ứng (Effect) cụ thể của chế độ Apocalypse. Tuy nhiên, điều kỳ lạ là nó **chỉ chứa đúng 2 file**:

1. `AApocalypseEffectController.cs` (Class cơ sở trừu tượng - Abstract Base Class)
2. `AddEnemiesStatModifierFromTurnApocalypseEffectController.cs` (Một implementation duy nhất)

---

## 1. Tại sao chỉ có một Effect Controller duy nhất?

Như đã phân tích ở file `ApocalypseController.cs` trước đó, hệ thống Apocalypse có hàng chục hiệu ứng khác nhau (Tăng máu quái, Tăng tốc độ quái, Tăng điểm Panic, Khóa nhà cửa, v.v.). 

Tuy nhiên, **hầu hết các hiệu ứng này là hiệu ứng Bị động (Passive)**. 
- Chúng chỉ là những con số tĩnh được định nghĩa trong Data (Model/Definition).
- `ApocalypseController` sẽ tự động đọc các con số này một lần duy nhất lúc bắt đầu game và cộng dồn lại (ví dụ tính ra `PanicGainMultiplier = 1.2f`).
- Các hệ thống khác tự đến lấy con số đó để dùng. Bản thân các Effect bị động không cần bất kỳ logic điều khiển (Controller) nào cả.

**Ngoại lệ duy nhất là `AddEnemiesStatModifierFromTurn`**. Đây là một hiệu ứng **Chủ động (Active & Dynamic)**.
- Hiệu ứng này không áp dụng ngay từ đầu đêm, mà nó chờ đến một **Turn (Lượt / Giờ cụ thể trong đêm)** thì mới đột ngột kích hoạt.
- Do nó thay đổi trạng thái theo thời gian thực (Stateful), nó bắt buộc phải có một Controller riêng để "lắng nghe" các sự kiện chuyển Turn và tính toán logic buff/debuff chỉ số quái vật ngay giữa trận đấu.

---

## 2. Phân tích Code

### [`AApocalypseEffectController.cs`](file:///d:/Game_Complier/TheLastStand.Controller.Apocalypse.ApocalypseEffects/AApocalypseEffectController.cs)
- Lớp cha trừu tượng (Abstract). 
- Chỉ đơn giản là chứa reference tới Model `AApocalypseEffect` và định nghĩa 2 hàm ảo `OnActivation(bool onLoad)` và `OnDeactivation(bool onLoad)` để các class con override (ghi đè).

### [`AddEnemiesStatModifierFromTurnApocalypseEffectController.cs`](file:///d:/Game_Complier/TheLastStand.Controller.Apocalypse.ApocalypseEffects/AddEnemiesStatModifierFromTurnApocalypseEffectController.cs)
- Chịu trách nhiệm quản lý hiệu ứng: **"Đột nhiên tăng/giảm chỉ số của toàn bộ quái vật kể từ Turn thứ N"**.
- **Cơ chế Event-Driven:**
  - Hàm `HookActivationConditions()` đăng ký (subscribe) vào sự kiện `ApocalypseManager.OnStatModifierFromTurnCheck`.
  - Hàm `HookDeactivationConditions()` đăng ký vào sự kiện `ApocalypseManager.OnDeactivateModifiersWithTurnConditions`.
- **Cơ chế kích hoạt (`CheckIfCanActivate`):**
  - Đọc `GameManager.Instance.Game.CurrentNightHour` (Giờ hiện tại của đêm).
  - So sánh với điều kiện của Effect. Nếu thỏa mãn -> gọi `Activate()`.
- **Cơ chế áp dụng chỉ số (`OnActivation` / `OnDeactivation`):**
  - Không chỉ cập nhật biến cache toàn cục `ActiveStatModifierFromTurn` trong `ApocalypseManager`.
  - Nó còn thực sự **duyệt qua toàn bộ quái vật và Boss đang sống trên bản đồ** (`EnemyUnitManager.EnemyUnits` và `BossManager.BossUnits`), trực tiếp can thiệp vào `EnemyUnitStatsController` để cộng/trừ chỉ số (ví dụ tăng giáp, tăng đam) ngay lập tức.
  - Đồng thời cập nhật cả `ChildStat` (chỉ số gốc) và `stat.Apocalypse` (chỉ số do khải huyền cộng thêm) để tách biệt rõ ràng khi tính toán UI hiển thị cho người chơi.

## Tổng kết
Thư mục `ApocalypseEffects` chứng minh nguyên tắc **KISS (Keep It Simple, Stupid)** của nhà phát triển: Nếu một Data Modifier chỉ là số tĩnh, thì đừng viết Controller cho nó. Chỉ viết Controller (như `AddEnemiesStatModifierFromTurnApocalypseEffectController`) khi tính năng đó thực sự cần chạy logic theo thời gian thực (Dynamic/Event-Driven).
