using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public abstract class VisualEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_Depth
	{
		Before,
		Behind,
		Dynamic,
		TargetDynamic,
		AboveAll
	}

	protected Dictionary<int, string> paths = new Dictionary<int, string>(4);

	protected Dictionary<int, string> pathsMirroredOnYAxis = new Dictionary<int, string>(4);

	public bool CanSkipIfMissingOrientation { get; private set; }

	public E_Depth SortingDepth { get; private set; }

	public Node Delay { get; private set; }

	public VisualEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement)?.Attribute("CanSkipIfMissingOrientation");
		if (xAttribute != null)
		{
			if (!bool.TryParse(xAttribute.Value, out var result))
			{
				Debug.LogError($"VisualEffectDefinition has an invalid CanSkipIfMissingOrientation value ! canSkipIfMissingOrientation: {result}");
			}
			CanSkipIfMissingOrientation = result;
		}
		XElement xElement = container.Element("Paths");
		if (!xElement.IsNullOrEmpty())
		{
			foreach (XElement item in xElement.Elements("Path"))
			{
				if (!item.IsNullOrEmpty())
				{
					XAttribute xAttribute2 = item.Attribute("Orientation");
					string value = item.Value;
					XAttribute xAttribute3 = item.Attribute("Flipped");
					bool result2 = false;
					if (xAttribute3 != null && !bool.TryParse(xAttribute3.Value, out result2))
					{
						Debug.LogError("Invalid Flipped value for path: " + value);
					}
					Dictionary<int, string> dictionary = ((!result2) ? paths : pathsMirroredOnYAxis);
					if (xAttribute2.IsNullOrEmpty())
					{
						if (value.EndsWith("_", StringComparison.OrdinalIgnoreCase))
						{
							dictionary[2] = value + "E";
							dictionary[0] = value + "N";
							dictionary[1] = value + "S";
							dictionary[3] = value + "W";
						}
						else
						{
							dictionary[2] = value;
							dictionary[0] = value;
							dictionary[1] = value;
							dictionary[3] = value;
						}
						continue;
					}
					string value2 = xAttribute2.Value;
					int i = 0;
					for (int length = value2.Length; i < length; i++)
					{
						switch (value2[i])
						{
						case 'E':
							dictionary[2] = value;
							break;
						case 'N':
							dictionary[0] = value;
							break;
						case 'S':
							dictionary[1] = value;
							break;
						case 'W':
							dictionary[3] = value;
							break;
						default:
							Debug.LogError($"VisualEffectDefinition (Path='{value}') defines an invalid orientation (char '{value2[i]}')!");
							break;
						}
					}
					continue;
				}
				Debug.LogError("VisualEffectDefinition has an invalid Path entry!");
				return;
			}
			XElement xElement2 = container.Element("Delay");
			Delay = ((xElement2 != null) ? Parser.Parse(xElement2.Value) : new NodeNumber(0.0));
			XElement xElement3 = container.Element("SortingDepth");
			if (xElement3 != null)
			{
				if (!Enum.TryParse<E_Depth>(xElement3.Value, out var result3))
				{
					Debug.LogError("VisualEffectDefinition (Path='" + paths[0] + "') defines an invalid SortingDepth (" + xElement3.Value + ")!");
				}
				else
				{
					SortingDepth = result3;
				}
			}
			else
			{
				Debug.LogError("VisualEffectDefinition (Path='" + paths[0] + "') must define a SortingDepth!");
			}
		}
		else
		{
			Debug.LogError("VisualEffectDefinition must define at least one Path!");
		}
	}

	public string GetPath(GameDefinition.E_Direction direction, bool isMirroredOnYAxis = false)
	{
		if (isMirroredOnYAxis && pathsMirroredOnYAxis.ContainsKey((int)direction))
		{
			return pathsMirroredOnYAxis[(int)direction];
		}
		if (paths.TryGetValue((int)direction, out var value))
		{
			return value;
		}
		return null;
	}

	public string GetAnyValidPath(bool isMirroredOnYAxis = false)
	{
		Dictionary<int, string> dictionary = (isMirroredOnYAxis ? pathsMirroredOnYAxis : paths);
		using (Dictionary<int, string>.KeyCollection.Enumerator enumerator = dictionary.Keys.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				int current = enumerator.Current;
				return dictionary[current];
			}
		}
		return string.Empty;
	}
}
