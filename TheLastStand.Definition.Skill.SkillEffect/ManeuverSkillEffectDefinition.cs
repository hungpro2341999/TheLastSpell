using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ManeuverSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Maneuver";
	}

	public override string Id => "Maneuver";

	public ManeuverSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
