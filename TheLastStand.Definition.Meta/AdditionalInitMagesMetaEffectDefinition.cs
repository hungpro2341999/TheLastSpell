using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class AdditionalInitMagesMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "AdditionalInitMages";

	public int Amount { get; private set; }

	public AdditionalInitMagesMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (int.TryParse(xElement.Value, out var result))
		{
			Amount = result;
		}
		else
		{
			Debug.LogError("Could not parse value " + xElement.Value + " to an integer!");
		}
	}
}
