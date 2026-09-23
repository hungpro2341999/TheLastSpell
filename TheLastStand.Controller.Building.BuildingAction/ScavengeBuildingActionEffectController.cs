using TPLib;
using TPLib.Log;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Controller xử lý hiệu ứng "Dọn dẹp / Khai quật" (Scavenge Effect) tàn tích (Ruins) hoặc xác quái (Corpses/Bone Piles).
/// Khi công nhân dọn dẹp một đống đổ nát:
/// - Gây sát thương/trừ máu của tàn tích đó (tiến tới dọn sạch hoàn toàn ô đất).
/// - Thu về tài nguyên: Vàng (Gold), Vật liệu (Materials), Linh hồn nguyền rủa (Damned Souls) và Trang bị/Vật phẩm rơi ra.
/// - Cập nhật thống kê tiến độ thành tựu (Achievement), báo cáo sản xuất (Production Report) và Analytics.
/// </summary>
public class ScavengeBuildingActionEffectController : BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model chuyên biệt lưu trữ dữ liệu dọn dẹp/khai quật tàn tích.
	/// </summary>
	public ScavengeBuildingActionEffect ScavengeBuildingActionEffect => base.BuildingActionEffect as ScavengeBuildingActionEffect;

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo controller cho hiệu ứng dọn dẹp công trình/tàn tích.
	/// </summary>
	/// <param name="definition">Định nghĩa dữ liệu khai quật (sát thương gây ra cho tàn tích, lượng vàng/vật liệu/soul/item nhận được).</param>
	/// <param name="productionBuilding">Module sản xuất của tàn tích/công trình đang khai quật.</param>
	public ScavengeBuildingActionEffectController(ScavengeBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new ScavengeBuildingActionEffect(definition, this, productionBuilding);
	}

	#endregion

	#region Tile Targeting Validation

	/// <summary>
	/// Kiểm tra tính hợp lệ trên ô Tile chỉ định. Luôn trả về true.
	/// </summary>
	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	#endregion

	#region Action Execution Logic

	/// <summary>
	/// Thực thi logic khai quật/dọn dẹp tàn tích:
	/// 1. Kiểm tra trạng thái tồn tại của tàn tích (không phá hủy nếu đã chết).
	/// 2. Hủy kích hoạt kỹ năng tử trận (Death Rattle) nếu có.
	/// 3. Gây sát thương lên tàn tích (giảm máu dần qua từng lần dọn dẹp).
	/// 4. Thu hoạch Vàng, Vật liệu, Damned Souls.
	/// 5. Tạo các vật phẩm/trang bị rơi ra (nếu có cấu hình CreateItemDefinitions) và ghi vào ProductionReport.
	/// 6. Kích hoạt toàn bộ hiệu ứng thị giác (GainGoldDisplay, GainMaterialDisplay, GainDamnedSoulsDisplay, AttackFeedback).
	/// </summary>
	public override void ExecuteActionEffect()
	{
		TheLastStand.Model.Building.Building buildingParent = base.BuildingActionEffect.ProductionBuilding.BuildingParent;
		// Nếu công trình thuộc loại bất tử hoặc đã chết hoàn toàn thì không thể dọn dẹp tiếp
		if (buildingParent.BlueprintModule.IsIndestructible || buildingParent.DamageableModule.IsDead)
		{
			return;
		}
		
		// Ngăn chặn kích hoạt hiệu ứng khi chết (Death Rattle) nếu công trình bị phá hủy bởi hành động dọn dẹp
		if (buildingParent.BattleModule != null)
		{
			buildingParent.BattleModule.ShouldTriggerDeathRattle = false;
		}
		
		// Tăng tiến độ thành tựu dọn dẹp xác quái và tàn tích
		TPSingleton<AchievementManager>.Instance.IncreaseAchievementProgression(StatContainer.STAT_SCAVENGED_CORPSES_AND_RUINS_AMOUNT, 1);
		
		// Trừ máu tàn tích tương ứng với công sức dọn dẹp (Damage)
		buildingParent.BuildingController.DamageableModuleController.LoseHealth(ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.Damage);
		
		// Cộng các tài nguyên thu hoạch được: Vàng, Vật liệu, Damned Souls
		int gainMaterials = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainMaterials;
		int gainGold = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainGold;
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + gainGold);
		TPSingleton<ResourceManager>.Instance.Materials += gainMaterials;
		ApplicationManager.Application.DamnedSouls += (uint)ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainDamnedSouls;
		TPSingleton<BuildingManager>.Instance.Log($"Scavenge {gainGold} Gold and {gainMaterials} Materials");
		
		// Nếu đây là đống xác quái (Bone Pile), ghi nhận vào GameAnalytics và điều kiện Meta
		if (BonePileDatabase.BonePileGeneratorsDefinition.Buildings.ContainsKey(buildingParent.Id))
		{
			TPSingleton<GameManager>.Instance.GameAnalytics.OnBonePileScavenged(gainGold, gainMaterials);
			TPSingleton<MetaConditionManager>.Instance.IncreaseScavengedBonePile(base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id);
		}
		
		// Khởi tạo dữ liệu Attack feedback để mô phỏng tác động sát thương dọn dẹp
		AttackSkillActionExecutionTileData attackData = new AttackSkillActionExecutionTileData
		{
			Damageable = buildingParent.DamageableModule,
			TotalDamage = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.Damage,
			TargetTile = buildingParent.OriginTile,
			TargetRemainingHealth = buildingParent.DamageableModule.Health,
			TargetHealthTotal = buildingParent.DamageableModule.HealthTotal,
			TargetArmorTotal = buildingParent.DamageableModule.ArmorTotal
		};
		
		// Hiển thị animation cộng Vàng (+Gold)
		if (gainGold > 0)
		{
			GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(gainGold);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		}
		
		// Hiển thị animation cộng Vật liệu (+Materials)
		if (gainMaterials > 0)
		{
			GainMaterialDisplay pooledComponent2 = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent2.Init(gainMaterials);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent2);
		}
		
		// Hiển thị animation cộng Damned Souls (+Damned Souls)
		if (ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainDamnedSouls > 0)
		{
			GainDamnedSoulsDisplay pooledComponent3 = ObjectPooler.GetPooledComponent("GainDamnedSoulsDisplay", ResourcePooler.LoadOnce<GainDamnedSoulsDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainDamnedSoulsDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent3.Init(ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainDamnedSouls);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent3);
		}
		
		// Xử lý tạo và trao vật phẩm thưởng nếu tàn tích này rơi ra trang bị
		if (ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.CreateItemDefinitions.Count > 0)
		{
			for (int i = 0; i < ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.CreateItemDefinitions.Count; i++)
			{
				CreateItemDefinition createItemDefinition = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.CreateItemDefinitions[i];
				ProductionItems productionItem = new ProductionItemController(base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition, base.BuildingActionEffect.ProductionBuilding.BuildingParent.ProductionModule.Level, ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.BuildingActionDefinitionContainer.Id, i).ProductionItem;
				productionItem.IsNightProduction = false;
				LevelProbabilitiesTreeController levelProbabilitiesTreeController = new LevelProbabilitiesTreeController(base.BuildingActionEffect.ProductionBuilding, ItemDatabase.ItemGenerationModifierListDefinitions[createItemDefinition.LevelModifierListId]);
				int prodRewardsCount = TPSingleton<ItemManager>.Instance.ProdRewardsCount;
				
				// Sinh các lựa chọn vật phẩm theo bậc phẩm chất / cấp độ tính toán từ xác suất
				for (int j = 0; j < prodRewardsCount; j++)
				{
					TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.None, createItemDefinition, levelProbabilitiesTreeController.GenerateLevel());
					productionItem.Items.Add(item);
					TPSingleton<ItemManager>.Instance.Log("(" + base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id + ") Item created: " + item.ItemDefinition.Id + ".", CLogLevel.MAJOR);
				}
				
				// Hiển thị thông báo nhận vật phẩm trên đầu công trình
				CreateItemDisplay pooledComponent4 = ObjectPooler.GetPooledComponent("CreateItemDisplay", ResourcePooler.LoadOnce<CreateItemDisplay>("Prefab/Displayable Effect/UI Effect Displays/CreateItemDisplay"), EffectManager.EffectDisplaysParent);
				pooledComponent4.Init(productionItem);
				base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent4);
				
				// Đưa các vật phẩm này vào báo cáo sản xuất (Production Report) để người chơi lựa chọn nhận
				TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
			}
		}
		
		// Gắn hiệu ứng rung/chớp chém sát thương khi đào bới lên tàn tích
		AttackFeedback attackFeedback = buildingParent.DamageableModule.DamageableView.AttackFeedback;
		attackFeedback.AddDamageInstance(attackData);
		buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(attackFeedback);
	}

	#endregion
}

