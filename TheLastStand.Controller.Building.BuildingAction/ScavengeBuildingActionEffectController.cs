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

public class ScavengeBuildingActionEffectController : BuildingActionEffectController
{
	public ScavengeBuildingActionEffect ScavengeBuildingActionEffect => base.BuildingActionEffect as ScavengeBuildingActionEffect;

	public ScavengeBuildingActionEffectController(ScavengeBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new ScavengeBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		TheLastStand.Model.Building.Building buildingParent = base.BuildingActionEffect.ProductionBuilding.BuildingParent;
		if (buildingParent.BlueprintModule.IsIndestructible || buildingParent.DamageableModule.IsDead)
		{
			return;
		}
		if (buildingParent.BattleModule != null)
		{
			buildingParent.BattleModule.ShouldTriggerDeathRattle = false;
		}
		TPSingleton<AchievementManager>.Instance.IncreaseAchievementProgression(StatContainer.STAT_SCAVENGED_CORPSES_AND_RUINS_AMOUNT, 1);
		buildingParent.BuildingController.DamageableModuleController.LoseHealth(ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.Damage);
		int gainMaterials = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainMaterials;
		int gainGold = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainGold;
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + gainGold);
		TPSingleton<ResourceManager>.Instance.Materials += gainMaterials;
		ApplicationManager.Application.DamnedSouls += (uint)ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainDamnedSouls;
		TPSingleton<BuildingManager>.Instance.Log($"Scavenge {gainGold} Gold and {gainMaterials} Materials");
		if (BonePileDatabase.BonePileGeneratorsDefinition.Buildings.ContainsKey(buildingParent.Id))
		{
			TPSingleton<GameManager>.Instance.GameAnalytics.OnBonePileScavenged(gainGold, gainMaterials);
			TPSingleton<MetaConditionManager>.Instance.IncreaseScavengedBonePile(base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id);
		}
		AttackSkillActionExecutionTileData attackData = new AttackSkillActionExecutionTileData
		{
			Damageable = buildingParent.DamageableModule,
			TotalDamage = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.Damage,
			TargetTile = buildingParent.OriginTile,
			TargetRemainingHealth = buildingParent.DamageableModule.Health,
			TargetHealthTotal = buildingParent.DamageableModule.HealthTotal,
			TargetArmorTotal = buildingParent.DamageableModule.ArmorTotal
		};
		if (gainGold > 0)
		{
			GainGoldDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainGoldDisplay", ResourcePooler.LoadOnce<GainGoldDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainGoldDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(gainGold);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		}
		if (gainMaterials > 0)
		{
			GainMaterialDisplay pooledComponent2 = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent2.Init(gainMaterials);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent2);
		}
		if (ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainDamnedSouls > 0)
		{
			GainDamnedSoulsDisplay pooledComponent3 = ObjectPooler.GetPooledComponent("GainDamnedSoulsDisplay", ResourcePooler.LoadOnce<GainDamnedSoulsDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainDamnedSoulsDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent3.Init(ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.GainDamnedSouls);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent3);
		}
		if (ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.CreateItemDefinitions.Count > 0)
		{
			for (int i = 0; i < ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.CreateItemDefinitions.Count; i++)
			{
				CreateItemDefinition createItemDefinition = ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.CreateItemDefinitions[i];
				ProductionItems productionItem = new ProductionItemController(base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition, base.BuildingActionEffect.ProductionBuilding.BuildingParent.ProductionModule.Level, ScavengeBuildingActionEffect.ScavengeBuildingActionDefinition.BuildingActionDefinitionContainer.Id, i).ProductionItem;
				productionItem.IsNightProduction = false;
				LevelProbabilitiesTreeController levelProbabilitiesTreeController = new LevelProbabilitiesTreeController(base.BuildingActionEffect.ProductionBuilding, ItemDatabase.ItemGenerationModifierListDefinitions[createItemDefinition.LevelModifierListId]);
				int prodRewardsCount = TPSingleton<ItemManager>.Instance.ProdRewardsCount;
				for (int j = 0; j < prodRewardsCount; j++)
				{
					TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.None, createItemDefinition, levelProbabilitiesTreeController.GenerateLevel());
					productionItem.Items.Add(item);
					TPSingleton<ItemManager>.Instance.Log("(" + base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id + ") Item created: " + item.ItemDefinition.Id + ".", CLogLevel.MAJOR);
				}
				CreateItemDisplay pooledComponent4 = ObjectPooler.GetPooledComponent("CreateItemDisplay", ResourcePooler.LoadOnce<CreateItemDisplay>("Prefab/Displayable Effect/UI Effect Displays/CreateItemDisplay"), EffectManager.EffectDisplaysParent);
				pooledComponent4.Init(productionItem);
				base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent4);
				TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
			}
		}
		AttackFeedback attackFeedback = buildingParent.DamageableModule.DamageableView.AttackFeedback;
		attackFeedback.AddDamageInstance(attackData);
		buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(attackFeedback);
	}
}
