using System.Collections.Generic;
using TPLib.Log;
using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization.Building;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveEffect;
using TheLastStand.Serialization.Building.BuildingPassive.PassiveTrigger;
using UnityEngine;

namespace TheLastStand.Model.Building.BuildingPassive;

public class BuildingPassive : IDeserializable, ISerializable
{
	public PassivesModule BuildingPassivesModule { get; private set; }

	public BuildingPassiveController BuildingPassiveController { get; private set; }

	public BuildingPassiveDefinition BuildingPassiveDefinition { get; private set; }

	public List<BuildingPassiveEffect> PassiveEffects { get; private set; }

	public List<TheLastStand.Model.Building.BuildingPassive.PassiveTrigger.PassiveTrigger> PassiveTriggers { get; private set; }

	public BuildingPassive(SerializedBuildingPassive serializedPassive, PassivesModule buildingPassivesModule, BuildingPassiveController buildingPassiveController, int saveVersion)
	{
		BuildingPassivesModule = buildingPassivesModule;
		BuildingPassiveController = buildingPassiveController;
		Deserialize(serializedPassive, saveVersion);
		DeserializeEffects(serializedPassive);
		DeserializeTriggers(serializedPassive);
	}

	public BuildingPassive(PassivesModule buildingPassivesModule, BuildingPassiveDefinition buildingPassiveDefinition, BuildingPassiveController buildingPassiveController)
	{
		BuildingPassivesModule = buildingPassivesModule;
		BuildingPassiveDefinition = buildingPassiveDefinition;
		BuildingPassiveController = buildingPassiveController;
		CreateEffects();
		CreateTriggers();
	}

	private void CreateEffects(BuildingPassiveEffectFactory.SerializedBuildingPassiveEffectData serializedData = null)
	{
		PassiveEffects = new List<BuildingPassiveEffect>();
		foreach (BuildingPassiveEffectDefinition passiveEffectDefinition in BuildingPassiveDefinition.PassiveEffectDefinitions)
		{
			BuildingPassiveEffect buildingPassiveEffect = BuildingPassiveEffectFactory.PassiveEffectFromDefinition(passiveEffectDefinition, BuildingPassivesModule, serializedData);
			if (buildingPassiveEffect == null)
			{
				CLoggerManager.Log($"Unknown building passive effect definition : {passiveEffectDefinition.GetType()}.", LogType.Error, CLogLevel.MAJOR);
			}
			else
			{
				PassiveEffects.Add(buildingPassiveEffect);
			}
		}
	}

	private void CreateTriggers(BuildingPassiveTriggerFactory.SerializedBuildingPassiveTriggerData serializedData = null)
	{
		PassiveTriggers = new List<TheLastStand.Model.Building.BuildingPassive.PassiveTrigger.PassiveTrigger>();
		foreach (PassiveTriggerDefinition triggerDefinition in BuildingPassiveDefinition.TriggerDefinitions)
		{
			TheLastStand.Model.Building.BuildingPassive.PassiveTrigger.PassiveTrigger passiveTrigger = BuildingPassiveTriggerFactory.PassiveTriggerFromDefinition(triggerDefinition, serializedData);
			if (passiveTrigger == null)
			{
				CLoggerManager.Log($"Unknown trigger definition : {triggerDefinition.GetType()}.", LogType.Error, CLogLevel.MAJOR);
			}
			else
			{
				PassiveTriggers.Add(passiveTrigger);
			}
		}
	}

	public void Deserialize(ISerializedData serializedData, int saveVersion = -1)
	{
		SerializedBuildingPassive serializedBuildingPassive = serializedData as SerializedBuildingPassive;
		if (!BuildingDatabase.BuildingPassiveDefinitions.TryGetValue(serializedBuildingPassive.Id, out var value))
		{
			CLoggerManager.Log("Could not find the given passive definition in the database : {container.Id}. This should not happen.", LogType.Warning);
		}
		BuildingPassiveDefinition = value;
	}

	private void DeserializeEffects(SerializedBuildingPassive container)
	{
		BuildingPassiveEffectFactory.SerializedBuildingPassiveEffectData serializedData = new BuildingPassiveEffectFactory.SerializedBuildingPassiveEffectData(container);
		CreateEffects(serializedData);
	}

	private void DeserializeTriggers(SerializedBuildingPassive container)
	{
		BuildingPassiveTriggerFactory.SerializedBuildingPassiveTriggerData serializedData = new BuildingPassiveTriggerFactory.SerializedBuildingPassiveTriggerData(container);
		CreateTriggers(serializedData);
	}

	public virtual ISerializedData Serialize()
	{
		List<SerializedAfterXProductionPhasesTrigger> list = new List<SerializedAfterXProductionPhasesTrigger>();
		for (int i = 0; i < PassiveTriggers.Count; i++)
		{
			if (PassiveTriggers[i] is AfterXProductionPhasesTrigger afterXProductionPhasesTrigger)
			{
				list.Add(afterXProductionPhasesTrigger.Serialize());
			}
		}
		List<SerializedAfterXNightEndTrigger> list2 = new List<SerializedAfterXNightEndTrigger>();
		for (int j = 0; j < PassiveTriggers.Count; j++)
		{
			if (PassiveTriggers[j] is AfterXNightEndTrigger afterXNightEndTrigger)
			{
				list2.Add(afterXNightEndTrigger.Serialize());
			}
		}
		List<SerializedAfterXNightTurnsTrigger> list3 = new List<SerializedAfterXNightTurnsTrigger>();
		for (int k = 0; k < PassiveTriggers.Count; k++)
		{
			if (PassiveTriggers[k] is AfterXNightTurnsTrigger afterXNightTurnsTrigger)
			{
				list3.Add(afterXNightTurnsTrigger.Serialize());
			}
		}
		List<SerializedGenerateLightFog> list4 = new List<SerializedGenerateLightFog>();
		for (int l = 0; l < PassiveEffects.Count; l++)
		{
			if (PassiveEffects[l] is GenerateLightFog generateLightFog)
			{
				list4.Add(generateLightFog.Serialize());
			}
		}
		return new SerializedBuildingPassive
		{
			Id = BuildingPassiveDefinition.Id,
			AfterXProductionPhasesTriggers = list,
			AfterXNightEndTriggers = list2,
			AfterXNightTurnsTriggers = list3,
			GenerateLightFogEffects = list4
		};
	}
}
