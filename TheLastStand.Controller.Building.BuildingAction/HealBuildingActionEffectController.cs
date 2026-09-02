using TPLib;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction;

namespace TheLastStand.Controller.Building.BuildingAction;

public class HealBuildingActionEffectController : BuildingActionEffectController
{
	public HealBuildingActionEffect HealBuildingActionEffect => base.BuildingActionEffect as HealBuildingActionEffect;

	public HealBuildingActionEffectController(HealBuildingActionEffectDefinition definition, ProductionModule productionBuilding)
		: base(definition, productionBuilding)
	{
		base.BuildingActionEffect = new HealBuildingActionEffect(definition, this, productionBuilding);
	}

	public override bool CanExecuteActionEffectOnTile(Tile tile)
	{
		return HealBuildingActionEffect.HealBuildingActionDefinition.BuildingActionTargeting switch
		{
			BuildingActionEffectDefinition.E_BuildingActionTargeting.All => true, 
			BuildingActionEffectDefinition.E_BuildingActionTargeting.Single => tile.Unit is PlayableUnit playableUnit && playableUnit.Health < playableUnit.HealthTotal, 
			_ => false, 
		};
	}

	public override void ExecuteActionEffect()
	{
		switch (HealBuildingActionEffect.HealBuildingActionDefinition.BuildingActionTargeting)
		{
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.All:
		{
			foreach (PlayableUnit playableUnit2 in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				Heal(playableUnit2, HealBuildingActionEffect.HealBuildingActionDefinition.Amount);
			}
			break;
		}
		case BuildingActionEffectDefinition.E_BuildingActionTargeting.Single:
			if (base.BuildingActionEffect.Target.Unit is PlayableUnit playableUnit)
			{
				Heal(playableUnit, HealBuildingActionEffect.HealBuildingActionDefinition.Amount);
			}
			else
			{
				TPSingleton<BuildingManager>.Instance.LogError("Selected Unit is not a playable.");
			}
			break;
		}
	}

	private void Heal(PlayableUnit playableUnit, int amount)
	{
		TPSingleton<BuildingManager>.Instance.Log($"Healing {playableUnit.Id} by {amount} points.");
		float num = playableUnit.UnitController.GainHealth(amount, refreshHud: false);
		if (!(num <= 0f))
		{
			HealFeedback healFeedback = playableUnit.DamageableView.HealFeedback;
			healFeedback.AddHealInstance(num, playableUnit.Health);
			playableUnit.UnitController.AddEffectDisplay(healFeedback);
			playableUnit.UnitView.UnitHUD.RefreshInjuryStage();
		}
	}
}
