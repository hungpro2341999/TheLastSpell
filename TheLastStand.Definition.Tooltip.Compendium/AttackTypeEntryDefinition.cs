using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class AttackTypeEntryDefinition : ACompendiumEntryDefinition
{
	public static class Constants
	{
		public const string Name = "AttackTypeEntry";
	}

	public AttackSkillActionDefinition.E_AttackType AttackType { get; private set; }

	public AttackTypeEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("AttackType");
		AttackSkillActionDefinition.E_AttackType result2;
		if (xElement != null)
		{
			if (Enum.TryParse<AttackSkillActionDefinition.E_AttackType>(xElement.Value, out var result))
			{
				AttackType = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse AttackType element into a E_AttackType : " + xElement.Value, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "AttackTypeEntryDefinition");
			}
		}
		else if (Enum.TryParse<AttackSkillActionDefinition.E_AttackType>(base.Id, out result2))
		{
			AttackType = result2;
		}
	}
}
