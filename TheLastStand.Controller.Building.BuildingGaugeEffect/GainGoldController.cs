using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

/// <summary>
/// Controller xử lý hiệu ứng "Tạo Vàng theo Chu kỳ" (Gain Gold Gauge Effect) từ thanh tiến độ của công trình (ví dụ: Mỏ vàng / Gold Mine).
/// Khi tích lũy đủ điểm sản xuất:
/// - Tính toán giá trị vàng sản sinh dựa trên cấp độ và thông số định nghĩa (ComputeGoldValue).
/// - Gửi số liệu báo cáo qua Analytics (nếu được phép).
/// - Cộng vàng trực tiếp vào kho tài nguyên tổng của người chơi (ResourceManager.Gold).
/// - Khởi tạo animation hiển thị số vàng bay lên (GainGoldDisplay).
/// </summary>
public class GainGoldController : BuildingGaugeEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu hiệu ứng sinh vàng theo tiến độ.
	/// </summary>
	public GainGold GainGold => base.BuildingGaugeEffect as GainGold;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller từ dữ liệu lưu trữ (Save game).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa trạng thái tiến độ tích lũy.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng sinh vàng.</param>
	public GainGoldController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainGold(productionBuilding, definition, this, new GainGoldView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	/// <summary>
	/// Khởi tạo mới controller khi công trình được xây dựng lần đầu.
	/// </summary>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng sinh vàng.</param>
	public GainGoldController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainGold(productionBuilding, definition, this, new GainGoldView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	#endregion

	#region Trigger Effect Execution

	/// <summary>
	/// Kích hoạt hiệu ứng hoàn tất thanh sản xuất vàng:
	/// 1. Tính toán lượng vàng thưởng nhận được qua hàm ComputeGoldValue().
	/// 2. Ghi nhận dữ liệu Analytics sự kiện nhận vàng từ công trình.
	/// 3. Cộng trực tiếp vào ResourceManager.Instance.Gold.
	/// 4. Tạo và hiển thị Prefab GainGoldDisplay nổi bật trên đầu công trình.
	/// </summary>
	/// <returns>Danh sách controller mục tiêu cho diễn hoạt animation.</returns>
	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		
		// Tính lượng vàng sản sinh (có tính toán theo cấp độ và hệ số nhân)
		int num = GainGold.ComputeGoldValue();
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendGoldBuildingEvent(num);
		}
		
		// Cộng vàng vào kho tài nguyên dùng chung
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + num);
		
		// Hiển thị animation số vàng nhận được (+Gold) bay lên từ vị trí công trình
		GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(num);
		base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		list.Add(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController);
		TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id}) Gold gain (+{num})", CLogLevel.MAJOR);
		return list;
	}

	#endregion
}

