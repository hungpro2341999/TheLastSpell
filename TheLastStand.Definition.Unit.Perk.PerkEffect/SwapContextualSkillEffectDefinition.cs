using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class SwapContextualSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "SwapContextualSkill";
	}

	public string ContextualSkillIdToLock { get; private set; }

	public string ContextualSkillIdToUnlock { get; private set; }

	public int OverallUses { get; private set; } = -1;

	public SwapContextualSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("ContextualSkillIdToLock");
		ContextualSkillIdToLock = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("ContextualSkillIdToUnlock");
		ContextualSkillIdToUnlock = xAttribute2.Value;
		XAttribute xAttribute3 = obj.Attribute("OverallUses");
		if (xAttribute3 != null)
		{
			if (int.TryParse(xAttribute3.Value, out var result))
			{
				OverallUses = result;
			}
			else
			{
				CLoggerManager.Log("Found a OverallUses for SwapContextualSkill but the int parsing failed. Safely assigning null.", LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "SwapContextualSkillEffectDefinition");
			}
		}
	}
}
