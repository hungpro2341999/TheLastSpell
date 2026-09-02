using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class IgnoreLineOfSightEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "IgnoreLineOfSight";
	}

	public override string Id => "IgnoreLineOfSight";

	public IgnoreLineOfSightEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
