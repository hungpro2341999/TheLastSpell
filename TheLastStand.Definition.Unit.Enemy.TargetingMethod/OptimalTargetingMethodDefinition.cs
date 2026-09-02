using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class OptimalTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "Optimal";

	public Dictionary<DamageableType, int> DamageableTypesWeight { get; private set; }

	public List<(string[] ids, int weight)> DamageableIdsWeight { get; private set; }

	public OptimalTargetingMethodDefinition(XContainer container = null)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		foreach (XElement item in xElement.Elements("DamageableType"))
		{
			if (DamageableTypesWeight == null)
			{
				Dictionary<DamageableType, int> dictionary = (DamageableTypesWeight = new Dictionary<DamageableType, int>());
			}
			XAttribute xAttribute = item.Attribute("Weight");
			int result = 1;
			if (xAttribute != null && !int.TryParse(xAttribute.Value, out result))
			{
				CLoggerManager.Log("GoalConditionDefinition Optimal UnitType is incorrect: " + xAttribute.Value + " is not a valid Integer", LogType.Error);
			}
			if (Enum.TryParse<DamageableType>(item.Value, out var result2))
			{
				DamageableTypesWeight.Add(result2, result);
			}
			else
			{
				CLoggerManager.Log("GoalConditionDefinition Optimal UnitType is incorrect: " + item.Value + " is not a valid DamageableType", LogType.Error);
			}
		}
		foreach (XElement item2 in xElement.Elements("DamageableId"))
		{
			if (DamageableIdsWeight == null)
			{
				List<(string[], int)> list = (DamageableIdsWeight = new List<(string[], int)>());
			}
			XAttribute xAttribute2 = item2.Attribute("Weight");
			int result3 = 1;
			if (xAttribute2 != null && !int.TryParse(xAttribute2.Value, out result3))
			{
				CLoggerManager.Log("GoalConditionDefinition Optimal UnitType is incorrect: " + xAttribute2.Value + " is not a valid Integer", LogType.Error);
			}
			List<string> list3 = new List<string>();
			foreach (XElement item3 in item2.Elements("IdList"))
			{
				XAttribute xAttribute3 = item3.Attribute("Value");
				foreach (string id in GenericDatabase.IdsListDefinitions[xAttribute3.Value].Ids)
				{
					if (!list3.Contains(id))
					{
						list3.Add(id);
					}
				}
			}
			foreach (XElement item4 in item2.Elements("Id"))
			{
				XAttribute xAttribute4 = item4.Attribute("Value");
				if (!list3.Contains(xAttribute4.Value))
				{
					list3.Add(xAttribute4.Value);
				}
			}
			DamageableIdsWeight.Add((list3.ToArray(), result3));
		}
	}
}
