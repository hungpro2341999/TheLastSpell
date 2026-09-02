using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SubAreaDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Distance { get; private set; }

	public int Height { get; private set; }

	public int Weight { get; private set; }

	public int Width { get; private set; }

	public SubAreaDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Weight");
		if (xAttribute != null)
		{
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("SubAreas' Weight should be of type integer !", LogType.Error);
				return;
			}
			Weight = result;
		}
		else
		{
			Weight = 1;
		}
		XElement xElement2 = xElement.Element("Distance");
		XElement xElement3 = xElement.Element("Width");
		XElement xElement4 = xElement.Element("Height");
		if (!int.TryParse(xElement2.Value, out var result2))
		{
			CLoggerManager.Log("SubAreas' Distance should be of type integer !", LogType.Error);
			return;
		}
		if (!int.TryParse(xElement3.Value, out var result3))
		{
			CLoggerManager.Log("SubAreas' Width should be of type integer !", LogType.Error);
			return;
		}
		if (!int.TryParse(xElement4.Value, out var result4))
		{
			CLoggerManager.Log("SubAreas' Height should be of type integer !", LogType.Error);
			return;
		}
		Distance = result2;
		Width = result3;
		Height = result4;
	}
}
