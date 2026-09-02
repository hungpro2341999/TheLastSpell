# Phân tích Chi tiết: Thư mục `TheLastStand.Controller.Building.BuildingAction`

Thư mục này chịu trách nhiệm cho các **Hành động tương tác với Công trình (Building Actions)**. Trong The Last Stand, người chơi không chỉ xây nhà mà còn có thể bấm vào nhà để thực hiện các hành động (Ví dụ: bấm vào Đống đổ nát (Ruins) để Scavenge, bấm vào mỏ vàng để thu thập).

Hệ thống này được thiết kế theo mô hình **Command Pattern**, trong đó mỗi hành động được tách thành các `Effect` độc lập có thể ghép lại với nhau.

---

## 1. [`BuildingActionController.cs`](file:///d:/Game_Complier/TheLastStand.Controller.Building.BuildingAction/BuildingActionController.cs) — Bộ điều phối trung tâm

Đây là class quản lý việc **khi nào thì người chơi được bấm nút** và **điều gì xảy ra khi bấm nút**. Nó gắn liền với `ProductionModule` của một tòa nhà.

### Các cơ chế kiểm tra (Validation):
- **`CanExecuteAction()`**: Kiểm tra xem người chơi có đủ điều kiện thực hiện hành động này không.
  - Phải đủ số lượng Công nhân (`Workers`).
  - Phải còn số lần sử dụng trong Turn (`UsesPerTurnRemaining`).
  - Phải đúng Phase (`ProductionState`, `DeploymentState`, `NightState`).
  - Nếu là hành động đẩy lùi sương mù (`RepelFog`), phải check xem sương mù có thể lùi thêm được không.
- **`CanExecuteActionOnTile()`**: Một số hành động bắt buộc người chơi phải chọn mục tiêu (Target) trên bản đồ lưới (Tile). Hàm này sẽ loop qua tất cả các `BuildingActionEffect` để xem có thể cast chiêu lên ô đó không.

### Cơ chế thực thi (`ExecuteActionEffects`):
- Trừ số lượt sử dụng trong ngày.
- Trừ đi số lượng Công nhân (Workers) yêu cầu thông qua `ResourceManager.UseWorkers()`.
- Lặp qua vòng lặp `for` để gọi hàm `ExecuteActionEffect()` của tất cả các hiệu ứng (Effects) đính kèm với hành động này.
- Gọi hệ thống Hiệu ứng hình ảnh (VFX) thông qua `CastFxController` để play animation.

---

## 2. Hệ thống Effects (Building Action Effects)

Đây là nơi chứa logic thực sự của từng hành động. Bất cứ nút bấm nào của tòa nhà cũng được tạo ra bằng cách ghép 1 hoặc nhiều Effect này lại với nhau từ cấu hình XML. Các file trong thư mục này (như `GainGoldBuildingActionEffectController`, `HealBuildingActionEffectController`) đều kế thừa từ một class cha chung.

### Ví dụ điển hình: [`ScavengeBuildingActionEffectController.cs`](file:///d:/Game_Complier/TheLastStand.Controller.Building.BuildingAction/ScavengeBuildingActionEffectController.cs)

Đây là hành động "Lục lọi" (Scavenge) Đống đổ nát (Ruins / Bone Piles) rất phổ biến trong ngày 1 của game. Khi người chơi bấm nút Scavenge, đoạn code này sẽ chạy và thực hiện một chuỗi các hiệu ứng phụ (Side-effects):

1. **Phá hủy nhà:** Nó gọi `buildingParent.DamageableModuleController.LoseHealth()` để gây sát thương vật lý lên chính đống đổ nát đó (Mỗi lần lục lọi thì đống đổ nát sẽ bị hư hại đi).
2. **Cộng tài nguyên:** Cộng thẳng Vàng (`GainGold`) và Vật liệu (`GainMaterials`) vào `ResourceManager`.
3. **Mở khóa thành tựu:** Tăng tiến trình thành tựu `STAT_SCAVENGED_CORPSES_AND_RUINS_AMOUNT`.
4. **Báo cáo Analytics:** Bắn sự kiện lên máy chủ để nhà phát triển phân tích (`GameAnalytics.OnBonePileScavenged`).
5. **Gacha Rơi đồ (RNG Items):** Nếu cấu hình yêu cầu rơi vũ khí, nó sẽ khởi tạo `LevelProbabilitiesTreeController` để quay xổ số xem người chơi nhận được đồ cấp mấy, rồi ném vào Túi đồ (Inventory) và ghi vào báo cáo cuối ngày (`ProductionReport`).
6. **Tạo Feedback UI (Chữ nổi):** Khởi tạo các Prefab chữ nổi như `GainGoldDisplay` (+10 Vàng) hay `AttackFeedback` (Sát thương hiện lên đống đổ nát) thông qua `ObjectPooler` để tối ưu bộ nhớ.

### Các Effect phổ biến khác:
- `FillGauge`: Làm đầy thanh năng lượng của nhà.
- `Heal` / `HealMana`: Hồi máu hoặc Mana cho Unit đứng gần.
- `RepelFog`: Đẩy lùi sương mù bằng trụ đèn Seer.
- `RerollWave`: Reroll (quay lại) hướng quái vật tấn công.

---

## Tóm tắt

Hệ thống `BuildingAction` của The Last Stand là một ví dụ rất chuẩn mực về **Command Pattern** kết hợp với **Composition**. Thay vì hard-code "Nhà mỏ vàng thì có hàm Lấy vàng", "Đống đổ nát có hàm Lục lọi", hệ thống cho phép Design Team (người viết XML) tự do định nghĩa các hành động và lắp ráp các viên gạch (Effect) lại với nhau.

Khi người chơi ấn 1 nút, `BuildingActionController` sẽ đứng ra thu thuế (trừ Workers, trừ Turn), sau đó nó ra lệnh cho hàng loạt các `EffectController` bên dưới kích hoạt các hiệu ứng đa dạng như cộng tiền, rơi đồ, trừ máu công trình, hiển thị chữ UI.
