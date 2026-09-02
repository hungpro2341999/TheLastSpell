using System.Xml.Linq;

namespace TheLastStand.Definition.Meta;

public class UnlockBuildingUpgradeMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockBuildingUpgrade";

	public string UpgradeId { get; private set; }

	public UnlockBuildingUpgradeMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		UpgradeId = xElement.Value;
	}

	public override string ToString()
	{
		return "UnlockBuildingUpgrade (" + UpgradeId + ")";
	}
}
