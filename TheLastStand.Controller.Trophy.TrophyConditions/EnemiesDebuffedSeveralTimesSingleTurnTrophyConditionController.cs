using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Serialization.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class EnemiesDebuffedSeveralTimesSingleTurnTrophyConditionController : HeroesTrophyConditionController
{
	public override string Name => "EnemiesDebuffedSeveralTimesSingleTurn";

	public Dictionary<int, Dictionary<int, Dictionary<int, int>>> DebuffsByEnemyIdByUserIdByTurn { get; private set; } = new Dictionary<int, Dictionary<int, Dictionary<int, int>>>();

	public EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition EnemiesDebuffedSeveralTimesSingleTurnDefinition => base.TrophyConditionDefinition as EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition;

	public EnemiesDebuffedSeveralTimesSingleTurnTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 3)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn AppendValue needs 3 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is int key2))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn should have as second argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[2] is int key3))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn should have as third argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!DebuffsByEnemyIdByUserIdByTurn.ContainsKey(key))
		{
			DebuffsByEnemyIdByUserIdByTurn.Add(key, new Dictionary<int, Dictionary<int, int>>());
		}
		if (!DebuffsByEnemyIdByUserIdByTurn[key].ContainsKey(key2))
		{
			DebuffsByEnemyIdByUserIdByTurn[key].Add(key2, new Dictionary<int, int>());
		}
		if (!DebuffsByEnemyIdByUserIdByTurn[key][key2].ContainsKey(key3))
		{
			DebuffsByEnemyIdByUserIdByTurn[key][key2].Add(key3, 1);
		}
		else
		{
			DebuffsByEnemyIdByUserIdByTurn[key][key2][key3]++;
		}
		OnValueChange();
	}

	public override void Clear()
	{
		DebuffsByEnemyIdByUserIdByTurn.Clear();
		isCompleted = false;
	}

	public override object GetTotal()
	{
		int num = 0;
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, int>>> item in DebuffsByEnemyIdByUserIdByTurn)
		{
			foreach (KeyValuePair<int, Dictionary<int, int>> item2 in item.Value)
			{
				num += item2.Value.Where((KeyValuePair<int, int> x) => x.Value >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.NumberOfDebuffs).Count();
			}
		}
		return num;
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 4)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn SetValue needs 4 arguments");
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is int key2))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn should have as second argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[2] is int key3))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn should have as third argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[3] is int value))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type EnemiesDebuffedSeveralTimesSingleTurn should have as fourth argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!DebuffsByEnemyIdByUserIdByTurn.ContainsKey(key))
		{
			DebuffsByEnemyIdByUserIdByTurn.Add(key, new Dictionary<int, Dictionary<int, int>>());
		}
		if (!DebuffsByEnemyIdByUserIdByTurn[key].ContainsKey(key2))
		{
			DebuffsByEnemyIdByUserIdByTurn[key].Add(key2, new Dictionary<int, int>());
		}
		if (!DebuffsByEnemyIdByUserIdByTurn[key][key2].ContainsKey(key3))
		{
			DebuffsByEnemyIdByUserIdByTurn[key][key2].Add(key3, 1);
		}
		else
		{
			DebuffsByEnemyIdByUserIdByTurn[key][key2][key3] = value;
		}
		OnValueChange();
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (EnemiesDebuffedSeveralTimesSingleTurnDefinition.Target == "All")
		{
			text += $"    Total : {GetTotal()}/{EnemiesDebuffedSeveralTimesSingleTurnDefinition.Value} \r\n";
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, int>>> item in DebuffsByEnemyIdByUserIdByTurn)
		{
			foreach (KeyValuePair<int, Dictionary<int, int>> item2 in item.Value)
			{
				if (!dictionary.ContainsKey(item2.Key))
				{
					dictionary.Add(item2.Key, item2.Value.Where((KeyValuePair<int, int> x) => x.Value >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.NumberOfDebuffs).Count());
				}
				else
				{
					dictionary[item2.Key] += item2.Value.Where((KeyValuePair<int, int> x) => x.Value >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.NumberOfDebuffs).Count();
				}
			}
		}
		foreach (KeyValuePair<int, int> item3 in dictionary)
		{
			text += string.Format("    {0} : {1}/{2} {3}\r\n", GetUnitName(item3.Key), item3.Value, EnemiesDebuffedSeveralTimesSingleTurnDefinition.Value, (base.FirstUnitToUnlockTrophy == item3.Key) ? "#First" : string.Empty);
		}
		return text;
	}

	protected override void CheckCompleteState()
	{
		if (isCompleted)
		{
			return;
		}
		if (EnemiesDebuffedSeveralTimesSingleTurnDefinition.Target == "All")
		{
			int num = 0;
			foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, int>>> item in DebuffsByEnemyIdByUserIdByTurn)
			{
				foreach (KeyValuePair<int, Dictionary<int, int>> item2 in item.Value)
				{
					num += item2.Value.Where((KeyValuePair<int, int> x) => x.Value >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.NumberOfDebuffs).Count();
				}
			}
			isCompleted = num >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.Value;
			return;
		}
		if (EnemiesDebuffedSeveralTimesSingleTurnDefinition.Target == "One")
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, int>>> item3 in DebuffsByEnemyIdByUserIdByTurn)
			{
				foreach (KeyValuePair<int, Dictionary<int, int>> item4 in item3.Value)
				{
					if (!dictionary.ContainsKey(item4.Key))
					{
						dictionary.Add(item4.Key, item4.Value.Where((KeyValuePair<int, int> x) => x.Value >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.NumberOfDebuffs).Count());
					}
					else
					{
						dictionary[item4.Key] += item4.Value.Where((KeyValuePair<int, int> x) => x.Value >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.NumberOfDebuffs).Count();
					}
					if (dictionary[item4.Key] >= EnemiesDebuffedSeveralTimesSingleTurnDefinition.Value)
					{
						base.FirstUnitToUnlockTrophy = item4.Key;
						isCompleted = true;
						return;
					}
				}
			}
		}
		isCompleted = false;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		foreach (SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.EnemiesDebuffedDictionary enemiesDebuffedDictionary in (container as SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy).EnemiesDebuffedDictionaries)
		{
			DebuffsByEnemyIdByUserIdByTurn.Add(enemiesDebuffedDictionary.Turn, new Dictionary<int, Dictionary<int, int>>());
			foreach (SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.UnitDebuffDictionary value in enemiesDebuffedDictionary.Values)
			{
				DebuffsByEnemyIdByUserIdByTurn[enemiesDebuffedDictionary.Turn][value.UnitId] = new Dictionary<int, int>();
				foreach (SerializedValueIntHeroesTrophy.IntPair value2 in value.Values)
				{
					DebuffsByEnemyIdByUserIdByTurn[enemiesDebuffedDictionary.Turn][value.UnitId][value2.UnitId] = value2.Value;
				}
			}
		}
	}

	public override ISerializedData Serialize()
	{
		List<SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.EnemiesDebuffedDictionary> list = new List<SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.EnemiesDebuffedDictionary>();
		foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, int>>> item3 in DebuffsByEnemyIdByUserIdByTurn)
		{
			List<SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.UnitDebuffDictionary> list2 = new List<SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.UnitDebuffDictionary>();
			foreach (KeyValuePair<int, Dictionary<int, int>> item4 in item3.Value)
			{
				List<SerializedValueIntHeroesTrophy.IntPair> list3 = new List<SerializedValueIntHeroesTrophy.IntPair>();
				foreach (KeyValuePair<int, int> item5 in item4.Value)
				{
					list3.Add(new SerializedValueIntHeroesTrophy.IntPair
					{
						UnitId = item5.Key,
						Value = item5.Value
					});
				}
				SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.UnitDebuffDictionary item = new SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.UnitDebuffDictionary
				{
					UnitId = item4.Key,
					Values = list3
				};
				list2.Add(item);
			}
			SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.EnemiesDebuffedDictionary item2 = new SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy.EnemiesDebuffedDictionary
			{
				Turn = item3.Key,
				Values = list2
			};
			list.Add(item2);
		}
		return new SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy
		{
			EnemiesDebuffedDictionaries = list
		};
	}
}
