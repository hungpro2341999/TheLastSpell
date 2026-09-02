using System.Xml.Linq;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class IgnoreLineOfSightInCityTilesEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "IgnoreLineOfSightInCityTiles";
	}

	public override bool DisplayCompendiumEntry => false;

	public override string Id => "IgnoreLineOfSightInCityTiles";

	public override bool ShouldBeDisplayed => false;

	public IgnoreLineOfSightInCityTilesEffectDefinition(XContainer container)
		: base(container)
	{
	}
}
