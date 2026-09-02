using System.Xml.Linq;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideAppendNextTextDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "AppendNextText";
	}

	public bool DontWaitForSkipInput { get; private set; }

	public bool NoNewLine { get; private set; }

	public AnimatedCutsceneSlideAppendNextTextDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		DontWaitForSkipInput = xElement.Element("DontWaitForSkipInput") != null;
		NoNewLine = xElement.Element("NoNewLine") != null;
	}
}
