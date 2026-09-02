using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Serialization.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesDamagedTrophyConditionController : HeroesTrophyConditionController
{
	public override string Name => "EnemiesDamaged";

	public Dictionary<int, List<int>> EnemiesDamagedByUserId { get; private set; } = new Dictionary<int, List<int>>();

	public EnemiesDamagedTrophyDefinition EnemiesDamagedDefinition => base.TrophyConditionDefinition as EnemiesDamagedTrophyDefinition;

	public EnemiesDamagedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamaged need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamaged should have as first argument a int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is int item))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamaged should have as second argument a int !", CLogLevel.DETAILED);
			return;
		}
		if (!EnemiesDamagedByUserId.ContainsKey(key))
		{
			EnemiesDamagedByUserId.Add(key, new List<int> { item });
		}
		else if (!EnemiesDamagedByUserId[key].Contains(item))
		{
			EnemiesDamagedByUserId[key].Add(item);
		}
		OnValueChange();
	}

	public override void Clear()
	{
		EnemiesDamagedByUserId.Clear();
		isCompleted = false;
	}

	public override object GetTotal()
	{
		int num = 0;
		foreach (KeyValuePair<int, List<int>> item in EnemiesDamagedByUserId)
		{
			num += item.Value.Count;
		}
		return num;
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamaged need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamaged should have as first argument a int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is int item))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDamaged should have as second argument a int !", CLogLevel.DETAILED);
			return;
		}
		if (!EnemiesDamagedByUserId.ContainsKey(key))
		{
			EnemiesDamagedByUserId.Add(key, new List<int> { item });
		}
		else
		{
			EnemiesDamagedByUserId[key].Clear();
			EnemiesDamagedByUserId[key].Add(item);
		}
		OnValueChange();
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (EnemiesDamagedDefinition.Target == "All")
		{
			text += $"    Total : {GetTotal()}/{EnemiesDamagedDefinition.Value} \r\n";
		}
		foreach (KeyValuePair<int, List<int>> item in EnemiesDamagedByUserId)
		{
			text += string.Format("    {0} : {1}/{2} {3}\r\n", item.Key, item.Value.Count, EnemiesDamagedDefinition.Value, (base.FirstUnitToUnlockTrophy == item.Key) ? "#First" : string.Empty);
		}
		return text;
	}

	protected override void CheckCompleteState()
	{
		if (isCompleted)
		{
			return;
		}
		if (EnemiesDamagedDefinition.Target == "All")
		{
			int num = 0;
			foreach (KeyValuePair<int, List<int>> item in EnemiesDamagedByUserId)
			{
				num += item.Value.Count;
			}
			isCompleted = num >= EnemiesDamagedDefinition.Value;
			return;
		}
		if (EnemiesDamagedDefinition.Target == "One")
		{
			foreach (KeyValuePair<int, List<int>> item2 in EnemiesDamagedByUserId)
			{
				if (item2.Value.Count >= EnemiesDamagedDefinition.Value)
				{
					base.FirstUnitToUnlockTrophy = item2.Key;
					isCompleted = true;
					return;
				}
			}
		}
		isCompleted = false;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		foreach (SerializedEnemiesDamagedTrophy.IntListPair item in (container as SerializedEnemiesDamagedTrophy).ValuesPerUnitId)
		{
			EnemiesDamagedByUserId[item.UnitId] = item.Values;
		}
	}

	public override ISerializedData Serialize()
	{
		List<SerializedEnemiesDamagedTrophy.IntListPair> list = new List<SerializedEnemiesDamagedTrophy.IntListPair>();
		foreach (KeyValuePair<int, List<int>> item in EnemiesDamagedByUserId)
		{
			list.Add(new SerializedEnemiesDamagedTrophy.IntListPair
			{
				UnitId = item.Key,
				Values = item.Value
			});
		}
		return new SerializedEnemiesDamagedTrophy
		{
			ValuesPerUnitId = list
		};
	}
}
