using TheLastStand.Definition;
using TheLastStand.Manager;
using TheLastStand.Serialization.Sink;
using UnityEngine;

namespace TheLastStand.Model.Sink;

public class SinkData
{
	public SinkDataDefinition SinkDataDefinition { get; }

	public int Rerolls { get; set; }

	public int BasePrice => SinkDataDefinition.BasePrice;

	public float RerollMultiplier => SinkDataDefinition.RerollMultiplier;

	public int RoundedTo => SinkDataDefinition.RoundedTo;

	public SinkData(SinkDataDefinition sinkDataDefinition)
	{
		SinkDataDefinition = sinkDataDefinition;
	}

	public int GetFinalPrice()
	{
		if (SinkManager.DebugSinkFree)
		{
			return 0;
		}
		return Mathf.RoundToInt((float)SinkDataDefinition.FinalPrice.EvalToInt(this) / (float)RoundedTo) * RoundedTo;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedSinkData serializedSinkData)
		{
			Rerolls = serializedSinkData.Rerolls;
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedSinkData
		{
			Rerolls = Rerolls
		};
	}
}
