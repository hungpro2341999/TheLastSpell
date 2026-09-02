using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Controller.CastFx;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.Meta;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingAction;

public class BuildingActionController
{
	public TheLastStand.Model.Building.BuildingAction.BuildingAction BuildingAction { get; }

	public BuildingActionController(XContainer container, ProductionModule productionBuilding)
	{
		XElement xElement = container as XElement;
		BuildingActionDefinition buildingActionDefinition = BuildingDatabase.BuildingActionDefinitions[xElement.Attribute("Id").Value];
		BuildingAction = new TheLastStand.Model.Building.BuildingAction.BuildingAction(buildingActionDefinition, this, productionBuilding);
		GenerateActionEffects(buildingActionDefinition, productionBuilding);
		if (BuildingAction.BuildingActionDefinition.CastFxDefinition != null)
		{
			BuildingAction.CastFx = new CastFxController(BuildingAction.BuildingActionDefinition.CastFxDefinition).CastFx;
			BuildingAction.CastFx.SourceTile = BuildingAction.ProductionBuilding.BuildingParent.OriginTile;
			BuildingAction.CastFx.CastFXInterpreterContext = new CastFXInterpreterContext(BuildingAction.CastFx);
		}
	}

	public BuildingActionController(BuildingActionDefinition actionDefinition, ProductionModule productionBuilding)
	{
		BuildingAction = new TheLastStand.Model.Building.BuildingAction.BuildingAction(actionDefinition, this, productionBuilding);
		GenerateActionEffects(actionDefinition, productionBuilding);
		if (BuildingAction.BuildingActionDefinition.CastFxDefinition != null)
		{
			BuildingAction.CastFx = new CastFxController(BuildingAction.BuildingActionDefinition.CastFxDefinition).CastFx;
			BuildingAction.CastFx.SourceTile = BuildingAction.ProductionBuilding.BuildingParent.OriginTile;
			BuildingAction.CastFx.CastFXInterpreterContext = new CastFXInterpreterContext(BuildingAction.CastFx);
		}
	}

	public bool CanExecuteAction()
	{
		if (BuildingAction.BuildingActionDefinition.ContainsRepelFogEffect && !TPSingleton<FogManager>.Instance.Fog.CanDecreaseFogDensity)
		{
			return false;
		}
		if (TPSingleton<ResourceManager>.Instance.Workers >= ResourceManager.GetModifiedWorkersCost(BuildingAction.BuildingActionDefinition) && (BuildingAction.BuildingActionDefinition.UsesPerTurnCount == -1 || BuildingAction.UsesPerTurnRemaining > 0))
		{
			if (!BuildingManager.DebugUseForceBuildingActionsAllPhases)
			{
				return GetActionCurrentState() == PhaseStates.E_PhaseState.Available;
			}
			return true;
		}
		return false;
	}

