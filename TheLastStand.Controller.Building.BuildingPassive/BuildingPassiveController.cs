using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class BuildingPassiveController
{
	public TheLastStand.Model.Building.BuildingPassive.BuildingPassive BuildingPassive { get; protected set; }

	public BuildingPassiveController(SerializedBuildingPassive serializedPassive, PassivesModule buildingPassivesModule, int saveVersion)
	{
		BuildingPassive = new TheLastStand.Model.Building.BuildingPassive.BuildingPassive(serializedPassive, buildingPassivesModule, this, saveVersion);
	}

	public BuildingPassiveController(PassivesModule buildingPassivesModule, BuildingPassiveDefinition buildingPassiveDefinition)
	{
		BuildingPassive = new TheLastStand.Model.Building.BuildingPassive.BuildingPassive(buildingPassivesModule, buildingPassiveDefinition, this);
		Trigger(E_EffectTime.OnCreation);
	}

	public void ImproveEffects(int bonus)
	{
		for (int i = 0; i < BuildingPassive.PassiveEffects.Count; i++)
		{
			BuildingPassive.PassiveEffects[i].BuildingPassiveEffectController.ImproveEffect(bonus);
		}
	}

	public void Trigger(E_EffectTime effectTime, bool force = false, bool OnLoad = false)
	{
		if (ApplicationManager.Application.State.GetName() == "LevelEditor")
		{
			return;
		}
		for (int i = 0; i < BuildingPassive.PassiveTriggers.Count; i++)
		{
			if (force || (effectTime == BuildingPassive.PassiveTriggers[i].PassiveTriggerDefinition.EffectTime && BuildingPassive.PassiveTriggers[i].PassiveTriggerController.UpdateAndCheckTrigger(OnLoad)))
			{
				for (int j = 0; j < BuildingPassive.PassiveEffects.Count; j++)
				{
					BuildingPassive.PassiveEffects[j].BuildingPassiveEffectController.Apply();
				}
			}
		}
		if (effectTime != E_EffectTime.OnDeath)
		{
			return;
		}
		for (int k = 0; k < BuildingPassive.PassiveTriggers.Count; k++)
		{
			for (int l = 0; l < BuildingPassive.PassiveEffects.Count; l++)
			{
				BuildingPassive.PassiveEffects[l].BuildingPassiveEffectController.OnDeath();
			}
		}
	}

	public void UndoPermanentPassiveEffects()
	{
		for (int i = 0; i < BuildingPassive.PassiveTriggers.Count; i++)
		{
			if (BuildingPassive.PassiveTriggers[i].PassiveTriggerDefinition.EffectTime == E_EffectTime.Permanent)
			{
				for (int j = 0; j < BuildingPassive.PassiveEffects.Count; j++)
				{
					BuildingPassive.PassiveEffects[j].BuildingPassiveEffectController.Unapply();
				}
			}
		}
	}
}
