using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ResupplyOverallUsesSkillEffectDefinition : ResupplyBuildingsSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ResupplyOverallUses";
	}

	public override string Id => "ResupplyOverallUses";

	public ResupplyOverallUsesSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
