using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class NoDodgeEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "NoDodge";
	}

	public override string Id => "NoDodge";

	public NoDodgeEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
