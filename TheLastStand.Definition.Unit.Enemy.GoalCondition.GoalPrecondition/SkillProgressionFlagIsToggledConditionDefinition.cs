using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.GoalCondition.GoalPrecondition;

public class SkillProgressionFlagIsToggledConditionDefinition : GoalConditionDefinition
{
	public const string Name = "SkillProgressionFlagIsToggled";

	public GlyphManager.E_SkillProgressionFlag SkillProgressionFlag { get; private set; }

	public SkillProgressionFlagIsToggledConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Flag");
		if (!Enum.TryParse<GlyphManager.E_SkillProgressionFlag>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("SkillProgressionFlagIsToggled Unable to parse " + xAttribute.Value + " (" + xAttribute.Value + ") into a E_SkillProgressionFlag", LogType.Error, CLogLevel.MAJOR);
		}
		SkillProgressionFlag = result;
	}
}
