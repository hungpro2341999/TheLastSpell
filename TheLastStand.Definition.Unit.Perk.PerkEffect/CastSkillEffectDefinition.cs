using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class CastSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "CastSkill";
	}

	public SkillDefinition SkillDefinition { get; private set; }

	public PerkTargetingDefinition PerkTargetingDefinition { get; private set; }

	public CastSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("SkillId");
		if (SkillDatabase.SkillDefinitions.TryGetValue(xAttribute.Value, out var value))
		{
			SkillDefinition = value;
		}
		else
		{
			CLoggerManager.Log("Skill " + xAttribute.Value + " not found!", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "CastSkillEffectDefinition");
		}
		PerkTargetingDefinition = new PerkTargetingDefinition(xElement.Element("PerkTargeting"), base.TokenVariables);
	}
}
