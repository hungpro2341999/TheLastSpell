using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class UnlockActionDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "UnlockAction";

	public string NewActionId { get; private set; }

	public UnlockActionDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute xAttribute = (xContainer as XElement).Attribute("NewActionId");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("UnlockActionDefinition must have an NewActionId");
		}
		else
		{
			NewActionId = xAttribute.Value;
		}
	}
}
