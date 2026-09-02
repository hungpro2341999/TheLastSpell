using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class GenerateNewItemsRosterDefinition : BuildingPassiveEffectDefinition
{
	public List<CreateRosterItemDefinition> CreateItemRosterDefinitions { get; } = new List<CreateRosterItemDefinition>();

	public GenerateNewItemsRosterDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements("CreateItem"))
		{
			CreateItemRosterDefinitions.Add(new CreateRosterItemDefinition(item));
		}
	}
}
