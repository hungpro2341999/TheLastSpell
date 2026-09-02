using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlayAnimatedCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "PlayAnimatedCutscene";
	}

	public string CutsceneId { get; private set; }

	public PlayAnimatedCutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		CutsceneId = xElement.Attribute("CutsceneId").Value;
	}
}
