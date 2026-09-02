using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingAction;

public class GainMaterialsBuildingActionEffectController : BuildingActionEffectController
{
	public GainMaterialsBuildingActionEffect GainMaterialsBuildingActionEffect => base.BuildingActionEffect as GainMaterialsBuildingActionEffect;

	public GainMaterialsBuildingActionEffectController(GainMaterialsBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new GainMaterialsBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return true;
	}

	public override void ExecuteActionEffect()
	{
		if (base.BuildingActionEffect.ProductionBuilding.BuildingParent.BlueprintModule.IsIndestructible || !base.BuildingActionEffect.ProductionBuilding.BuildingParent.DamageableModule.IsDead)
		{
			TPSingleton<ResourceManager>.Instance.Materials += GainMaterialsBuildingActionEffect.GainMaterialsBuildingActionDefinition.GainMaterials;
			TPSingleton<BuildingManager>.Instance.Log($"Gaining {GainMaterialsBuildingActionEffect.GainMaterialsBuildingActionDefinition.GainMaterials} Materials from GainMaterials building action effect.");
			GainMaterialDisplay pooledComponent = ObjectPooler.GetPooledComponent("GainMaterialDisplay", ResourcePooler.LoadOnce<GainMaterialDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainMaterialDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent.Init(GainMaterialsBuildingActionEffect.GainMaterialsBuildingActionDefinition.GainMaterials);
			base.BuildingActionEffect.ProductionBuilding.BuildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent);
		}
	}
}
