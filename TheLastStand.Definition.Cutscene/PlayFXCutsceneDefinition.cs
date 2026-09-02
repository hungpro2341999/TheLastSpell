using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.SpawnFx;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class PlayFXCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayFX";
	}

	public SpawnFxDefinition SpawnFxDefinition { get; private set; }

	public int? TileX { get; private set; }

	public int? TileY { get; private set; }

	public bool WaitForFXDuration { get; private set; }

	public bool SpecifiedTilePosition
	{
		get
		{
			if (TileX.HasValue)
			{
				return TileY.HasValue;
			}
			return false;
		}
	}

	public PlayFXCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("SpawnFXs");
		if (xElement != null)
		{
			SpawnFxDefinition = new SpawnFxDefinition(xElement);
		}
		XAttribute xAttribute = obj.Attribute("WaitForFXDuration");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				WaitForFXDuration = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		XAttribute xAttribute2 = obj.Attribute("TileX");
		if (xAttribute2 != null)
		{
			if (int.TryParse(xAttribute2.Value, out var result2))
			{
				TileX = result2;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into int.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		XAttribute xAttribute3 = obj.Attribute("TileY");
		if (xAttribute3 != null)
		{
			if (int.TryParse(xAttribute3.Value, out var result3))
			{
				TileY = result3;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute3.Value + " into int.", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}
