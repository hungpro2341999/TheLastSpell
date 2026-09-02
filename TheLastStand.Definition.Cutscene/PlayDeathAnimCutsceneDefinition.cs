using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class PlayDeathAnimCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayDeathAnim";
	}

	public bool WaitDeathAnim { get; private set; }

	public PlayDeathAnimCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("WaitDeathAnim");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				WaitDeathAnim = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}
}
