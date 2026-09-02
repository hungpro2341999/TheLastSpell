using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveSellRatioDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveSellRatio";

	public int Value { get; private set; } = 1;

	public ImproveSellRatioDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute xAttribute = (xContainer as XElement).Attribute("UpgradedBonusValue");
		if (!xAttribute.IsNullOrEmpty())
		{
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				TPDebug.LogError(base.Id + " UpgradeEffect must have a valid Attribute UpgradedBonusValue (int)");
			}
			else
			{
				Value = result;
			}
		}
	}
}
