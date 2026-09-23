using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

/// <summary>
/// Controller xử lý hiệu ứng "Tạo Vật Phẩm / Trang Bị" (Create Item Gauge Effect) khi thanh tiến trình sản xuất đạt mốc.
/// Áp dụng cho các công trình như Nhà rèn (Blacksmith), Tiệm cung thủ (Bowyer), Xưởng may ma thuật (Armorer/Mage shop):
/// - Khi tích đủ điểm sản xuất, hệ thống sẽ tự động tạo ra một nhóm vật phẩm trang bị theo tỷ lệ xác suất (Probabilities Tree).
/// - Hiển thị animation thông báo nhận đồ (CreateItemDisplay).
/// - Đưa các vật phẩm này vào Báo cáo sản xuất ban ngày (Production Report) để người chơi tự do lựa chọn trang bị thưởng.
/// </summary>
public class CreateItemGaugeEffectController : BuildingGaugeEffectController
{
	#region Constants

	/// <summary>
	/// Các hằng số cấu hình đường dẫn tài nguyên Prefab.
	/// </summary>
	public static class Constants
	{
		public const string CreateItemDisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/CreateItemDisplay";
	}

	#endregion

	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu hiệu ứng tạo vật phẩm từ tiến trình công trình.
	/// </summary>
	public CreateItemGaugeEffect CreateItem => base.BuildingGaugeEffect as CreateItemGaugeEffect;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller từ dữ liệu đã lưu (Save game / Deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa trạng thái thanh tiến độ.</param>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng tạo vật phẩm.</param>
	public CreateItemGaugeEffectController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new CreateItemGaugeEffect(productionBuilding, definition, this, new CreateItemView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	/// <summary>
	/// Khởi tạo mới controller khi công trình được xây dựng hoặc thiết lập lần đầu.
	/// </summary>
	/// <param name="productionBuilding">Module sản xuất của công trình liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng tạo vật phẩm.</param>
	public CreateItemGaugeEffectController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new CreateItemGaugeEffect(productionBuilding, definition, this, new CreateItemView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	#endregion

	#region Trigger Effect Execution

	/// <summary>
	/// Kích hoạt hiệu ứng hoàn thành thanh sản xuất trang bị:
	/// 1. Sinh danh sách các vật phẩm (Item) theo cấp độ tính toán từ cây xác suất (GenerationProbabilitiesTree).
	/// 2. Khởi tạo đối tượng ProductionItems chứa các lựa chọn phần thưởng.
	/// 3. Lấy Prefab CreateItemDisplay từ ObjectPooler và hiển thị trên đầu công trình.
	/// 4. Đưa danh sách trang bị vào ProductionReport để hiển thị trong bảng tổng kết sản xuất buổi sáng.
	/// </summary>
	/// <returns>Danh sách controller mục tiêu cho diễn hoạt animation.</returns>
	public override List<IEffectTargetSkillActionController> TriggerEffect()
	{
		List<IEffectTargetSkillActionController> list = base.TriggerEffect();
		CreateItemGaugeEffectDefinition createItemGaugeEffectDefinition = CreateItem.CreateItemGaugeEffectDefinition;
		
		// Khởi tạo container sản phẩm thưởng dựa trên cấp độ nâng cấp sản xuất toàn cục
		ProductionItems productionItem = new ProductionItemController(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition, TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel.Level).ProductionItem;
		productionItem.IsNightProduction = false;
		int prodRewardsCount = TPSingleton<ItemManager>.Instance.ProdRewardsCount;
		
		// Tạo số lượng vật phẩm tương ứng với số lựa chọn thưởng mà người chơi được nhận
		for (int i = 0; i < prodRewardsCount; i++)
		{
			TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.None, createItemGaugeEffectDefinition.CreateItemDefinition, CreateItem.GenerationProbabilitiesTree.GenerateLevel());
			productionItem.Items.Add(item);
			TPSingleton<ItemManager>.Instance.Log("(" + base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id + ") Item created: " + item.ItemDefinition.Id + ".", CLogLevel.MAJOR);
		}
		
		// Hiển thị hiệu ứng đồ họa icon vật phẩm bay lên tại công trình
		CreateItemDisplay pooledComponent = ObjectPooler.GetPooledComponent("CreateItemDisplay", ResourcePooler.LoadOnce<CreateItemDisplay>("Prefab/Displayable Effect/UI Effect Displays/CreateItemDisplay"), EffectManager.EffectDisplaysParent);
		pooledComponent.Init(productionItem);
		base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		list.Add(base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController);
		
		// Ghi nhận vào Báo cáo sản xuất tổng thể
		TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
		return list;
	}

	#endregion
}

