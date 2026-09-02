using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class NoBlockEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "NoBlock";
	}

	public override string Id => "NoBlock";

	public NoBlockEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
	}
}
