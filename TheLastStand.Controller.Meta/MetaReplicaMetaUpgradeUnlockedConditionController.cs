using TPLib;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaReplicaMetaUpgradeUnlockedConditionController : MetaReplicaConditionController
{
	public MetaReplicaMetaUpgradeUnlockedConditionDefinition MetaUpgradeUnlockedDefinition => base.ConditionDefinition as MetaReplicaMetaUpgradeUnlockedConditionDefinition;

	public MetaReplicaMetaUpgradeUnlockedConditionController(MetaReplicaMetaUpgradeUnlockedConditionDefinition definition)
		: base(definition)
	{
	}

	public override bool Evaluate(MetaNarration context)
	{
		return TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Find((MetaUpgrade u) => u.MetaUpgradeDefinition.Id == MetaUpgradeUnlockedDefinition.UpgradeId) != null;
	}
}
