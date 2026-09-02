using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;

public class DamageSurroundingEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Damage";
	}

	public float Multiplier { get; private set; }

	public override string Id => "Damage";

	public override bool DisplayCompendiumEntry => false;

	public override bool ShouldBeDisplayed => true;

	public DamageSurroundingEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		Multiplier = float.Parse(xElement.Attribute("Multiplier").Value, NumberStyles.Float, CultureInfo.InvariantCulture);
	}
}
