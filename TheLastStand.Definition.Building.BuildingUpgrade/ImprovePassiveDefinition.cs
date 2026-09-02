using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ImprovePassiveDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ImprovePassive";

	public string PassiveId { get; private set; }

	public Node Value { get; private set; }

	public ImprovePassiveDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XElement xElement = xContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("PassiveId");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("ImprovePassiveDefinition must have a PassiveId");
			return;
		}
		PassiveId = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("UpgradedBonusValue");
		if (xAttribute2.IsNullOrEmpty())
		{
			TPDebug.LogError(base.Id + " UpgradeEffect must have an Attribute UpgradedBonusValue");
		}
		else
		{
			Value = Parser.Parse(xAttribute2.Value);
		}
	}
}
