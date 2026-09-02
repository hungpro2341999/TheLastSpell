using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class ReplaceBuildingDefinition : BuildingUpgradeEffectDefinition
{
	public const string Name = "ReplaceBuilding";

	public string NewBuildingId { get; private set; }

	public ReplaceBuildingDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		base.Deserialize(xContainer);
		XAttribute xAttribute = (xContainer as XElement).Attribute("NewBuildingId");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("ReplaceBuildingDefinition must have a NewBuildingId");
		}
		else
		{
			NewBuildingId = xAttribute.Value;
		}
	}
}
