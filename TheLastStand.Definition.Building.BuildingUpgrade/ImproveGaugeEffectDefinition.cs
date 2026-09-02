using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveGaugeEffectDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveGaugeEffect";

	public int Value { get; private set; }

	public ImproveGaugeEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute xAttribute = (xContainer as XElement).Attribute("UpgradedBonusValue");
		int result;
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError(base.Id + " UpgradeEffect must have an Attribute UpgradedBonusValue");
		}
		else if (!int.TryParse(xAttribute.Value, out result))
		{
			TPDebug.LogError(base.Id + " UpgradeEffect must have a valid Attribute UpgradedBonusValue (int)");
		}
		else
		{
			Value = result;
		}
	}
}
