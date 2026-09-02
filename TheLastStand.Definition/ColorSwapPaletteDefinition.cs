using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class ColorSwapPaletteDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class ColorSwapDefinition : TheLastStand.Framework.Serialization.Definition
	{
		public int Index { get; private set; } = -1;

		public Color OutputColor { get; private set; } = Color.cyan;

		public ColorSwapDefinition(XContainer container)
			: base(container)
		{
		}

		public override void Deserialize(XContainer container)
		{
			XElement xElement = container as XElement;
			XElement xElement2 = xElement.Element("Index");
			if (xElement2.IsNullOrEmpty())
			{
				Debug.LogError("A ColorSwap hasn't an Index!");
				return;
			}
			if (!int.TryParse(xElement2.Value, out var result) || result < 0 || result > 99)
			{
				Debug.Log("A ColorSwap " + HasAnInvalidInt(xElement2.Value));
				return;
			}
			Index = result;
			XElement xElement3 = xElement.Element("OutputColor");
			if (!ColorUtility.TryParseHtmlString(xElement3.Value, out var color))
			{
				Debug.Log(string.Format("The ColorSwap with Index : {0} {1}", result, HasAnInvalid("Color", xElement3.Value)));
			}
			else
			{
				OutputColor = color;
			}
		}
	}

	public List<ColorSwapDefinition> ColorSwapDefinitions { get; private set; }

	public string Id { get; private set; }

	public int Weight { get; private set; } = 1;

	public ColorSwapPaletteDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			Debug.LogError("A ColorSwapPalette hasn't an Id!");
			return;
		}
		Id = xAttribute.Value;
		IEnumerable<XElement> enumerable = xElement.Elements("ColorSwap");
		if (!enumerable.Any())
		{
			Debug.LogError("ColorSwapPalette " + Id + " has no ColorSwap at all!");
			return;
		}
		ColorSwapDefinitions = new List<ColorSwapDefinition>(enumerable.Count());
		foreach (XElement item in enumerable)
		{
			ColorSwapDefinition colorSwapDefinition = new ColorSwapDefinition(item);
			if (colorSwapDefinition.Index != -1)
			{
				ColorSwapDefinitions.Add(colorSwapDefinition);
			}
		}
		XAttribute xAttribute2 = xElement.Attribute("Weight");
		if (xAttribute2 != null)
		{
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError("The ColorSwapPalette : " + Id + " " + HasAnInvalidInt(xAttribute2.Value));
			}
			else
			{
				Weight = result;
			}
		}
	}
}
