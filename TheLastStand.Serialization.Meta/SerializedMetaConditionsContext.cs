using System;
using TheLastStand.Model.Meta;

namespace TheLastStand.Serialization.Meta;

[Serializable]
public class SerializedMetaConditionsContext : ISerializedData
{
	public MetaConditionSpecificContext Context;
}
