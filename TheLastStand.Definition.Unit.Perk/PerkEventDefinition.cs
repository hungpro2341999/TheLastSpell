using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class PerkEventDefinition : TheLastStand.Framework.Serialization.Definition
{
	public E_EffectTime EffectTime { get; private set; }

	public List<APerkActionDefinition> PerkActionDefinitions { get; private set; }

	public PerkDataConditionsDefinition PerkDataConditionsDefinition { get; private set; }

	public PerkEventDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("EffectTime");
		if (Enum.TryParse<E_EffectTime>(xAttribute.Value, out var result))
		{
			EffectTime = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse EffectTime attribute into an E_EffectTime : \"" + xAttribute.Value + "\".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkEventDefinition");
		}
		PerkDataConditionsDefinition = new PerkDataConditionsDefinition(xElement.Element("Conditions"), base.TokenVariables);
		PerkActionDefinitions = new List<APerkActionDefinition>();
		foreach (XElement item in xElement.Element("Actions").Elements())
		{
			switch (item.Name.LocalName)
			{
			case "DecreaseBuffer":
				PerkActionDefinitions.Add(new DecreaseBufferDefinition(item, base.TokenVariables));
				break;
			case "IncreaseBuffer":
				PerkActionDefinitions.Add(new IncreaseBufferDefinition(item, base.TokenVariables));
				break;
			case "SetBufferTo":
				PerkActionDefinitions.Add(new SetBufferDefinition(item, base.TokenVariables));
				break;
			case "TriggerEffects":
				PerkActionDefinitions.Add(new TriggerEffectsDefinition(item, base.TokenVariables));
				break;
			case "TriggerEffectsOnAllAttackData":
				PerkActionDefinitions.Add(new TriggerEffectsOnAllAttackDataDefinition(item, base.TokenVariables));
				break;
			case "InstantiateStatEffectDisplay":
				PerkActionDefinitions.Add(new InstantiateStatEffectDisplayDefinition(item, base.TokenVariables));
				break;
			case "InstantiateRestoreEffectDisplay":
				PerkActionDefinitions.Add(new InstantiateRestoreEffectDisplayDefinition(item, base.TokenVariables));
				break;
			case "InstantiateBuffEffectDisplay":
				PerkActionDefinitions.Add(new InstantiateBuffEffectDisplayDefinition(item, base.TokenVariables));
				break;
			case "RefreshPerkActivationFeedback":
				PerkActionDefinitions.Add(new RefreshPerkActivationFeedbackDefinition(item, base.TokenVariables));
				break;
			case "ForbidSkillUndo":
				PerkActionDefinitions.Add(new ForbidSkillUndoDefinition(item, base.TokenVariables));
				break;
			case "RefillGauge":
				PerkActionDefinitions.Add(new RefillGaugeDefinition(item, base.TokenVariables));
				break;
			case "Log":
				PerkActionDefinitions.Add(new LogPerkActionDefinition(item, base.TokenVariables));
				break;
			default:
				CLoggerManager.Log("Trying to Deserialize an unimplemented PerkAction : " + item.Name.LocalName, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkEventDefinition");
				break;
			}
		}
	}
}
