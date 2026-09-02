using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class FollowSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Follow";
	}

	public override string Id => "Follow";

	public FollowSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
