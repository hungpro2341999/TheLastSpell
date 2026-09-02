using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class FocusTileCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "FocusTile";
	}

	public int PosX { get; private set; }

	public int PosY { get; private set; }

	public FocusTileCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("X");
		if (int.TryParse(xAttribute.Value, out var result))
		{
			PosX = result;
		}
		else
		{
			CLoggerManager.Log("Unable to parse " + xAttribute.Value + " into int.", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute2 = obj.Attribute("Y");
		if (int.TryParse(xAttribute2.Value, out var result2))
		{
			PosY = result2;
		}
		else
		{
			CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into int.", LogType.Error, CLogLevel.MAJOR);
		}
	}
}
