using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalTargetCondition;

public class TargetIdConditionDefinition : GoalConditionDefinition
{
	public const string Name = "TargetId";

	public bool Exclude { get; private set; }

	public string[] TargetIds { get; private set; }

	public TargetIdConditionDefinition(XContainer container)
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
		foreach (XElement item in xElement.Elements("TargetsListId"))
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
		foreach (XElement item2 in xElement.Elements("TargetId"))
		{
			XAttribute xAttribute3 = item2.Attribute("Value");
			if (!list.Contains(xAttribute3.Value))
			{
				list.Add(xAttribute3.Value);
			}
		}
		TargetIds = list.ToArray();
	}
}
