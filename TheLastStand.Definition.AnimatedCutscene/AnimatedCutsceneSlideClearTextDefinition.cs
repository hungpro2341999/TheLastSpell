using System.Xml.Linq;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideClearTextDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "ClearText";
	}

	public AnimatedCutsceneSlideClearTextDefinition(XContainer container)
		: base(container)
	{
	}
}
