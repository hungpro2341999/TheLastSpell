using System.Xml.Linq;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideReplaceCommanderViewDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "ReplaceCommanderView";
	}

	public AnimatedCutsceneSlideReplaceCommanderViewDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
