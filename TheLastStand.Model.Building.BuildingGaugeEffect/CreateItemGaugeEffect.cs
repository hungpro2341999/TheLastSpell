using TPLib;
using TheLastStand.Controller;
using TheLastStand.Controller.Building.BuildingGaugeEffect;
using TheLastStand.Database;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.View.Building.BuildingGaugeEffect;

namespace TheLastStand.Model.Building.BuildingGaugeEffect;

public class CreateItemGaugeEffect : BuildingGaugeEffect
{
	public CreateItemGaugeEffectDefinition CreateItemGaugeEffectDefinition => base.BuildingGaugeEffectDefinition as CreateItemGaugeEffectDefinition;

	public LevelProbabilitiesTreeController GenerationProbabilitiesTree { get; private set; }

	public CreateItemGaugeEffect(ProductionModule productionBuilding, BuildingGaugeEffectDefinition buildingGaugeEffectDefinition, BuildingGaugeEffectController buildingGaugeEffectController, BuildingGaugeEffectView buildingGaugeEffectView)
		: base(productionBuilding, buildingGaugeEffectDefinition, buildingGaugeEffectController, buildingGaugeEffectView)
	{
		GenerationProbabilitiesTree = new LevelProbabilitiesTreeController(TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel, ItemDatabase.ItemGenerationModifierListDefinitions[CreateItemGaugeEffectDefinition.CreateItemDefinition.LevelModifierListId]);
	}
}
