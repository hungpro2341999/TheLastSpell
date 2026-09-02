using TPLib;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Serialization.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit.Stat;

public class EnemyUnitStat : UnitStat
{
	public float ApocalypseBaseValueStatModifier { get; set; }

	public float Apocalypse { get; set; }

	public float Progression { get; set; }

	public StatModifierDefinition AffixStatModifier { get; set; }

	public override float Base
	{
		get
		{
			float num = base.Base;
			num += Progression;
			float num2 = (ApocalypseManager.IsThisStatIncreasedByPercentage(base.StatId) ? Mathf.Ceil(num * (ApocalypseBaseValueStatModifier * 0.01f)) : ApocalypseBaseValueStatModifier);
			num += num2;
			if (AffixStatModifier == null)
			{
				return num;
			}
			return GetValueWithStatModifier(num, AffixStatModifier.PercentageModifier, AffixStatModifier.FlatModifier);
		}
		set
		{
			base.Base = value;
		}
	}

	public override float Final
	{
		get
		{
			if (ShouldNullifyStat())
			{
				return 0f;
			}
			return base.Final + Apocalypse;
		}
	}

	public EnemyUnitStat(UnitStatsController statsController, UnitStatDefinition.E_Stat id, Vector2 boundaries)
		: base(statsController, id, boundaries)
	{
	}

	public static float ComputeStatProgression(int initialDay, int increaseEveryXDay, bool increaseOnFirstDay, float bonusEveryXDay, int delay = -1, float baseBonus = 0f, int maxIncreases = int.MaxValue)
	{
		int num = TPSingleton<GameManager>.Instance.Game.DayNumber - initialDay - (delay + 1);
		if (num < 0)
		{
			return baseBonus;
		}
		if (num == 0)
		{
			return baseBonus + (increaseOnFirstDay ? bonusEveryXDay : 0f);
		}
		return (float)Mathf.Clamp(Mathf.CeilToInt(((float)num + 1f) / (float)increaseEveryXDay) - ((!increaseOnFirstDay) ? 1 : 0), 0, maxIncreases) * bonusEveryXDay + baseBonus;
	}

	public static float GetValueWithStatModifier(float baseValue, float percentageModifier, float flatModifier)
	{
		baseValue *= percentageModifier / 100f;
		baseValue += flatModifier;
		return baseValue;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedEnemyUnitStat serializedEnemyUnitStat = container as SerializedEnemyUnitStat;
		base.Deserialize(serializedEnemyUnitStat.Stat, saveVersion);
	}

	public override ISerializedData Serialize()
	{
		return new SerializedEnemyUnitStat
		{
			Stat = (base.Serialize() as SerializedUnitStat)
		};
	}
}
