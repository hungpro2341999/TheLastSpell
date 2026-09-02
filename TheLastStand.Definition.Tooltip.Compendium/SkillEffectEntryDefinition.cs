using System.Xml.Linq;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class SkillEffectEntryDefinition : ACompendiumEntryDefinition
{
	public static class Constants
	{
		public const string Name = "SkillEffectEntry";
	}

	public string SkillEffectId { get; private set; }

	public SkillEffectEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		SkillEffectId = (container as XElement).Element("SkillEffectId")?.Value ?? base.Id;
	}
}
