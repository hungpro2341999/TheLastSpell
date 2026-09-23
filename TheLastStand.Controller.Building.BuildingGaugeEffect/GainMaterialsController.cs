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
/// Controller xử lý hiệu ứng "Tạo Vật Liệu theo Chu kỳ" (Gain Materials Gauge Effect) từ thanh tiến độ của công trình (ví dụ: Xưởng cưa, Mỏ đá).
/// Khi tích lũy đủ điểm sản xuất:
/// - Tính toán số lượng vật liệu sản sinh dựa trên cấp độ công trình (ComputeMaterialsValue).
/// - Gửi số liệu báo cáo qua Analytics.
/// - Cộng vật liệu trực tiếp vào kho tài nguyên dùng chung (ResourceManager.Materials).
/// - Khởi tạo animation hiển thị số vật liệu bay lên (GainMaterialDisplay).
/// </summary>
public class GainMaterialsController : BuildingGaugeEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu hiệu ứng sinh vật liệu theo tiến độ.
	/// </summary>
	private GainMaterials GainMaterials => base.BuildingGaugeEffect as GainMaterials;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller từ dữ liệu đã lưu (Save game).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa trạng thái tiến độ tích lũy.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng sinh vật liệu.</param>
	public GainMaterialsController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainMaterials(productionBuilding, definition, this, new GainMaterialsView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	/// <summary>
	/// Khởi tạo mới controller khi công trình được xây dựng lần đầu.
	/// </summary>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng sinh vật liệu.</param>
	public GainMaterialsController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new GainMaterials(productionBuilding, definition, this, new GainMaterialsView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	#endregion

	#region Trigger Effect Execution

	/// <summary>
	/// Kích hoạt hiệu ứng hoàn tất thanh sản xuất vật liệu:
	/// 1. Tính toán lượng vật liệu sản xuất nhận được qua ComputeMaterialsValue().
	/// 2. Gửi sự kiện Analytics ghi nhận sản lượng vật liệu.
	/// 3. Cộng trực tiếp vào ResourceManager.Instance.Materials.
	/// 4. Tạo và hiển thị Prefab GainMaterialDisplay trên đầu công trình.
	/// </summary>
	/// <returns>Danh sách controller mục tiêu cho diễn hoạt animation.</returns>
	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		
		// Tính toán lượng vật liệu tạo ra
		int num = GainMaterials.ComputeMaterialsValue();
		if (Analytics.AllowedToSendData)
		{
			Analytics.SendMaterialBuildingEvent(num);
		}
		
		// Cộng vật liệu vào kho tài nguyên dùng chung
		TPSingleton<ResourceManager>.Instance.Materials += num;
		
		// Hiển thị animation số vật liệu nhận được (+Materials) trên công trình
		GainMaterialDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(num);
		base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		list.Add(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController);
		TPSingleton<BuildingManager>.Instance.Log($"({base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id}) Material gain (+{num})", CLogLevel.MAJOR);
		return list;
	}

	#endregion
}

