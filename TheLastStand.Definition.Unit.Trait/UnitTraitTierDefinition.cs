using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Trait;

public class UnitTraitTierDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string DefaultTierId = "Default";
	}

	public HashSet<int> Costs { get; private set; }

	public string Id { get; private set; }

	public bool IsBackground { get; private set; }

	public UnitTraitTierDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("IsBackground");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result))
			{
				IsBackground = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse IsBackground into a bool for UnitTraitTierDefinition \"" + Id + "\".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "UnitTraitTierDefinition");
			}
		}
		Costs = new HashSet<int>();
		foreach (XElement item in obj.Elements("Cost"))
		{
			if (int.TryParse(item.Attribute("Value").Value, out var result2))
			{
				Costs.Add(result2);
			}
			else
			{
				CLoggerManager.Log("Could not parse Cost Value attribute into an int in UnitTraitTierDefinition \"" + Id + "\".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "UnitTraitTierDefinition");
			}
		}
	}
}
