using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ExtinguishBrazierSkillEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ExtinguishBrazier";
	}

	public override string Id => "ExtinguishBrazier";

	public int BrazierDamage { get; private set; }

	public ExtinguishBrazierSkillEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		BrazierDamage = int.Parse(xElement.Element("BrazierDamage").Value);
	}
}
