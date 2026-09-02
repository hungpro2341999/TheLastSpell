using TPLib;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Serialization.Achievements;

namespace TheLastStand.DRM.Achievements;

public class AchievementUnlocker : ISerializable, IDeserializable
{
	private readonly Achievement achievementToUnlock;

	private int valueLimit;

	private int value;

	public AchievementUnlocker(Achievement achievementToUnlock, int valueLimit, int defaultValue = 0)
	{
		this.achievementToUnlock = achievementToUnlock;
		this.valueLimit = valueLimit;
		value = defaultValue;
	}

	public void IncreaseValue(int addedValue = 1)
	{
		value += addedValue;
		if (value >= valueLimit)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(achievementToUnlock);
		}
	}

	public void Reset()
	{
		value = 0;
	}

	public void SetValue(int newValue)
	{
		Reset();
		IncreaseValue(newValue);
	}

	public void SetValueLimit(int newValueLimit)
	{
		valueLimit = newValueLimit;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedAchievementUnlocker serializedAchievementUnlocker)
		{
			SetValue(serializedAchievementUnlocker.Value);
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedAchievementUnlocker
		{
			Value = value
		};
	}
}
