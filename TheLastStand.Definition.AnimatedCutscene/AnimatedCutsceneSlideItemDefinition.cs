using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.AnimatedCutscene;

public abstract class AnimatedCutsceneSlideItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	public AnimatedCutsceneSlideItemDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
