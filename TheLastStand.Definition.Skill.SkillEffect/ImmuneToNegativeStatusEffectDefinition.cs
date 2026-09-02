using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ImmuneToNegativeStatusEffectDefinition : StatusEffectDefinition
{
	public static class Constants
	{
		public const string Id = "NegativeStatusImmunityEffect";
	}

	public override string Id => "NegativeStatusImmunityEffect";

	public Status.E_StatusType StatusImmunity { get; private set; }

	public override Status.E_StatusType StatusType => StatusImmunity;

	public ImmuneToNegativeStatusEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public static string GetImmuneStatusIconName(Status.E_StatusType statusImmunity)
	{
		switch (statusImmunity)
		{
		case Status.E_StatusType.Debuff:
		case Status.E_StatusType.DebuffImmunity:
			return "Debuff";
		case Status.E_StatusType.Poison:
		case Status.E_StatusType.PoisonImmunity:
			return "Poison";
		case Status.E_StatusType.Stun:
		case Status.E_StatusType.StunImmunity:
			return "Stun";
		case Status.E_StatusType.AllNegative:
		case Status.E_StatusType.AllNegativeImmunity:
			return "NegativeAlterations";
		default:
			return string.Empty;
		}
	}

	public static string GetImmunityStyleId(Status.E_StatusType statusImmunity)
	{
		return statusImmunity switch
		{
			Status.E_StatusType.DebuffImmunity => "DebuffImmunity", 
			Status.E_StatusType.PoisonImmunity => "PoisonImmunity", 
			Status.E_StatusType.StunImmunity => "StunImmunity", 
			Status.E_StatusType.AllNegativeImmunity => "AllNegativeImmunity", 
			_ => string.Empty, 
		};
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Element("Type").Attribute("Id");
		if (Enum.TryParse<Status.E_StatusType>(xAttribute.Value, out var result))
		{
			switch (result)
			{
			case Status.E_StatusType.Debuff:
				StatusImmunity = Status.E_StatusType.DebuffImmunity;
				break;
			case Status.E_StatusType.Poison:
				StatusImmunity = Status.E_StatusType.PoisonImmunity;
				break;
			case Status.E_StatusType.Stun:
				StatusImmunity = Status.E_StatusType.StunImmunity;
				break;
			case Status.E_StatusType.Contagion:
				StatusImmunity = Status.E_StatusType.ContagionImmunity;
				break;
			case Status.E_StatusType.AllNegative:
				StatusImmunity = Status.E_StatusType.AllNegativeImmunity;
				break;
			default:
				CLoggerManager.Log($"For a SkillEffect : ImmuneToNegativeStatusEffect, the Attribute Id of the Element Type should be Stun, Poison, Debuff or AllNegative ! (Wrong Value : {result})", LogType.Error);
				break;
			}
		}
		else
		{
			CLoggerManager.Log("For a SkillEffect : ImmuneToNegativeStatusEffect, the Attribute Id of the Element Type should be of type Model.Status.Status.E_StatusType ! (Wrong Value : " + xAttribute.Value + ")", LogType.Error);
		}
	}
}
