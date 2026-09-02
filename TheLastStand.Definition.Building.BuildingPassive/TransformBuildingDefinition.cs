using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class TransformBuildingDefinition : BuildingPassiveEffectDefinition
{
	public List<string> BuildingIds { get; private set; }

	public bool Instantaneous { get; private set; }

	public bool PlayDestructionSmoke { get; private set; }

	public TransformBuildingDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingIds = new List<string>();
		foreach (XElement item in xElement.Elements("BuildingId"))
		{
			BuildingIds.Add(item.Value);
		}
		XElement xElement2 = xElement.Element("BuildingListId");
		if (xElement2 != null)
		{
			if (GenericDatabase.IdsListDefinitions.TryGetValue(xElement2.Value, out var value))
			{
				BuildingIds.AddRange(value.Ids);
			}
			else
			{
				CLoggerManager.Log("Could not find a building id list with id: " + xElement2.Value);
			}
		}
		XElement xElement3 = xElement.Element("Instantaneous");
		if (xElement3 != null)
		{
			if (!bool.TryParse(xElement3.Value, out var result))
			{
				CLoggerManager.Log("Could not parse TransformBuildingDefinition Instantaneous value to a valid bool.");
				Instantaneous = false;
			}
			else
			{
				Instantaneous = result;
			}
		}
		XElement xElement4 = xElement.Element("PlayDestructionSmoke");
		if (xElement4 != null)
		{
			if (!bool.TryParse(xElement4.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse TransformBuildingDefinition PlayDestructionSmoke value to a valid bool.");
				PlayDestructionSmoke = false;
			}
			else
			{
				PlayDestructionSmoke = result2;
			}
		}
	}

	public string GetRandomBuildingId()
	{
		int randomRange = RandomManager.GetRandomRange(this, 0, BuildingIds.Count);
		return BuildingIds[randomRange];
	}
}
