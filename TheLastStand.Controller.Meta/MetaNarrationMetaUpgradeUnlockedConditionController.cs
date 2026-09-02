using TPLib;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaNarrationMetaUpgradeUnlockedConditionController : MetaNarrationConditionController
{
	public MetaReplicaMetaUpgradeUnlockedConditionDefinition MetaUpgradeUnlockedDefinition => base.ConditionDefinition as MetaReplicaMetaUpgradeUnlockedConditionDefinition;

	public MetaNarrationMetaUpgradeUnlockedConditionController(MetaReplicaMetaUpgradeUnlockedConditionDefinition definition)
		: base(definition)
	{
	}

	public override bool Evaluate()
	{
		if (TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == MetaUpgradeUnlockedDefinition.UpgradeId) == null && TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == MetaUpgradeUnlockedDefinition.UpgradeId) == null)
		{
			return TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == MetaUpgradeUnlockedDefinition.UpgradeId) != null;
		}
		return true;
	}
}
