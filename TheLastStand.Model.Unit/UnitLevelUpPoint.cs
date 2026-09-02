using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Unit;

public class UnitLevelUpPoint : ISerializable, IDeserializable
{
	public bool HasAnyStatPoint
	{
		get
		{
			if (!HasMainStatPoint)
			{
				return HasSecondaryStatPoint;
			}
			return true;
		}
	}

	public bool HasMainStatPoint { get; set; } = true;

	public bool HasSecondaryStatPoint { get; set; } = true;

	public UnitLevelUpPoint()
	{
	}

	public UnitLevelUpPoint(SerializedLevelUpPoint levelUpPoint)
	{
		Deserialize(levelUpPoint);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedLevelUpPoint serializedLevelUpPoint = container as SerializedLevelUpPoint;
		HasMainStatPoint = serializedLevelUpPoint.HasMainStatPoint;
		HasSecondaryStatPoint = serializedLevelUpPoint.HasSecondaryStatPoint;
	}

	public ISerializedData Serialize()
	{
		return new SerializedLevelUpPoint
		{
			HasMainStatPoint = HasMainStatPoint,
			HasSecondaryStatPoint = HasSecondaryStatPoint
		};
	}
}
