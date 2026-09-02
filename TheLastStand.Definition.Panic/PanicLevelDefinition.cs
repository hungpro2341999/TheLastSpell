using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Panic;

public class PanicLevelDefinition : TheLastStand.Framework.Serialization.Definition
{
	public PanicRewardDefinition PanicRewardDefinition { get; private set; }

	public float PanicValueNeeded { get; private set; }

	public PanicLevelDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Element("PanicValueNeeded").Attribute("Value");
		if (!int.TryParse(xAttribute.Value, out var result))
		{
			Debug.LogError("Invalid PanicValueNeeded " + xAttribute.Value);
		}
		PanicValueNeeded = result;
		XElement container2 = obj.Element("Reward");
		PanicRewardDefinition = new PanicRewardDefinition(container2);
	}
}
