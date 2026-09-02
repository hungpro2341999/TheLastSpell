using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImproveLevelDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImproveLevel";

	public int LevelsCount { get; private set; } = 1;

	public ImproveLevelDefinition(XContainer xContainer)
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
				LevelsCount = result;
			}
		}
	}
}
