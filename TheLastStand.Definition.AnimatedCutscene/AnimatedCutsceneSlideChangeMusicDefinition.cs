using System.Xml.Linq;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideChangeMusicDefinition : AnimatedCutsceneSlideItemDefinition
{
	public class Constants
	{
		public const string Id = "ChangeMusic";
	}

	public string ClipAssetName { get; private set; }

	public AnimatedCutsceneSlideChangeMusicDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("ClipAssetName");
		ClipAssetName = xAttribute.Value;
	}
}
