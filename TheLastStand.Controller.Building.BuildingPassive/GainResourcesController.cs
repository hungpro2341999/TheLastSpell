using TPLib;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class GainResourcesController : BuildingPassiveEffectController
{
	public GainResources GainResources => base.BuildingPassiveEffect as GainResources;

	public GainResourcesController(PassivesModule buildingPassivesModule, GainResourcesDefinition gainResourcesDefinition)
	{
		base.BuildingPassiveEffect = new GainResources(buildingPassivesModule, gainResourcesDefinition, this);
	}

	public override void Apply()
	{
		TheLastStand.Model.Building.Building buildingParent = base.BuildingPassiveEffect.BuildingPassivesModule.BuildingParent;
		int gainMaterials = GainResources.GainResourcesDefinition.GainMaterials;
		int gainGold = GainResources.GainResourcesDefinition.GainGold;
		int gainDamnedSouls = GainResources.GainResourcesDefinition.GainDamnedSouls;
		TPSingleton<ResourceManager>.Instance.SetGold(TPSingleton<ResourceManager>.Instance.Gold + gainGold);
		TPSingleton<ResourceManager>.Instance.Materials += gainMaterials;
		ApplicationManager.Application.DamnedSouls += (uint)gainDamnedSouls;
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
		if (gainDamnedSouls > 0)
		{
			GainDamnedSoulsDisplay pooledComponent3 = ObjectPooler.GetPooledComponent("GainDamnedSoulsDisplay", ResourcePooler.LoadOnce<GainDamnedSoulsDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainDamnedSoulsDisplay"), EffectManager.EffectDisplaysParent);
			pooledComponent3.Init(gainDamnedSouls);
			buildingParent.BuildingController.BlueprintModuleController.AddEffectDisplay(pooledComponent3);
		}
	}
}
