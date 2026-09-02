using System.Xml.Linq;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideNextAnimationDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "NextAnimation";
	}

	public string CustomParameter { get; private set; }

	public bool WaitForClipDuration { get; private set; }

	public AnimatedCutsceneSlideNextAnimationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		WaitForClipDuration = xElement.Element("WaitForClipDuration") != null;
		CustomParameter = xElement.Attribute("CustomParameter")?.Value ?? null;
	}
}
