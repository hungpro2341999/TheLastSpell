using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaNarrationConditionsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<MetaReplicaConditionDefinition> Conditions { get; private set; }

	public MetaNarrationConditionsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		Conditions = new List<MetaReplicaConditionDefinition>();
		foreach (XElement item in obj.Elements())
		{
			switch (item.Name.LocalName)
			{
			case "DaysPlayed":
				Conditions.Add(new MetaReplicaDaysPlayedConditionDefinition(item));
				break;
			case "UsedReplica":
				Conditions.Add(new MetaReplicaUsedReplicaConditionDefinition(item));
				break;
			case "MetaUpgradeUnlocked":
				Conditions.Add(new MetaReplicaMetaUpgradeUnlockedConditionDefinition(item));
				break;
			default:
				Debug.LogError("Invalid condition name " + item.Name.LocalName + ".");
				return;
			}
		}
	}
}
