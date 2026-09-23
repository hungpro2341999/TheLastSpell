using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingGaugeEffect;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

/// <summary>
/// Controller cơ sở trừu tượng (Base Controller) quản lý hiệu ứng khi thanh tiến độ sản xuất (Production Gauge) của công trình đạt mốc tối đa.
/// Trong The Last Spell, các công trình sản xuất tích lũy điểm sản xuất (Units) qua từng đêm hoặc thông qua hành động nạp điểm;
/// khi đạt mốc ngưỡng (UnitsThreshold), controller này sẽ kích hoạt hiệu ứng hoàn thành sản xuất (nhận vàng, vật liệu, tạo trang bị, tăng chỉ số, v.v.).
/// </summary>
public abstract class BuildingGaugeEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model lưu trữ dữ liệu thanh tiến độ sản xuất của công trình (điểm tích lũy hiện tại, mốc ngưỡng, v.v.).
	/// </summary>
	public TheLastStand.Model.Building.BuildingGaugeEffect.BuildingGaugeEffect BuildingGaugeEffect { get; protected set; }

	#endregion

	#region Trigger Verification & Execution

	/// <summary>
	/// Kiểm tra xem hiệu ứng thanh tiến độ đã đủ điều kiện để kích hoạt hay chưa.
	/// Mặc định: Điểm sản xuất tích lũy (Units) phải lớn hơn hoặc bằng mốc ngưỡng (UnitsThreshold).
	/// </summary>
	/// <returns>True nếu thanh tiến độ đã đầy và có thể trả thưởng, ngược lại False.</returns>
	public virtual bool CanTriggerEffect()
	{
		return BuildingGaugeEffect.Units >= BuildingGaugeEffect.UnitsThreshold;
	}

	/// <summary>
	/// Kích hoạt hiệu ứng hoàn thành sản xuất:
	/// - Nếu không phải là Vòng phong ấn ma thuật (MagicCircle), thêm câu thoại thoại ngẫu nhiên (Bark) thông báo hoàn thành sản xuất ("BuildingGaugeCompletion").
	/// - Trả về danh sách các controller mục tiêu để phục vụ cho các hiệu ứng diễn hoạt thị giác / camera.
	/// </summary>
	/// <returns>Danh sách IEffectTargetSkillActionController chịu tác động của hiệu ứng.</returns>
	public virtual List<IEffectTargetSkillActionController> TriggerEffect()
	{
		// Nếu công trình không phải là Magic Circle, hiển thị câu thoại thông báo công trình đã hoàn thành sản phẩm
		if (!(BuildingGaugeEffect.ProductionBuilding.BuildingParent is MagicCircle))
		{
			TPSingleton<BarkManager>.Instance.AddPotentialBark("BuildingGaugeCompletion", BuildingGaugeEffect.ProductionBuilding.BuildingParent.BlueprintModule, 0f);
			TPSingleton<BarkManager>.Instance.Display();
		}
		return new List<IEffectTargetSkillActionController>();
	}

	#endregion
}

