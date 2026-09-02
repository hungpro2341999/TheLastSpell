using System;
using System.Xml.Linq;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.SpawnFx;

public class SpawnVisualEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Path { get; private set; }

	public Node Delay { get; private set; }

	public VisualEffectDefinition.E_Depth SortingDepth { get; private set; }

	public SpawnVisualEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("Path");
		if (!xElement2.IsNullOrEmpty())
		{
			Path = xElement2.Value;
			XElement xElement3 = xElement.Element("Delay");
			Delay = ((xElement3 != null) ? Parser.Parse(xElement3.Value) : new NodeNumber(0.0));
			XElement xElement4 = xElement.Element("SortingDepth");
			if (xElement4 != null && Enum.TryParse<VisualEffectDefinition.E_Depth>(xElement4.Value, out var result))
			{
				SortingDepth = result;
			}
		}
		else
		{
			Debug.LogError("SpawnVisualEffectDefinition must define at least one Path!");
		}
	}
}
