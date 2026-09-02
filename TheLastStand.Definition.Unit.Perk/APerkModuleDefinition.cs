using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkCondition;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public abstract class APerkModuleDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<APerkConditionDefinition> PerkConditionDefinitions { get; private set; } = new List<APerkConditionDefinition>();

	public List<PerkEventDefinition> PerkEventDefinitions { get; private set; } = new List<PerkEventDefinition>();

	public List<APerkEffectDefinition> PerkEffectDefinitions { get; private set; } = new List<APerkEffectDefinition>();

	public APerkModuleDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("Conditions");
		if (xElement != null)
		{
			DeserializeConditions(xElement);
		}
		XElement xElement2 = obj.Element("Events");
		if (xElement2 != null)
		{
			DeserializeEvents(xElement2);
		}
		XElement xElement3 = obj.Element("Effects");
		if (xElement3 != null)
		{
			DeserializeEffects(xElement3);
		}
	}

	protected void DeserializeConditions(XElement xConditions)
	{
		foreach (XElement item in xConditions.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "IsTrue":
				PerkConditionDefinitions.Add(new IsTrueConditionDefinition(item, base.TokenVariables));
				break;
			case "IsFalse":
				PerkConditionDefinitions.Add(new IsFalseConditionDefinition(item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log("Tried to Deserialize an unimplemented PerkCondition: " + item.Name.LocalName, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "APerkModuleDefinition");
				break;
			}
		}
	}

	protected void DeserializeEvents(XElement xEvents)
	{
		foreach (XElement item in xEvents.Elements("Event"))
		{
			PerkEventDefinitions.Add(new PerkEventDefinition(item, base.TokenVariables));
		}
	}

	protected void DeserializeEffects(XElement xEffects)
	{
		foreach (XElement item in xEffects.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "StatModifier":
				PerkEffectDefinitions.Add(new StatModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "PermanentBaseStatModifier":
				PerkEffectDefinitions.Add(new PermanentBaseStatModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "StatLocker":
				PerkEffectDefinitions.Add(new StatLockerEffectDefinition(item, base.TokenVariables));
				break;
			case "DynamicStatsModifier":
				PerkEffectDefinitions.Add(new DynamicStatsModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "ComputationStatLocker":
				PerkEffectDefinitions.Add(new ComputationStatLockerEffectDefinition(item, base.TokenVariables));
				break;
			case "SkillModifier":
				PerkEffectDefinitions.Add(new SkillModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "AddSkillEffect":
				PerkEffectDefinitions.Add(new AddSkillEffectDefinition(item, base.TokenVariables));
				break;
			case "AddSkill":
				PerkEffectDefinitions.Add(new AddPerkSkillEffectDefinition(item, base.TokenVariables));
				break;
			case "LockSkill":
				PerkEffectDefinitions.Add(new LockSkillEffectDefinition(item, base.TokenVariables));
				break;
			case "AllowDiagonalPropagation":
				PerkEffectDefinitions.Add(new AllowDiagonalPropagationEffectDefinition(item, base.TokenVariables));
				break;
			case "UnlockContextualSkill":
				PerkEffectDefinitions.Add(new UnlockContextualSkillEffectDefinition(item, base.TokenVariables));
				break;
			case "SwapContextualSkill":
				PerkEffectDefinitions.Add(new SwapContextualSkillEffectDefinition(item, base.TokenVariables));
				break;
			case "ResetBuffer":
				PerkEffectDefinitions.Add(new ResetBufferEffectDefinition(item, base.TokenVariables));
				break;
			case "RestoreStat":
				PerkEffectDefinitions.Add(new RestoreStatEffectDefinition(item, base.TokenVariables));
				break;
			case "ApplyStatus":
				PerkEffectDefinitions.Add(new ApplyStatusEffectDefinition(item, base.TokenVariables));
				break;
			case "CastSkill":
				PerkEffectDefinitions.Add(new CastSkillEffectDefinition(item, base.TokenVariables));
				break;
			case "DealDamage":
				PerkEffectDefinitions.Add(new DealDamageEffectDefinition(item, base.TokenVariables));
				break;
			case "ModifyDefensesDamage":
				PerkEffectDefinitions.Add(new ModifyDefensesDamageEffectDefinition(item, base.TokenVariables));
				break;
			case "GetAdditionalExperience":
				PerkEffectDefinitions.Add(new GetAdditionalExperienceEffectDefinition(item, base.TokenVariables));
				break;
			case "EquipmentSlotModifier":
				PerkEffectDefinitions.Add(new EquipmentSlotModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "RestoreUses":
				PerkEffectDefinitions.Add(new RestoreUsesEffectDefinition(item, base.TokenVariables));
				break;
			case "ReplacePerk":
				PerkEffectDefinitions.Add(new ReplacePerkEffectDefinition(item, base.TokenVariables));
				break;
			case "AttackDataModifier":
				PerkEffectDefinitions.Add(new AttackDataModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "ReplaceItemSkill":
				PerkEffectDefinitions.Add(new ReplaceItemSkillEffectDefinition(item, base.TokenVariables));
				break;
			case "StatBoundaryModifier":
				PerkEffectDefinitions.Add(new StatBoundaryModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "MomentumModifier":
				PerkEffectDefinitions.Add(new MomentumModifierEffectDefinition(item, base.TokenVariables));
				break;
			case "ModifyContextualSkillOverallUses":
				PerkEffectDefinitions.Add(new ModifyContextualSkillOverallUsesEffectDefinition(item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log("Tried to Deserialize an unimplemented PerkEffect: " + item.Name.LocalName, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "APerkModuleDefinition");
				break;
			}
		}
	}
}
