using System.Xml.Linq;
using TPLib.Log;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ExileCasterEffectDefinition : AffectingUnitSkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ExileCaster";
	}

	public override string Id => "ExileCaster";

	public override bool DisplayCompendiumEntry => false;

	public bool ForcePlayDieAnim { get; private set; }

	public override bool ShouldBeDisplayed => false;

	public ExileCasterEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		AffectedUnits = E_SkillUnitAffect.Caster;
		XAttribute xAttribute = (container as XElement).Attribute("ForcePlayDieAnim");
		if (xAttribute != null)
		{
			if (bool.TryParse(xAttribute.Value, out var result))
			{
				ForcePlayDieAnim = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse ExileCaster effect ForcePlayDieAnim value " + xAttribute.Value + " to a valid bool.");
			}
		}
	}
}
