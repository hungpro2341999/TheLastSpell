using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class KillSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Kill";
	}

	public override string Id => "Kill";

	public KillSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
