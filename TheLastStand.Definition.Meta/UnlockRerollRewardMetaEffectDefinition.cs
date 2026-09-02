using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class UnlockRerollRewardMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UnlockRerollReward";

	public int RerollReward { get; private set; }

	public UnlockRerollRewardMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (int.TryParse(xElement.Value, out var result))
		{
			RerollReward = result;
		}
		else
		{
			Debug.LogError("Could not parse value " + xElement.Value + " to an integer!");
		}
	}
}
