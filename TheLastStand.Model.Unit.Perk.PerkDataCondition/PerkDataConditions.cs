using System.Collections.Generic;
using System.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using UnityEngine;

namespace TheLastStand.Model.Unit.Perk.PerkDataCondition;

public class PerkDataConditions
{
	public Perk Perk { get; private set; }

	public List<APerkDataCondition> Conditions { get; private set; }

	public PerkDataConditions(PerkDataConditionsDefinition perkDataConditionsDefinition, Perk perk = null)
	{
		Perk = perk;
		GenerateConditions(perkDataConditionsDefinition, perk);
	}

	private void GenerateConditions(PerkDataConditionsDefinition perkDataConditionsDefinition, Perk perk = null)
	{
		Conditions = new List<APerkDataCondition>();
		if (perkDataConditionsDefinition == null)
		{
			return;
		}
		foreach (APerkDataConditionDefinition conditionDefinition in perkDataConditionsDefinition.ConditionDefinitions)
		{
			if (!(conditionDefinition is IsTrueDataConditionDefinition aPerkDataConditionDefinition))
			{
				if (conditionDefinition is IsFalseDataConditionDefinition aPerkDataConditionDefinition2)
				{
					Conditions.Add(new IsFalseDataCondition(aPerkDataConditionDefinition2, perk));
				}
				else
				{
					CLoggerManager.Log($"Tried to Generate a PerkDataCondition that isn't implemented : {conditionDefinition}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkDataConditions");
				}
			}
			else
			{
				Conditions.Add(new IsTrueDataCondition(aPerkDataConditionDefinition, perk));
			}
		}
	}

	public bool IsValid(PerkDataContainer perkDataContainer, bool updatePerkTargetObject = true)
	{
		if (updatePerkTargetObject)
		{
			Perk.TargetObject = perkDataContainer;
		}
		return Conditions.All((APerkDataCondition condition) => condition.IsValid());
	}
}