	public bool CanExecuteActionOnAnyTile()
	{
		for (int num = TPSingleton<TileMapManager>.Instance.TileMap.Tiles.Length - 1; num >= 0; num--)
		{
			if (CanExecuteActionOnTile(TPSingleton<TileMapManager>.Instance.TileMap.Tiles[num]))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanExecuteActionOnTile(Tile tile)
	{
		int i = 0;
		for (int count = BuildingAction.BuildingActionEffects.Count; i < count; i++)
		{
			if (!BuildingAction.BuildingActionEffects[i].BuildingActionEffectController.CanExecuteActionEffectOnTile(tile))
			{
				return false;
			}
		}
		return true;
	}

	public void ExecuteActionEffects()
	{
		if (BuildingAction.BuildingActionDefinition.UsesPerTurnCount != -1)
		{
			BuildingAction.UsesPerTurnRemaining--;
		}
		int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(BuildingAction.BuildingActionDefinition, updateGlyphLimits: true);
		TPSingleton<ResourceManager>.Instance.UseWorkers(modifiedWorkersCost);
		if (BuildingAction.BuildingActionEffects.Any((BuildingActionEffect o) => o is ScavengeBuildingActionEffect))
		{
			TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.ScavengeWorkers, modifiedWorkersCost);
			TPSingleton<ResourceManager>.Instance.ScavengeWorkersThisTurn += modifiedWorkersCost;
			int scavengeWorkersThisTurn = TPSingleton<ResourceManager>.Instance.ScavengeWorkersThisTurn;
			if (scavengeWorkersThisTurn > 0)
			{
				TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxScavengeWorkersSingleProd, scavengeWorkersThisTurn);
			}
		}
		int num = 0;
		for (int count = BuildingAction.BuildingActionEffects.Count; num < count; num++)
		{
			BuildingAction.BuildingActionEffects[num].BuildingActionEffectController.ExecuteActionEffect();
		}
		if (BuildingAction.BuildingActionDefinition.CastFxDefinition != null)
		{
			BuildingAction.CastFx.AffectedTiles.Clear();
			BuildingAction.CastFx.TargetTile = BuildingAction.Target;
			BuildingAction.CastFx.AffectedTiles.Add(new List<Tile>(1) { BuildingAction.Target });
			BuildingAction.CastFx.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), BuildingAction.ProductionBuilding.BuildingParent);
		}
	}

	public PhaseStates.E_PhaseState GetActionCurrentState()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			return BuildingAction.BuildingActionDefinition.PhaseStates.NightState;
		}
		if (TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Deployment)
		{
			return BuildingAction.BuildingActionDefinition.PhaseStates.ProductionState;
		}
		return BuildingAction.BuildingActionDefinition.PhaseStates.DeploymentState;
	}

	public void SetTarget(Tile tile)
	{
		BuildingAction.Target = tile;
		for (int num = BuildingAction.BuildingActionEffects.Count - 1; num >= 0; num--)
		{
			BuildingAction.BuildingActionEffects[num].Target = tile;
		}
	}

	private void GenerateActionEffects(BuildingActionDefinition actionDefinition, ProductionModule productionBuilding)
	{
		if (actionDefinition.BuildingActionEffectDefinition != null)
		{
			BuildingAction.BuildingActionEffects = new List<BuildingActionEffect>();
			int i = 0;
			for (int count = actionDefinition.BuildingActionEffectDefinition.Count; i < count; i++)
			{
				BuildingActionEffectDefinition buildingActionEffectDefinition = actionDefinition.BuildingActionEffectDefinition[i];
				BuildingActionEffect buildingActionEffect = ((buildingActionEffectDefinition is FillGaugeBuildingActionEffectDefinition definition) ? new FillGaugeBuildingActionEffectController(definition, productionBuilding).FillGaugeBuildingActionEffect : ((buildingActionEffectDefinition is HealBuildingActionEffectDefinition definition2) ? new HealBuildingActionEffectController(definition2, productionBuilding).HealBuildingActionEffect : ((buildingActionEffectDefinition is HealManaBuildingActionEffectDefinition definition3) ? new HealManaBuildingActionEffectController(definition3, productionBuilding).HealManaBuildingActionEffect : ((buildingActionEffectDefinition is ScavengeBuildingActionEffectDefinition definition4) ? new ScavengeBuildingActionEffectController(definition4, productionBuilding).ScavengeBuildingActionEffect : ((buildingActionEffectDefinition is GainGoldBuildingActionEffectDefinition definition5) ? new GainGoldBuildingActionEffectController(definition5, productionBuilding).GainGoldBuildingActionEffect : ((buildingActionEffectDefinition is GainMaterialsBuildingActionEffectDefinition definition6) ? new GainMaterialsBuildingActionEffectController(definition6, productionBuilding).GainMaterialsBuildingActionEffect : ((buildingActionEffectDefinition is RepelFogBuildingActionEffectDefinition definition7) ? new RepelFogBuildingActionEffectController(definition7, productionBuilding).RepelFogBuildingActionEffect : ((buildingActionEffectDefinition is RevealDangerIndicatorsBuildingActionEffectDefinition definition8) ? new RevealDangerIndicatorsBuildingActionEffectController(definition8, productionBuilding).RevealWaveEnemiesRatioBuildingActionEffect : ((buildingActionEffectDefinition is RerollWaveBuildingActionEffectDefinition definition9) ? ((BuildingActionEffect)new RerollWaveBuildingActionEffectController(definition9, productionBuilding).RerollWaveBuildingActionEffect) : ((BuildingActionEffect)((!(buildingActionEffectDefinition is UpgradeStatBuildingActionEffectDefinition definition10)) ? null : new UpgradeStatBuildingActionEffectController(definition10, productionBuilding).UpgradeStatBuildingActionEffect)))))))))));
				BuildingActionEffect item = buildingActionEffect;
				TheLastStand.Model.Building.BuildingAction.BuildingAction buildingAction = BuildingAction;
				buildingActionEffectDefinition = actionDefinition.BuildingActionEffectDefinition[i];
				bool isExecutionInstant = ((buildingActionEffectDefinition is HealBuildingActionEffectDefinition healBuildingActionEffectDefinition) ? (healBuildingActionEffectDefinition.BuildingActionTargeting == BuildingActionEffectDefinition.E_BuildingActionTargeting.All) : ((buildingActionEffectDefinition is HealManaBuildingActionEffectDefinition healManaBuildingActionEffectDefinition) ? (healManaBuildingActionEffectDefinition.BuildingActionTargeting == BuildingActionEffectDefinition.E_BuildingActionTargeting.All) : (!(buildingActionEffectDefinition is UpgradeStatBuildingActionEffectDefinition upgradeStatBuildingActionEffectDefinition) || upgradeStatBuildingActionEffectDefinition.BuildingActionTargeting == BuildingActionEffectDefinition.E_BuildingActionTargeting.All)));
				buildingAction.IsExecutionInstant = isExecutionInstant;
				BuildingAction.BuildingActionEffects.Add(item);
			}
		}
	}
}
