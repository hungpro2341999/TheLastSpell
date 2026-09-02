using System;

namespace TheLastStand.Model.Status;

[Serializable]
public class StatusSourceInfo : ISerializedData
{
	public int SourceRandomId;

	public DamageableType SourceType;

	public StatusSourceInfo(int sourceRandomId, DamageableType sourceType)
	{
		SourceRandomId = sourceRandomId;
		SourceType = sourceType;
	}

	public StatusSourceInfo()
	{
	}
}
