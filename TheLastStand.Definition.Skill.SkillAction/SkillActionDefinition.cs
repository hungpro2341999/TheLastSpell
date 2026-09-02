using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public abstract class SkillActionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public bool ApplyOnCaster { get; set; }

	public Dictionary<string, List<SkillEffectDefinition>> SkillEffectDefinitions { get; private set; }

	public SkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public static SkillEffectDefinition DeserializeSkillEffect(XElement skillEffectElement, Dictionary<string, string> tokenVariables = null)
	{
		return skillEffectElement.Name.LocalName switch
		{
			"ArmorPiercing" => new ArmorPiercingEffectDefinition(skillEffectElement), 
			"ArmorShredding" => new ArmorShreddingEffectDefinition(skillEffectElement), 
			"Buff" => new BuffEffectDefinition(skillEffectElement, tokenVariables), 
			"CasterEffect" => new CasterEffectDefinition(skillEffectElement, tokenVariables), 
			"Charged" => new ChargedEffectDefinition(skillEffectElement, tokenVariables), 
			"Contagion" => new ContagionEffectDefinition(skillEffectElement, tokenVariables), 
			"Debuff" => new DebuffEffectDefinition(skillEffectElement, tokenVariables), 
			"ExileCaster" => new ExileCasterEffectDefinition(skillEffectElement), 
			"Follow" => new FollowSkillEffectDefinition(skillEffectElement), 
			"IgnoreLineOfSight" => new IgnoreLineOfSightEffectDefinition(skillEffectElement), 
			"IgnoreLineOfSightInCityTiles" => new IgnoreLineOfSightInCityTilesEffectDefinition(skillEffectElement), 
			"Inaccurate" => new InaccurateSkillEffectDefinition(skillEffectElement, tokenVariables), 
			"Isolated" => new IsolatedSkillEffectDefinition(skillEffectElement, tokenVariables), 
			"Kill" => new KillSkillEffectDefinition(skillEffectElement), 
			"Maneuver" => new ManeuverSkillEffectDefinition(skillEffectElement), 
			"ModifyAffectedUnits" => new ModifyAffectedUnitsEffectDefinition(skillEffectElement, tokenVariables), 
			"Momentum" => new MomentumEffectDefinition(skillEffectElement, tokenVariables), 
			"MultiHit" => new MultiHitSkillEffectDefinition(skillEffectElement, tokenVariables), 
			"NoBlock" => new NoBlockEffectDefinition(skillEffectElement), 
			"NoDodge" => new NoDodgeEffectDefinition(skillEffectElement), 
			"NoMomentum" => new NoMomentumEffectDefinition(skillEffectElement), 
			"Opportunistic" => new OpportunisticSkillEffectDefinition(skillEffectElement, tokenVariables), 
			"Poison" => new PoisonEffectDefinition(skillEffectElement, tokenVariables), 
			"Propagation" => new PropagationSkillEffectDefinition(skillEffectElement, tokenVariables), 
			"RemoveStatus" => new RemoveStatusEffectDefinition(skillEffectElement), 
			"RegenStat" => new RegenStatSkillEffectDefinition(skillEffectElement, tokenVariables), 
			"DecreaseStat" => new DecreaseStatSkillEffectDefinition(skillEffectElement, tokenVariables), 
			"Stun" => new StunEffectDefinition(skillEffectElement, tokenVariables), 
			"SurroundingEffect" => new SurroundingEffectDefinition(skillEffectElement, tokenVariables), 
			"NegativeStatusImmunityEffect" => new ImmuneToNegativeStatusEffectDefinition(skillEffectElement, tokenVariables), 
			"ResupplyCharges" => new ResupplyChargesSkillEffectDefinition(skillEffectElement), 
			"ResupplyOverallUses" => new ResupplyOverallUsesSkillEffectDefinition(skillEffectElement), 
			"ResupplySkills" => new ResupplySkillsSkillEffectDefinition(skillEffectElement), 
			"ExtinguishBrazier" => new ExtinguishBrazierSkillEffectDefinition(skillEffectElement), 
			_ => null, 
		};
	}

	public List<SkillEffectDefinition> GetEffects(string effectId)
	{
		if (SkillEffectDefinitions != null && SkillEffectDefinitions.TryGetValue(effectId, out var value))
		{
			if (value.Count <= 0)
			{
				return null;
			}
			return value;
		}
		return null;
	}

	public List<T> GetEffects<T>(string effectId) where T : SkillEffectDefinition
	{
		if (SkillEffectDefinitions == null || !SkillEffectDefinitions.TryGetValue(effectId, out var value))
		{
			return null;
		}
		if (value.Count == 0)
		{
			return null;
		}
		List<T> list = new List<T>();
		for (int i = 0; i < value.Count; i++)
		{
			if (value[i] is T)
			{
				list.Add(value[i] as T);
			}
		}
		if (list.Count != 0)
		{
			return list;
		}
		return null;
	}

	public T GetFirstEffect<T>(string effectId) where T : SkillEffectDefinition
	{
		List<SkillEffectDefinition> effects = GetEffects(effectId);
		if (effects == null || effects.Count <= 0)
		{
			return null;
		}
		return effects[0] as T;
	}

	public bool HasAnyEffect(IEnumerable<string> effectIds)
	{
		foreach (string effectId in effectIds)
		{
			if (HasEffect(effectId))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEffect(string effectId)
	{
		if (SkillEffectDefinitions != null)
		{
			return SkillEffectDefinitions.ContainsKey(effectId);
		}
		return false;
	}

	public bool HasEffect<T>() where T : SkillEffectDefinition
	{
		foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition in SkillEffectDefinitions)
		{
			foreach (SkillEffectDefinition item in skillEffectDefinition.Value)
			{
				if (item is T)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("ApplyOnCaster");
		if (!xAttribute.IsNullOrEmpty())
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				ApplyOnCaster = result;
			}
			else
			{
				CLoggerManager.Log("The skill must have a valid ApplyOnCaster", LogType.Error);
			}
		}
		foreach (XElement item in obj.Elements())
		{
			XElement xElement = item.Element("SkillEffects");
			if (xElement == null)
			{
				continue;
			}
			foreach (XElement item2 in xElement.Elements())
			{
				SkillEffectDefinition skillEffectDefinition = DeserializeSkillEffect(item2);
				AddEffect(skillEffectDefinition);
			}
		}
	}

	public bool TryGetAllEffects<T>(string effectId, out List<T> effects) where T : SkillEffectDefinition
	{
		effects = GetEffects<T>(effectId);
		return effects != null;
	}

	public bool TryGetFirstEffect<T>(string effectId, out T effect) where T : SkillEffectDefinition
	{
		effect = GetFirstEffect<T>(effectId);
		return effect != null;
	}

	private void AddEffect(SkillEffectDefinition skillEffectDefinition)
	{
		if (SkillEffectDefinitions == null)
		{
			SkillEffectDefinitions = new Dictionary<string, List<SkillEffectDefinition>>();
		}
		if (!SkillEffectDefinitions.TryGetValue(skillEffectDefinition.Id, out var value))
		{
			value = new List<SkillEffectDefinition>();
			SkillEffectDefinitions.Add(skillEffectDefinition.Id, value);
		}
		value.Add(skillEffectDefinition);
	}
}
