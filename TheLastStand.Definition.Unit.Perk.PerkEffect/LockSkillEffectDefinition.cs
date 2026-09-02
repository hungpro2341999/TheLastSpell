using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class LockSkillEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "LockSkill";
	}

	public string SkillId { get; private set; }

	public LockSkillEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("SkillId");
		SkillId = xAttribute.Value;
	}
}
