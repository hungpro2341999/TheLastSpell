using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaUpgradeController
{
	public MetaUpgrade MetaUpgrade { get; private set; }

	public MetaUpgradeController(MetaUpgradeDefinition definition)
	{
		MetaUpgrade = new MetaUpgrade(definition, this);
		GenerateConditionsLists();
	}

	public bool AreActivationConditionsFulfilled()
	{
		if (!AreConditionsFulfilled(MetaCondition.E_MetaConditionCategory.Activation) || MetaUpgrade.InvestedSouls < MetaUpgrade.MetaUpgradeDefinition.Price)
		{
			return false;
		}
		return true;
	}

	public bool AreUnlockConditionsFulfilled()
	{
		return AreConditionsFulfilled(MetaCondition.E_MetaConditionCategory.Unlock);
	}

	private bool AreConditionsFulfilled(MetaCondition.E_MetaConditionCategory category)
	{
		List<List<MetaConditionController>> conditions = MetaUpgrade.GetConditions(category);
		if (conditions.Count == 0)
		{
			return true;
		}
		foreach (List<MetaConditionController> item in conditions)
		{
			bool flag = true;
			int groupIndex = item[0].MetaCondition.MetaConditionDefinition.ConditionsGroupIndex;
			MetaUpgradeDefinition.ConditionsGroup conditionsGroup = ((category == MetaCondition.E_MetaConditionCategory.Activation) ? MetaUpgrade.MetaUpgradeDefinition.ActivationConditionsDefinitions : MetaUpgrade.MetaUpgradeDefinition.UnlockConditionsDefinitions).FirstOrDefault((MetaUpgradeDefinition.ConditionsGroup o) => o.GroupIndex == groupIndex);
			if (conditionsGroup != null && conditionsGroup.CheckOnce && item[0].MetaCondition.HasBeenChecked)
			{
				continue;
			}
			for (int num = item.Count - 1; num >= 0; num--)
			{
				item[num].MetaCondition.HasBeenChecked = true;
			}
			for (int num2 = item.Count - 1; num2 >= 0; num2--)
			{
				if (!item[num2].IsComplete())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private void GenerateConditionsLists()
	{
		GenerateConditionsList(MetaUpgrade.MetaUpgradeDefinition.UnlockConditionsDefinitions, MetaCondition.E_MetaConditionCategory.Unlock);
		GenerateConditionsList(MetaUpgrade.MetaUpgradeDefinition.ActivationConditionsDefinitions, MetaCondition.E_MetaConditionCategory.Activation);
	}

	private void GenerateConditionsList(List<MetaUpgradeDefinition.ConditionsGroup> conditionsGroups, MetaCondition.E_MetaConditionCategory category)
	{
		for (int num = conditionsGroups.Count - 1; num >= 0; num--)
		{
			MetaUpgradeDefinition.ConditionsGroup conditionsGroup = conditionsGroups[num];
			for (int num2 = conditionsGroup.Conditions.Count - 1; num2 >= 0; num2--)
			{
				MetaConditionDefinition metaConditionDefinition = conditionsGroup.Conditions[num2];
				string id = $"{MetaUpgrade.MetaUpgradeDefinition.Id}=>{category}=>{metaConditionDefinition}";
				TPSingleton<MetaConditionManager>.Instance.CreateMetaCondition(id, this, metaConditionDefinition, category);
			}
		}
	}

	public override string ToString()
	{
		return MetaUpgrade.Name + " (Controller)";
	}
}
