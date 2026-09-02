using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AddPerkSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AddSkill";
	}

	public SkillDefinition SkillDefinition { get; private set; }

	public int UsesPerNight { get; private set; } = -1;

	public int UsesPerTurn { get; private set; } = -1;

	public AddPerkSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("SkillId");
		if (SkillDatabase.SkillDefinitions.TryGetValue(xAttribute.Value, out var value))
		{
			SkillDefinition = value;
		}
		else
		{
			CLoggerManager.Log("Skill " + xAttribute.Value + " not found!", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "AddPerkSkillEffectDefinition");
		}
		XAttribute xAttribute2 = obj.Attribute("UsesPerNight");
		if (xAttribute2 != null)
		{
			if (int.TryParse(xAttribute2.Value, out var result))
			{
				UsesPerNight = result;
			}
			else
			{
				CLoggerManager.Log("Found a UsesPerNight for AddSkill but the int parsing failed. Assigning -1 by default.", LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "AddPerkSkillEffectDefinition");
				UsesPerNight = -1;
			}
		}
		XAttribute xAttribute3 = obj.Attribute("UsesPerTurn");
		if (xAttribute3 != null)
		{
			if (int.TryParse(xAttribute3.Value, out var result2))
			{
				UsesPerTurn = result2;
				return;
			}
			CLoggerManager.Log("Found a UsesPerTurn for AddSkill but the int parsing failed. Assigning -1 by default.", LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "AddPerkSkillEffectDefinition");
			UsesPerTurn = -1;
		}
		else
		{
			CLoggerManager.Log("UsesPerTurn was not specified for AddSkill. Assigning the SkillDefinition value by default.", LogType.Log, CLogLevel.NORMAL, forcePrintInUnity: true, "AddPerkSkillEffectDefinition");
			UsesPerTurn = SkillDefinition.UsesPerTurnCount;
		}
	}
}
