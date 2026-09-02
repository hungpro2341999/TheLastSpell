using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class RangeDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public Vector2Int Range { get; private set; }

	public RangeDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		int x = 0;
		int y = -1;
		XAttribute xAttribute2 = obj.Attribute("Min");
		if (xAttribute2 != null)
		{
			if (int.TryParse(xAttribute2.Value, out var result))
			{
				x = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse Min attribute into an int in RangeDefinition : " + xAttribute2.Value);
			}
		}
		XAttribute xAttribute3 = obj.Attribute("Max");
		if (xAttribute3 != null)
		{
			if (int.TryParse(xAttribute3.Value, out var result2))
			{
				y = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse Max attribute into an int in RangeDefinition : " + xAttribute3.Value);
			}
		}
		Range = new Vector2Int(x, y);
	}
}
