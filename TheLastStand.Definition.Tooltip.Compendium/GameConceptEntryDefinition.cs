using System.Xml.Linq;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class GameConceptEntryDefinition : ACompendiumEntryDefinition
{
	public static class Constants
	{
		public const string Name = "GameConceptEntry";
	}

	public string GameConceptId { get; private set; }

	public GameConceptEntryDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("GameConceptId");
		GameConceptId = xElement.Value;
	}
}
