using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class ReplaceItemSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ReplaceItemSkill";
	}

	public bool HasUsesPerTurnLinked => !string.IsNullOrEmpty(LinkedSkillIdForUsesPerTurn);

	public int OverallUses { get; private set; } = -1;

	public string SkillIdToReplace { get; private set; }

	public string SkillIdReplacement { get; private set; }

	public string LinkedSkillIdForUsesPerTurn { get; private set; }

	public ReplaceItemSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("SkillIdToReplace");
		SkillIdToReplace = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("SkillIdReplacement");
		SkillIdReplacement = xAttribute2.Value;
		XAttribute xAttribute3 = obj.Attribute("LinkedSkillIdForUsesPerTurn");
		if (xAttribute3 != null)
		{
			LinkedSkillIdForUsesPerTurn = xAttribute3.Value;
		}
		XAttribute xAttribute4 = obj.Attribute("OverallUses");
		if (xAttribute4 != null)
		{
			if (int.TryParse(xAttribute4.Value, out var result))
			{
				OverallUses = result;
			}
			else
			{
				CLoggerManager.Log("Found a OverallUses for ReplaceItemSkill but the int parsing failed.", LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "ReplaceItemSkillEffectDefinition");
			}
		}
	}
}
