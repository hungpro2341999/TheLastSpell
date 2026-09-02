using System.Globalization;
using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ArmorShreddingEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ArmorShredding";
	}

	public override string Id => "ArmorShredding";

	public float BonusDamage { get; set; } = 1f;

	public ArmorShreddingEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute;
		if ((xAttribute = (container as XElement).Attribute("BonusDamage")) != null)
		{
			BonusDamage = float.Parse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		}
	}
}
