using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Controller.Trophy;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Model.Trophy;
using TheLastStand.Serialization.Trophy;
using UnityEngine;

namespace TheLastStand.Manager;

public class TrophyManager : Manager<TrophyManager>, ISerializable, IDeserializable
{
	public uint ComputedDamnedSoulsEarnedThisNight { get; private set; }

	public uint DamnedSoulsEarnedThisNightWithMultiplier { get; private set; }

	public int EnemiesKilledThisTurn { get; set; }

	public List<Trophy> Trophies { get; } = new List<Trophy>();

	public int RawDamnedSoulsEarnedThisNight { get; set; }

	public int SpawnWaveDuration { get; set; }

	public static void AddDamnedSouls(int damnedSoulsToAdd)
	{
		TPSingleton<TrophyManager>.Instance.RawDamnedSoulsEarnedThisNight += damnedSoulsToAdd;
	}

	public static void AddEnemyKill(int damnedSoulsEarned)
	{
		TPSingleton<TrophyManager>.Instance.EnemiesKilledThisTurn++;
		AddDamnedSouls(damnedSoulsEarned);
	}

	public static void AppendValueToTrophiesConditions<T>(params object[] args) where T : TrophyConditionController
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
		{
			return;
		}
		foreach (TrophyConditionController item in from x in TPSingleton<TrophyManager>.Instance.Trophies
			where x.TrophyConditionController.GetType() == typeof(T)
			select x.TrophyConditionController)
		{
			if (!item.ShouldCheckCompletionBeforeUpdate || !item.IsCompleted)
			{
				item.AppendValue(args);
			}
		}
	}

	public static void SetValueToTrophiesConditions<T>(params object[] args) where T : TrophyConditionController
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
		{
			return;
		}
		foreach (TrophyConditionController item in from x in TPSingleton<TrophyManager>.Instance.Trophies
			where x.TrophyConditionController.GetType() == typeof(T)
			select x.TrophyConditionController)
		{
			item.SetValue(args);
		}
	}

	public static void SetMaxValueToTrophiesConditions<T>(params object[] args) where T : TrophyConditionController
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
		{
			return;
		}
		foreach (TrophyConditionController item in from x in TPSingleton<TrophyManager>.Instance.Trophies
			where x.TrophyConditionController.GetType() == typeof(T)
			select x.TrophyConditionController)
		{
			item.SetMaxValue(args);
		}
	}

	public uint AutoGainDamnedSouls(bool isDefeat)
	{
		ComputedDamnedSoulsEarnedThisNight = ComputeDamnedSoulsEarnedWithoutTrophies();
		ComputedDamnedSoulsEarnedThisNight += ComputeEarnedDamnedSoulsByTrophies(isDefeat);
		ApplicationManager.Application.DamnedSouls += ComputedDamnedSoulsEarnedThisNight;
		return ComputedDamnedSoulsEarnedThisNight;
	}

	public uint ComputeDamnedSoulsEarnedWithoutTrophies()
	{
		float num = 1f;
		if (TrophyDatabase.DefaultTrophyDefinition.MultiplierPerDay.ContainsKey(TPSingleton<GameManager>.Instance.Game.DayNumber))
		{
			num = TrophyDatabase.DefaultTrophyDefinition.MultiplierPerDay[TPSingleton<GameManager>.Instance.Game.DayNumber].EvalToFloat(new MultiplierEvalContext());
		}
		DamnedSoulsEarnedThisNightWithMultiplier = (uint)Mathf.RoundToInt((float)RawDamnedSoulsEarnedThisNight * num);
		uint num2 = TPSingleton<GlyphManager>.Instance.DamnedSoulsPercentageModifier + TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier;
		float num3 = 1f + (float)num2 / 100f;
		DamnedSoulsEarnedThisNightWithMultiplier = (uint)((float)DamnedSoulsEarnedThisNightWithMultiplier * num3);
		return DamnedSoulsEarnedThisNightWithMultiplier;
	}

	public uint ComputeEarnedDamnedSoulsByTrophies(bool isDefeat)
	{
		List<Trophy> successfulTrophies = GetSuccessfulTrophies(isDefeat);
		uint num = 0u;
		for (int i = 0; i < successfulTrophies.Count; i++)
		{
			num += successfulTrophies[i].TrophyDefinition.DamnedSoulsEarned;
		}
		return num;
	}

	public List<Trophy> GetSuccessfulTrophies(bool isDefeat, bool shouldUpdateCompletedState = true)
	{
		List<Trophy> list = new List<Trophy>();
		for (int i = 0; i < Trophies.Count; i++)
		{
			if ((shouldUpdateCompletedState ? Trophies[i].TrophyController.IsCompleted(isDefeat) : Trophies[i].TrophyController.IsCompletedWithoutUpdate(isDefeat)) && !Trophies[i].TrophyController.IsOverrided(isDefeat))
			{
				list.Add(Trophies[i]);
			}
		}
		return list;
	}

	public string GetProgressionLog(bool isDefeat)
	{
		string text = "\r\n";
		string empty = string.Empty;
		List<Trophy> source = TPSingleton<TrophyManager>.Instance.Trophies.Where((Trophy trophy) => trophy.TrophyController.IsCompleted(isDefeat) && !trophy.TrophyController.IsOverrided(isDefeat)).ToList();
		List<Trophy> source2 = TPSingleton<TrophyManager>.Instance.Trophies.Where((Trophy trophy) => trophy.TrophyController.IsOverrided(isDefeat)).ToList();
		List<Trophy> source3 = TPSingleton<TrophyManager>.Instance.Trophies.Where((Trophy trophy) => !trophy.TrophyController.IsCompleted(isDefeat)).ToList();
		empty = empty + "- Completed" + text;
		foreach (TrophyController item in source.Select((Trophy x) => x.TrophyController))
		{
			empty = empty + "  " + item?.ToString() + text;
		}
		empty = empty + "- Overriden" + text;
		foreach (TrophyController item2 in source2.Select((Trophy x) => x.TrophyController))
		{
			empty = empty + "  " + item2?.ToString() + text;
		}
		empty = empty + "- Incomplete" + text;
		foreach (TrophyController item3 in source3.Select((Trophy x) => x.TrophyController))
		{
			empty = empty + "  " + item3?.ToString() + text;
		}
		return empty;
	}

	public void OnNightEnd(bool isDefeat)
	{
		if (ApplicationManager.Application.RunsCompleted != 0)
		{
			AutoGainDamnedSouls(isDefeat);
		}
	}

	public void RenewTrophies()
	{
		RawDamnedSoulsEarnedThisNight = 0;
		int trophyDefinitionIndex = 0;
		for (int count = TrophyDatabase.TrophyDefinitions.Count; trophyDefinitionIndex < count; trophyDefinitionIndex++)
		{
			if (Trophies.TryFind((Trophy x) => x.TrophyDefinition == TrophyDatabase.TrophyDefinitions[trophyDefinitionIndex], out var value))
			{
				value.TrophyConditionController.Clear();
			}
			else
			{
				Trophies.Add(new TrophyController(TrophyDatabase.TrophyDefinitions[trophyDefinitionIndex]).Trophy);
			}
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedTrophies serializedTrophies = container as SerializedTrophies;
		RenewTrophies();
		if (serializedTrophies == null)
		{
			return;
		}
		RawDamnedSoulsEarnedThisNight = serializedTrophies.RawDamnedSoulsEarnedThisNight;
		foreach (SerializedValueIntTrophy valueIntTrophy in serializedTrophies.ValueIntTrophies)
		{
			foreach (Trophy trophy in Trophies)
			{
				if (trophy.TrophyConditionController.Name == valueIntTrophy.Name)
				{
					trophy.TrophyConditionController.Deserialize(valueIntTrophy, saveVersion);
					break;
				}
			}
		}
		foreach (SerializedValueIntHeroesTrophy valueIntHeroesTrophy in serializedTrophies.ValueIntHeroesTrophies)
		{
			foreach (Trophy trophy2 in Trophies)
			{
				if (trophy2.TrophyConditionController.Name == valueIntHeroesTrophy.Name)
				{
					trophy2.TrophyConditionController.Deserialize(valueIntHeroesTrophy, saveVersion);
					break;
				}
			}
		}
		Trophies.FirstOrDefault((Trophy t) => t.TrophyConditionController is EnemiesDamagedTrophyConditionController)?.TrophyConditionController.Deserialize(serializedTrophies.EnemiesDamagedTrophy, saveVersion);
		Trophies.FirstOrDefault((Trophy t) => t.TrophyConditionController is EnemiesDebuffedSeveralTimesSingleTurnTrophyConditionController)?.TrophyConditionController.Deserialize(serializedTrophies.EnemiesDebuffedSeveralTimesSingleTurnTrophy, saveVersion);
	}

	public ISerializedData Serialize()
	{
		return new SerializedTrophies
		{
			RawDamnedSoulsEarnedThisNight = RawDamnedSoulsEarnedThisNight,
			ValueIntTrophies = (from t in Trophies
				where t.TrophyConditionController is ValueIntTrophyConditionController
				select t.TrophyConditionController.Serialize() as SerializedValueIntTrophy).ToList(),
			ValueIntHeroesTrophies = (from t in Trophies
				where t.TrophyConditionController is ValueIntHeroesTrophyConditionController valueIntHeroesTrophyConditionController && valueIntHeroesTrophyConditionController.ProgressionPerUnitId.Count > 0
				select t.TrophyConditionController.Serialize() as SerializedValueIntHeroesTrophy).ToList(),
			EnemiesDamagedTrophy = (Trophies.FirstOrDefault((Trophy t) => t.TrophyConditionController is EnemiesDamagedTrophyConditionController)?.TrophyConditionController.Serialize() as SerializedEnemiesDamagedTrophy),
			EnemiesDebuffedSeveralTimesSingleTurnTrophy = (Trophies.FirstOrDefault((Trophy t) => t.TrophyConditionController is EnemiesDebuffedSeveralTimesSingleTurnTrophyConditionController)?.TrophyConditionController.Serialize() as SerializedEnemiesDebuffedSeveralTimesSingleTurnTrophy)
		};
	}

	[DevConsoleCommand(Name = "ShowTrophiesProgressions")]
	public static void DebugShowTrophiesProgressions(bool isDefeat)
	{
		string text = "\r\n";
		string text2 = "==== TROPHIES PROGRESSION ====" + text;
		text2 += TPSingleton<TrophyManager>.Instance.GetProgressionLog(isDefeat);
		TPSingleton<TrophyManager>.Instance.Log(text2);
	}

	[DevConsoleCommand(Name = "UnlockTrophy")]
	public static void DebugUnlockTrophy(bool isDefeat, [StringConverter(typeof(Trophy.StringToTrophyIdConverter))] string trophyId = "")
	{
		TPSingleton<TrophyManager>.Instance.Trophies.Find((Trophy x) => x.TrophyDefinition.Id == trophyId).TrophyConditionController.ForceComplete();
		DebugShowTrophiesProgressions(isDefeat);
	}

	[DevConsoleCommand(Name = "UnlockAllTrophies")]
	public static void DebugUnlockAllTrophies()
	{
		foreach (Trophy trophy in TPSingleton<TrophyManager>.Instance.Trophies)
		{
			trophy.TrophyConditionController.ForceComplete();
		}
	}

	[DevConsoleCommand(Name = "RawSoulsEarnedThisNight")]
	public static void SetRawSoulsEarnedThisNight(int rawSouls)
	{
		TPSingleton<TrophyManager>.Instance.RawDamnedSoulsEarnedThisNight = rawSouls;
	}
}
