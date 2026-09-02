using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class InitUnitVisualsCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "InitUnitVisuals";
	}

	public bool CastSpawnSkill { get; private set; } = true;

	public bool PlaySpawnAnim { get; private set; } = true;

	public bool WaitSpawnAnim { get; private set; } = true;

	public bool WaitAppearanceDelay { get; private set; } = true;

	public InitUnitVisualsCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("CastSpawnSkill");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				CastSpawnSkill = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse InitUnitVisualsCutsceneDefinition castSpawnSkill value " + xAttribute.Value + " as a valid bool value.");
				CastSpawnSkill = true;
			}
		}
		XAttribute xAttribute2 = obj.Attribute("PlaySpawnAnim");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result2))
			{
				PlaySpawnAnim = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse InitUnitVisualsCutsceneDefinition playSpawnAnim value " + xAttribute2.Value + " as a valid bool value.");
				PlaySpawnAnim = true;
			}
		}
		XAttribute xAttribute3 = obj.Attribute("WaitSpawnAnim");
		if (xAttribute3 != null)
		{
			if (bool.TryParse(xAttribute3.Value, out var result3))
			{
				WaitSpawnAnim = result3;
			}
			else
			{
				CLoggerManager.Log("Could not parse InitUnitVisualsCutsceneDefinition waitSpawnAnim value " + xAttribute3.Value + " as a valid bool value.");
				WaitSpawnAnim = true;
			}
		}
		XAttribute xAttribute4 = obj.Attribute("WaitAppearanceDelay");
		if (xAttribute4 != null)
		{
			if (bool.TryParse(xAttribute4.Value, out var result4))
			{
				WaitAppearanceDelay = result4;
				return;
			}
			CLoggerManager.Log("Could not parse InitUnitVisualsCutsceneDefinition waitAppearanceDelay value " + xAttribute4.Value + " as a valid bool value.");
			WaitAppearanceDelay = true;
		}
	}
}
