using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public List<AnimatedCutsceneSlideDefinition> SlidesDefinitions { get; private set; }

	public AnimatedCutsceneDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		SlidesDefinitions = new List<AnimatedCutsceneSlideDefinition>();
		foreach (XElement item in obj.Elements("Slide"))
		{
			SlidesDefinitions.Add(new AnimatedCutsceneSlideDefinition(item));
		}
	}
}
