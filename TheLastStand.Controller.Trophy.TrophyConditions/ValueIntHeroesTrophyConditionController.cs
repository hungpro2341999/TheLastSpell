using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Serialization.Trophy;
using UnityEngine;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public abstract class ValueIntHeroesTrophyConditionController : HeroesTrophyConditionController
{
	protected ValueIntHeroesTrophyConditionDefinition ValueIntHeroesTrophyConditionDefinition => base.TrophyConditionDefinition as ValueIntHeroesTrophyConditionDefinition;

	public Dictionary<int, int> ProgressionPerUnitId { get; } = new Dictionary<int, int>();

	protected ValueIntHeroesTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	protected override void CheckCompleteState()
	{
		if (isCompleted)
		{
			return;
		}
		switch (ValueIntHeroesTrophyConditionDefinition.Target)
		{
		case "All":
		{
			float num = 0f;
			foreach (KeyValuePair<int, int> item in ProgressionPerUnitId)
			{
				num += (float)item.Value;
			}
			isCompleted = num >= (float)ValueIntHeroesTrophyConditionDefinition.Value;
			break;
		}
		case "One":
		{
			foreach (KeyValuePair<int, int> item2 in ProgressionPerUnitId)
			{
				if (item2.Value >= ValueIntHeroesTrophyConditionDefinition.Value)
				{
					base.FirstUnitToUnlockTrophy = item2.Key;
					isCompleted = true;
				}
			}
			break;
		}
		}
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " needs 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is int num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as second argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (ProgressionPerUnitId.ContainsKey(key))
		{
			ProgressionPerUnitId[key] += num;
		}
		else
		{
			ProgressionPerUnitId.Add(key, num);
		}
		OnValueChange();
	}

	public override void Clear()
	{
		ProgressionPerUnitId.Clear();
		isCompleted = false;
	}

	public override object GetTotal()
	{
		int num = 0;
		foreach (KeyValuePair<int, int> item in ProgressionPerUnitId)
		{
			num += item.Value;
		}
		return num;
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " needs 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is int value))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as second argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (ProgressionPerUnitId.ContainsKey(key))
		{
			ProgressionPerUnitId[key] = value;
		}
		else
		{
			ProgressionPerUnitId.Add(key, value);
		}
		OnValueChange();
	}

	public override void SetMaxValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " needs 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is int num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type " + Name + " should have as second argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (ProgressionPerUnitId.ContainsKey(key))
		{
			ProgressionPerUnitId[key] = Mathf.Max(ProgressionPerUnitId[key], num);
		}
		else
		{
			ProgressionPerUnitId.Add(key, num);
		}
		OnValueChange();
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (ValueIntHeroesTrophyConditionDefinition.Target == "All")
		{
			text += $"    Total : {GetTotal()}/{ValueIntHeroesTrophyConditionDefinition.Value} \r\n";
		}
		foreach (KeyValuePair<int, int> item in ProgressionPerUnitId)
		{
			text += string.Format("    {0} : {1}/{2} {3}\r\n", GetUnitName(item.Key), item.Value, ValueIntHeroesTrophyConditionDefinition.Value, (base.FirstUnitToUnlockTrophy == item.Key) ? "#First" : string.Empty);
		}
		return text;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		foreach (SerializedValueIntHeroesTrophy.IntPair item in (container as SerializedValueIntHeroesTrophy).ValuePerUnitId)
		{
			ProgressionPerUnitId[item.UnitId] = item.Value;
		}
	}

	public override ISerializedData Serialize()
	{
		return new SerializedValueIntHeroesTrophy
		{
			Name = Name,
			ValuePerUnitId = ProgressionPerUnitId.Select((KeyValuePair<int, int> p) => new SerializedValueIntHeroesTrophy.IntPair
			{
				UnitId = p.Key,
				Value = p.Value
			}).ToList()
		};
	}
}
