using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class NoMomentumEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "NoMomentum";
	}

	public override string Id => "NoMomentum";

	public NoMomentumEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
