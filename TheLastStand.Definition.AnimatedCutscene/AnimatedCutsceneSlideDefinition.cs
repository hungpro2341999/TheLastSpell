using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.AnimatedCutscene;

public class AnimatedCutsceneSlideDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public List<AnimatedCutsceneSlideItemDefinition> SlideElementsDefinitions { get; private set; }

	public AnimatedCutsceneSlideDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		SlideElementsDefinitions = new List<AnimatedCutsceneSlideItemDefinition>();
		foreach (XElement item in obj.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "Delay":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideDelayDefinition(item));
				continue;
			case "NextAnimation":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideNextAnimationDefinition(item));
				continue;
			case "AppendNextText":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideAppendNextTextDefinition(item));
				continue;
			case "ClearText":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideClearTextDefinition(item));
				continue;
			case "PlaySound":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlidePlaySoundDefinition(item));
				continue;
			case "ChangeMusic":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideChangeMusicDefinition(item));
				continue;
			case "ReplaceCommanderView":
				SlideElementsDefinitions.Add(new AnimatedCutsceneSlideReplaceCommanderViewDefinition(item));
				continue;
			}
			CLoggerManager.Log("Unknown slide item Id " + item.Name.LocalName + " in Slide " + Id + ".", LogType.Error);
			return;
		}
	}
}
