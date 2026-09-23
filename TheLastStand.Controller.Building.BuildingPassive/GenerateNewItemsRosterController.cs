using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển hiệu ứng nội tại sinh danh sách trang bị mới cho Cửa hàng (Generate New Items Roster).
/// Quản lý danh sách các quy tắc sinh đồ (CreateRosterItemControllers), kết hợp hiệu ứng nâng cấp vĩnh viễn (Meta Upgrades),
/// tính toán cấp độ ngẫu nhiên theo cây xác suất và bày bán vào Cửa hàng (Shop).
/// </summary>
public class GenerateNewItemsRosterController : BuildingPassiveEffectController
{
	#region Properties

	/// <summary>
	/// Danh sách các bộ điều khiển cấu hình tạo từng nhóm vật phẩm trong danh sách bán hàng.
	/// </summary>
	public List<CreateRosterItemController> CreateRosterItemControllers { get; } = new List<CreateRosterItemController>();

	/// <summary>
	/// Model lưu trữ định nghĩa và dữ liệu của hiệu ứng sinh danh sách đồ.
	/// </summary>
	public GenerateNewItemsRoster GenerateNewItemsRoster => base.BuildingPassiveEffect as GenerateNewItemsRoster;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo GenerateNewItemsRosterController: Đảm bảo công trình có module sản xuất,
	/// tạo đối tượng model và khởi tạo các CreateRosterItemController con.
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình.</param>
	/// <param name="buildingPassiveDefinition">Định nghĩa cấu hình sinh danh sách trang bị.</param>
	public GenerateNewItemsRosterController(PassivesModule buildingPassivesModule, GenerateNewItemsRosterDefinition buildingPassiveDefinition)
	{
		// Đảm bảo công trình cha sở hữu module sản xuất
		buildingPassivesModule.BuildingParent.TryCreateEmptyProductionModule();
		base.BuildingPassiveEffect = new GenerateNewItemsRoster(buildingPassivesModule, buildingPassiveDefinition, this);

		// Khởi tạo từng bộ điều khiển con cho từng nhóm item được định nghĩa
		foreach (CreateRosterItemDefinition createItemRosterDefinition in buildingPassiveDefinition.CreateItemRosterDefinitions)
		{
			CreateRosterItemControllers.Add(new CreateRosterItemController(buildingPassivesModule.BuildingParent.ProductionModule, createItemRosterDefinition));
		}
	}

	#endregion

	#region Passive Effect Execution

	/// <summary>
	/// Thực thi áp dụng hiệu ứng: Kích hoạt quy trình sinh toàn bộ trang bị mới vào Shop.
	/// </summary>
	public override void Apply()
	{
		GenerateItems();
	}

	/// <summary>
	/// Thuật toán sinh trang bị:
	/// Xóa sạch hàng cũ trong Shop -> Tính toán số lượng theo biểu thức và Meta Upgrades ->
	/// Sinh cấp độ đồ (Level) theo cây xác suất -> Đưa vào các slot Shop -> Sắp xếp lại quầy hàng.
	/// </summary>
	private void GenerateItems()
	{
		TPSingleton<ItemManager>.Instance.Log("#" + GetType().Name + ".#About to generate items for building " + base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent.Id + " !");
		
		// Xóa sạch các món đồ hiện có trong Shop
		TPSingleton<BuildingManager>.Instance.Shop.ShopController.ClearItems();

		int i = 0;
		for (int count = CreateRosterItemControllers.Count; i < count; i++)
		{
			Node count2 = CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition.Count;

			// Kiểm tra hiệu ứng sửa đổi số lượng từ Meta Upgrades
			if (MetaUpgradeEffectsController.TryGetEffectsOfType<CreateItemModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
			{
				for (int j = 0; j < effects.Length; j++)
				{
					if (CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition.HasID && effects[j].CreateItemId == CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition.Id)
					{
						count2 = effects[j].Count;
						break;
					}
				}
			}

			// Đánh giá số lượng vật phẩm cần tạo theo ngữ cảnh
			int num = count2.EvalToInt(new ItemInterpreterContext());
			for (int k = 0; k < num; k++)
			{
				// Rút ngẫu nhiên cấp độ vật phẩm (Level) từ cây xác suất
				int num2 = CreateRosterItemControllers[i].CreateRosterItem.GenerationProbabilitiesTree.GenerateLevel();
				TPSingleton<ItemManager>.Instance.Log($"#{GetType().Name}.#Generating one item of index {k} and level {num2}");
				ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.Shop, CreateRosterItemControllers[i].CreateRosterItem.CreateRosterItemDefinition.CreateItemDefinition, num2);
			}
		}

		// Sắp xếp lại các món hàng trong Shop theo thứ tự danh mục
		TPSingleton<BuildingManager>.Instance.Shop.ShopController.SortItems();
	}

	#endregion
}
