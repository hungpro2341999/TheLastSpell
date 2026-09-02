using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class AddSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "AddSkillEffect";
	}

	public Dictionary<string, List<SkillEffectDefinition>> SkillEffectDefinitions;

	public AddSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		SkillEffectDefinitions = new Dictionary<string, List<SkillEffectDefinition>>();
		foreach (XElement item in obj.Element("SkillEffects").Elements())
		{
			AddEffect(SkillActionDefinition.DeserializeSkillEffect(item, base.TokenVariables));
		}
	}

	private void AddEffect(SkillEffectDefinition skillEffectDefinition)
	{
		if (!SkillEffectDefinitions.TryGetValue(skillEffectDefinition.Id, out var value))
		{
			value = new List<SkillEffectDefinition>();
			SkillEffectDefinitions.Add(skillEffectDefinition.Id, value);
		}
		value.Add(skillEffectDefinition);
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
}
