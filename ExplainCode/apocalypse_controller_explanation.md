# Phân tích `ApocalypseController.cs`

File `ApocalypseController.cs` đóng vai trò là "Bộ não trung tâm" xử lý toàn bộ các hình phạt (Debuffs / Modifiers) của chế độ Tận Thế (Apocalypse) lên trò chơi. 

Đoạn code bạn đang xem (từ dòng 133 trở đi) thuộc về một trong những quy trình quan trọng nhất của class này: **Quy trình Tính toán & Tổng hợp hiệu ứng (Compute).**

## 1. Phương thức khởi tạo hàng loạt (Batch Computation)

```csharp
private void ComputeAll() // (Tên giả định dựa trên bối cảnh dòng 133)
{
    ComputeAffixesFlags();
    ComputeBuildingsDeadZoneRangeModifiers();
    ComputeBuildingsNotDemolishable();
    // ...
    ComputeSkillProgressionFlags();
}
```

Thay vì mỗi khi cần kiểm tra một hiệu ứng, game lại phải duyệt qua toàn bộ danh sách Modifier (việc này rất tốn hiệu năng, nhất là trong các hàm tính toán sát thương thời gian thực), nhà phát triển đã chọn cách **Tổng hợp trước dữ liệu (Pre-compute / Cache)**. 

Hàm này thường được gọi một lần khi load map hoặc khi bắt đầu một Run mới. Nhiệm vụ của nó là gom nhóm các hiệu ứng rải rác lại thành các từ điển (`Dictionary`) hoặc danh sách (`List`) để các hệ thống khác tra cứu tức thì (O(1)).

## 2. Cách các hàm `Compute...` hoạt động

Cấu trúc chung của tất cả các hàm `Compute` này đều tuân theo mô hình 3 bước (như trong `ComputeBuildingsDeadZoneRangeModifiers`):

### Bước 1: Xóa dữ liệu cũ
```csharp
Apocalypse.BuildingsModifiedDeadZoneRange.Clear();
```
Xóa sạch từ điển lưu trữ (nằm trong data model `Apocalypse`) để đảm bảo không bị dính dữ liệu từ vòng chơi (Run) trước đó.

### Bước 2: Duyệt qua tất cả các hiệu ứng đang kích hoạt
```csharp
foreach (ApocalypseEffectDefinition allEffect in Apocalypse.AllEffects)
```
Duyệt qua danh sách TẤT CẢ các hiệu ứng (Effects) đã được kích hoạt từ các Modifier mà người chơi chọn.

### Bước 3: Lọc (Cast) và Cập nhật Model
```csharp
// Kiểm tra xem hiệu ứng hiện tại có phải là loại cần tìm hay không (Sử dụng Pattern Matching của C#)
if (!(allEffect is ModifyBuildingsDeadZoneRangeApocalypseEffectDefinition modifyBuildingsDeadZoneRangeApocalypseEffectDefinition))
{
    continue; // Bỏ qua nếu không phải
}

// Nếu đúng, cập nhật thông số vào Model
foreach (string buildingsId in modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.BuildingsIds)
{
    if (Apocalypse.BuildingsModifiedDeadZoneRange.ContainsKey(buildingsId))
        Apocalypse.BuildingsModifiedDeadZoneRange[buildingsId] = modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range;
    else
        Apocalypse.BuildingsModifiedDeadZoneRange.Add(buildingsId, modifyBuildingsDeadZoneRangeApocalypseEffectDefinition.Range);
}
```

## 3. Một số hệ thống chịu ảnh hưởng trực tiếp (Từ dòng 134-149)

Dựa vào tên các hàm được gọi, ta có thể thấy Apocalypse Mode can thiệp rất sâu vào Core Gameplay của The Last Stand. Cụ thể:

*   **Vật phẩm (Items):** `ComputeAffixesFlags()` -> Can thiệp vào dòng chỉ số (Affix) khi rớt đồ. Quái có thể rớt đồ bị nguyền rủa (Negative Affixes).
*   **Xây dựng (Buildings):** 
    *   `ComputeBuildingsDeadZoneRangeModifiers()` -> Chỉnh sửa vùng cấm xây dựng (Dead zone) quanh các nhà.
    *   `ComputeBuildingsNotDemolishable()` -> Cấm phá hủy một số nhà nhất định để thu hồi vốn.
*   **Kẻ địch (Enemies):** 
    *   `ComputeEnemiesInjuryStageStatModifiers()` -> Quái vật có thể hung hãn hơn (nhận buff) khi bị thương.
    *   `ComputeEnemiesSpawnWaveWeightMultiplier()` -> Tăng số lượng/Trọng lượng của đợt quái vật.
    *   `ComputeEnemiesStatsBaseValueModifiers()` -> Tăng Máu, Sát thương cơ bản.
*   **Sương mù (Fog):** `ComputeFogSpawnersMultiplier()` -> Cửa sinh quái (Fog Spawner) đẩy lùi nhanh hơn hoặc sinh nhiều hơn.
*   **Hero (Playable Unit):** 
    *   `ComputePlayableUnitBlockLineOfSight()` -> Hero chắn tầm nhìn của nhau (rất nguy hiểm cho Archer).
    *   `ComputeRemoveStartingPlayableUnit()` -> Xóa bớt số lượng Hero khởi đầu.

> [!NOTE]
> **Kiến trúc dữ liệu:** `ApocalypseController` không tự lưu trữ các giá trị này. Nó chỉ là **Controller** (người điều phối). Nơi thực sự lưu trữ các giá trị sau khi tính toán xong là Class Model tên là `Apocalypse` (có thể truy cập thông qua Singleton hoặc reference). Các Controller khác (như `UnitController` hay `BuildingController`) sẽ đọc dữ liệu từ Model `Apocalypse` này để áp dụng buff/debuff.
