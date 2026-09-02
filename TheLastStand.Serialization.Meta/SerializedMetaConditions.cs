using System;
using System.Collections.Generic;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedMetaConditions : ISerializedData
{
	public SerializedMetaConditionsContext CampaignContext;

	public List<SerializedMetaCondition> Conditions = new List<SerializedMetaCondition>();
}
