using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaReplicaDaysPlayedConditionDefinition : MetaReplicaConditionDefinition
{
	public class Constants
	{
		public const string Name = "DaysPlayed";
	}

	public int DaysCount { get; private set; }

	public MetaReplicaDaysPlayedConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		if (!int.TryParse(xElement.Value, out var result))
		{
			Debug.LogError("Could not parse " + xElement.Value + " to a valid int value.");
		}
		else
		{
			DaysCount = result;
		}
	}
}
