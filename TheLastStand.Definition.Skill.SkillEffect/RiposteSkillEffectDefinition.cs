using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class RiposteSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Riposte";
	}

	public override string Id => "Riposte";

	public RiposteSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
