using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Cutscene;

public class PlayCommanderFadeOutAnimCutsceneDefinition : TheLastStand.Framework.Serialization.Definition, ICutsceneDefinition
{
	public class Constants
	{
		public const string Id = "CommanderPlayFadeOutAnim";
	}

	public PlayCommanderFadeOutAnimCutsceneDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
