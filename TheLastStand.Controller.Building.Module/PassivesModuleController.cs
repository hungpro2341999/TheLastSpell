using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Controller.Building.Module;

public class PassivesModuleController : BuildingModuleController
{
	public PassivesModule PassivesModule { get; }

	public PassivesModuleController(BuildingController buildingControllerParent, PassivesModuleDefinition passivesModuleDefinition)
		: base(buildingControllerParent, passivesModuleDefinition)
	{
		PassivesModule = base.BuildingModule as PassivesModule;
	}

	public void ApplyPassiveEffect(E_EffectTime effectTime, bool force = false, bool onLoad = false)
	{
		if (PassivesModule.BuildingPassives != null && !(ApplicationManager.Application.State.GetName() == "LevelEditor"))
		{
			for (int i = 0; i < PassivesModule.BuildingPassives.Count; i++)
			{
				PassivesModule.BuildingPassives[i].BuildingPassiveController.Trigger(effectTime, force, onLoad);
			}
		}
	}

	public void CreatePassives()
	{
		PassivesModule.BuildingPassives = new List<TheLastStand.Model.Building.BuildingPassive.BuildingPassive>();
		for (int i = 0; i < PassivesModule.PassivesModuleDefinition.BuildingPassiveDefinitions.Count; i++)
		{
			PassivesModule.BuildingPassives.Add(new BuildingPassiveController(PassivesModule, PassivesModule.PassivesModuleDefinition.BuildingPassiveDefinitions[i]).BuildingPassive);
		}
	}

	public void EndTurn()
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
			{
				ApplyPassiveEffect(E_EffectTime.OnEndProductionTurn);
			}
			break;
		case Game.E_Cycle.Night:
			if (SpawnWaveManager.CurrentSpawnWave == null)
			{
				ApplyPassiveEffect(E_EffectTime.OnNightEnd);
			}
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
				ApplyPassiveEffect(E_EffectTime.OnEndNightTurnEnemy);
				break;
			case Game.E_NightTurn.PlayableUnits:
				ApplyPassiveEffect(E_EffectTime.OnEndNightTurnPlayable);
				break;
			}
			break;
		}
	}

	public void OnDeath(bool triggerOnDeathEvent = true)
	{
		if (!(ApplicationManager.Application.State.GetName() == "LevelEditor"))
		{
			if (triggerOnDeathEvent)
			{
				ApplyPassiveEffect(E_EffectTime.OnDeath);
			}
			UndoPermanentPassiveEffects();
		}
	}

	public void StartTurn()
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
			{
				ApplyPassiveEffect(E_EffectTime.OnStartProductionTurn);
			}
			break;
		case Game.E_Cycle.Night:
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.EnemyUnits:
				ApplyPassiveEffect(E_EffectTime.OnStartNightTurnEnemy);
				break;
			case Game.E_NightTurn.PlayableUnits:
				ApplyPassiveEffect(E_EffectTime.OnStartNightTurnPlayable);
				break;
			}
			break;
		}
	}

	public void UndoPermanentPassiveEffects()
	{
		for (int i = 0; i < PassivesModule.BuildingPassives.Count; i++)
		{
			PassivesModule.BuildingPassives[i].BuildingPassiveController.UndoPermanentPassiveEffects();
		}
	}

	public void DeserializePassive(List<SerializedBuildingPassive> passiveElements, int saveVersion)
	{
		PassivesModule.BuildingPassives = new List<TheLastStand.Model.Building.BuildingPassive.BuildingPassive>();
		foreach (SerializedBuildingPassive passiveElement in passiveElements)
		{
			PassivesModule.BuildingPassives.Add(new BuildingPassiveController(passiveElement, PassivesModule, saveVersion).BuildingPassive);
		}
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new PassivesModule(building, buildingModuleDefinition as PassivesModuleDefinition, this);
	}
}
