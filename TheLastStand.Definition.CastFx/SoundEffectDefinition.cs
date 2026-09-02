using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public class SoundEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Node Delay { get; private set; }

	public string FolderPath { get; private set; } = string.Empty;

	public string Path { get; private set; } = string.Empty;

	public Dictionary<string, int> RandomPaths { get; } = new Dictionary<string, int>();

	public bool IsSpatialized { get; private set; } = true;

	public SoundEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("FolderPath");
		XElement xElement2 = container.Element("Path");
		XElement xElement3 = container.Element("RandomPaths");
		if (xElement == null && xElement2 == null && xElement3 == null)
		{
			CLoggerManager.Log("The SoundEffect must have a FolderPath, a Path or RandomPaths", LogType.Error);
			return;
		}
		if (xElement != null)
		{
			FolderPath = xElement.Value;
		}
		else if (xElement2 != null)
		{
			Path = xElement2.Value;
		}
		else
		{
			foreach (XElement item in xElement3.Elements("RandomPath"))
			{
				XAttribute xAttribute = item.Attribute("Path");
				if (RandomPaths.ContainsKey(xAttribute.Value))
				{
					CLoggerManager.Log("Random paths already contains path " + xAttribute.Value + "!", LogType.Warning);
					continue;
				}
				if (!int.TryParse(item.Attribute("Weight").Value, out var result))
				{
					CLoggerManager.Log("Random path's weight should be of type float!", LogType.Error);
					return;
				}
				RandomPaths.Add(xAttribute.Value, result);
			}
		}
		XElement xElement4 = container.Element("Delay");
		Delay = ((xElement4 != null) ? Parser.Parse(xElement4.Value) : new NodeNumber(0.0));
		XElement xElement5 = container.Element("IsSpatialized");
		if (xElement5 != null)
		{
			if (!bool.TryParse(xElement5.Value, out var result2))
			{
				CLoggerManager.Log("IsSpatialized should be of type bool!", LogType.Error);
			}
			IsSpatialized = result2;
		}
	}
}
