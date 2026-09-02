using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TargetHasBuildingIdConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TargetHasBuildingId";

	public bool Exclude { get; private set; }

	public string[] BuildingIds { get; private set; }

	public TargetHasBuildingIdConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Exclude");
		if (xAttribute != null)
		{
			Exclude = bool.Parse(xAttribute.Value);
		}
		List<string> list = new List<string>();
		foreach (XElement item in xElement.Elements("BuildingsListId"))
		{
			XAttribute xAttribute2 = item.Attribute("Value");
			foreach (string id in GenericDatabase.IdsListDefinitions[xAttribute2.Value].Ids)
			{
				if (!list.Contains(id))
				{
					list.Add(id);
				}
			}
		}
		foreach (XElement item2 in xElement.Elements("BuildingId"))
		{
			XAttribute xAttribute3 = item2.Attribute("Value");
			if (!list.Contains(xAttribute3.Value))
			{
				list.Add(xAttribute3.Value);
			}
		}
		BuildingIds = list.ToArray();
	}
}
