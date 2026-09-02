using System.Xml.Linq;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Definition.Building.BuildingGaugeEffect;

public class UpgradeStatGaugeEffectDefinition : BuildingGaugeEffectDefinition
{
	public const string Name = "GlobalUpgradeStat";

	public UpgradeStatDefinition UpgradeStatDefinition { get; set; }

	public UpgradeStatGaugeEffectDefinition(XContainer container)
		: base("GlobalUpgradeStat", container)
	{
	}

	public override BuildingGaugeEffectDefinition Clone()
	{
		UpgradeStatGaugeEffectDefinition obj = base.Clone() as UpgradeStatGaugeEffectDefinition;
		obj.UpgradeStatDefinition = UpgradeStatDefinition;
		return obj;
	}
}
