using System.Xml.Linq;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class CreateRosterItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string BuildingLevelModifiersListId { get; set; }

	public CreateItemDefinition CreateItemDefinition { get; private set; }

	public CreateRosterItemDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = (container as XElement).Element("BuildingLevelModifiersList");
		BuildingLevelModifiersListId = xElement.Attribute("Id")?.Value;
		CreateItemDefinition = new CreateItemDefinition(container);
	}
}
