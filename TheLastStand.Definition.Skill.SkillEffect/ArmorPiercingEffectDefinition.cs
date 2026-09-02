using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ArmorPiercingEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ArmorPiercing";
	}

	public override string Id => "ArmorPiercing";

	public ArmorPiercingEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
