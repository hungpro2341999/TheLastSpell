using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class SwapActionDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "SwapAction";

	public string OldActionId { get; private set; }

	public string NewActionId { get; private set; }

	public SwapActionDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XElement xElement = xContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("OldActionId");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("SwapActionDefinition must have an OldActionId");
			return;
		}
		OldActionId = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("NewActionId");
		if (xAttribute2.IsNullOrEmpty())
		{
			TPDebug.LogError("SwapkActionDefinition must have an NewActionId");
		}
		else
		{
			NewActionId = xAttribute2.Value;
		}
	}
}
