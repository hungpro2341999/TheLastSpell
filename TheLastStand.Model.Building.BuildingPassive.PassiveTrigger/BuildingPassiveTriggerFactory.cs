using TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

public static class BuildingPassiveTriggerFactory
{
	public class SerializedBuildingPassiveTriggerData
	{
		public SerializedBuildingPassive Container;

		public int AfterXProductionPhasesTriggerIndex;

		public int AfterXNightEndTriggerIndex;

		public int AfterXNightTurnsTriggerIndex;

		public SerializedBuildingPassiveTriggerData(SerializedBuildingPassive container)
		{
			Container = container;
			AfterXProductionPhasesTriggerIndex = 0;
			AfterXNightEndTriggerIndex = 0;
		}
	}

	public static PassiveTrigger PassiveTriggerFromDefinition(PassiveTriggerDefinition triggerDefinition, SerializedBuildingPassiveTriggerData serializedData = null)
	{
		if (!(triggerDefinition is AfterXProductionPhasesTriggerDefinition definition))
		{
			if (!(triggerDefinition is AfterXNightEndTriggerDefinition definition2))
			{
				if (!(triggerDefinition is AfterXNightTurnsTriggerDefinition definition3))
				{
					if (!(triggerDefinition is StartOfNightEnemyTurnTriggerDefinition definition4))
					{
						if (!(triggerDefinition is StartOfNightPlayableTurnTriggerDefinition definition5))
						{
							if (!(triggerDefinition is StartOfProductionTriggerDefinition definition6))
							{
								if (!(triggerDefinition is EndOfProductionTriggerDefinition definition7))
								{
									if (!(triggerDefinition is OnDeathTriggerDefinition definition8))
									{
										if (!(triggerDefinition is PermanentTriggerDefinition definition9))
										{
											if (!(triggerDefinition is OnConstructionTriggerDefinition definition10))
											{
												if (triggerDefinition is OnExtinguishTriggerDefinition definition11)
												{
													return new OnExtinguishTriggerController(definition11).PassiveTrigger;
												}
												return null;
											}
											return new OnConstructionTriggerController(definition10).PassiveTrigger;
										}
										return new PermanentTriggerController(definition9).PassiveTrigger;
									}
									return new OnDeathTriggerController(definition8).PassiveTrigger;
								}
								return new EndOfProductionTriggerController(definition7).PassiveTrigger;
							}
							return new StartOfProductionTriggerController(definition6).PassiveTrigger;
						}
						return new StartOfNightPlayableTurnTriggerController(definition5).PassiveTrigger;
					}
					return new StartOfNightEnemyTurnTriggerController(definition4).PassiveTrigger;
				}
				return (serializedData?.Container?.AfterXNightTurnsTriggers != null && serializedData.AfterXNightTurnsTriggerIndex < serializedData.Container.AfterXNightTurnsTriggers.Count) ? new AfterXNightTurnsTriggerController(serializedData.Container.AfterXNightTurnsTriggers[serializedData.AfterXNightTurnsTriggerIndex++], definition3).PassiveTrigger : new AfterXNightTurnsTriggerController(definition3).PassiveTrigger;
			}
			return (serializedData?.Container?.AfterXNightEndTriggers != null && serializedData.AfterXNightEndTriggerIndex < serializedData.Container.AfterXNightEndTriggers.Count) ? new AfterXNightEndTriggerController(serializedData.Container.AfterXNightEndTriggers[serializedData.AfterXNightEndTriggerIndex++], definition2).PassiveTrigger : new AfterXNightEndTriggerController(definition2).PassiveTrigger;
		}
		return (serializedData?.Container?.AfterXProductionPhasesTriggers != null && serializedData.AfterXProductionPhasesTriggerIndex < serializedData.Container.AfterXProductionPhasesTriggers.Count) ? new AfterXProductionPhasesTriggerController(serializedData.Container.AfterXProductionPhasesTriggers[serializedData.AfterXProductionPhasesTriggerIndex++], definition).PassiveTrigger : new AfterXProductionPhasesTriggerController(definition).PassiveTrigger;
	}
}
