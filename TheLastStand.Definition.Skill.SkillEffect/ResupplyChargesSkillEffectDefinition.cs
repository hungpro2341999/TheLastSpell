using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ResupplyChargesSkillEffectDefinition : ResupplyBuildingsSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ResupplyCharges";
	}

	public override string Id => "ResupplyCharges";

	public ResupplyChargesSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
