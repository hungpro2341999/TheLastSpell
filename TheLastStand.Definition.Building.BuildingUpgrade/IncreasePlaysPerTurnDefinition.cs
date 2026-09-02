using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class IncreasePlaysPerTurnDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "IncreasePlaysPerTurn";

	public int Value { get; set; }

	public IncreasePlaysPerTurnDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute xAttribute = (xContainer as XElement).Attribute("Value");
		if (!xAttribute.IsNullOrEmpty())
		{
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				TPDebug.LogError(base.Id + " UpgradeEffect must have a valid Attribute Value (int)");
			}
			else
			{
				Value = result;
			}
		}
	}
}
