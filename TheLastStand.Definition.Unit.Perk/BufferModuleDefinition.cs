using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class BufferModuleDefinition : APerkModuleDefinition
{
	public enum BufferIndex
	{
		Buffer,
		Buffer2,
		Buffer3,
		Buffer4,
		Buffer5,
		Buffer6,
		Buffer7,
		Buffer8,
		Buffer9
	}

	public static class Constants
	{
		public const string Id = "BufferModule";
	}

	public int DefaultBufferValue { get; private set; }

	public BufferModuleDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("DefaultBufferValue");
		if (xAttribute != null)
		{
			if (int.TryParse(xAttribute.Value, out var result))
			{
				DefaultBufferValue = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse DefaultBufferValue attribute into an int : " + xAttribute.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "BufferModuleDefinition");
			}
		}
	}
}
