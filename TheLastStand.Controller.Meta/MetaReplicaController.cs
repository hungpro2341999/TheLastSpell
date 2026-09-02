using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaReplicaController
{
	public MetaReplica MetaReplica { get; private set; }

	public MetaReplicaController(MetaReplicaDefinition definition, MetaNarration context)
	{
		MetaReplica = new MetaReplica(definition, this, context)
		{
			ConditionControllers = new List<MetaNarrationConditionController>()
		};
		if (definition.ConditionsDefinition == null)
		{
			return;
		}
		for (int num = definition.ConditionsDefinition.Conditions.Count - 1; num >= 0; num--)
		{
			if (definition.ConditionsDefinition.Conditions[num] is MetaReplicaDaysPlayedConditionDefinition definition2)
			{
				MetaReplica.ConditionControllers.Add(new MetaNarrationDaysPlayedConditionController(definition2));
			}
			else if (definition.ConditionsDefinition.Conditions[num] is MetaReplicaMetaUpgradeUnlockedConditionDefinition definition3)
			{
				MetaReplica.ConditionControllers.Add(new MetaNarrationMetaUpgradeUnlockedConditionController(definition3));
			}
			else
			{
				if (!(definition.ConditionsDefinition.Conditions[num] is MetaReplicaUsedReplicaConditionDefinition definition4))
				{
					TPSingleton<MetaNarrationsManager>.Instance.LogError("Could not create a narration condition for definition type " + definition.ConditionsDefinition.Conditions[num].GetType().Name + ".");
					break;
				}
				MetaReplica.ConditionControllers.Add(new MetaReplicaUsedReplicaConditionController(definition4));
			}
		}
	}

	public bool CheckConditions()
	{
		for (int num = MetaReplica.ConditionControllers.Count - 1; num >= 0; num--)
		{
			if (!MetaReplica.ConditionControllers[num].Evaluate())
			{
				return false;
			}
		}
		return true;
	}
}
