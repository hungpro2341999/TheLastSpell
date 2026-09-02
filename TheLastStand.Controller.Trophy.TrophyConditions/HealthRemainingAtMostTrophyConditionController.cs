using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class HealthRemainingAtMostTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "HealthRemainingAtMost";

	public HealthRemainingAtMostTrophyDefinition HealthRemainingAtMostTrophyDefinition => base.TrophyConditionDefinition as HealthRemainingAtMostTrophyDefinition;

	public HealthRemainingAtMostTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthRemainingAtMost need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthRemainingAtMost should have as first argument a string !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthRemainingAtMost should have as second argument a float !", CLogLevel.DETAILED);
			return;
		}
		if (!base.ProgressionPerUnitId.ContainsKey(key))
		{
			base.ProgressionPerUnitId.Add(key, (int)num);
		}
		else
		{
			base.ProgressionPerUnitId[key] += (int)num;
		}
		OnValueChange();
	}

	protected override void CheckCompleteState()
	{
		if (isCompleted)
		{
			return;
		}
		if (HealthRemainingAtMostTrophyDefinition.Target == "All")
		{
			float num = 0f;
			foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
			{
				num += (float)item.Value;
			}
			isCompleted = num <= (float)HealthRemainingAtMostTrophyDefinition.Value;
			return;
		}
		if (HealthRemainingAtMostTrophyDefinition.Target == "One")
		{
			foreach (KeyValuePair<int, int> item2 in base.ProgressionPerUnitId)
			{
				if (item2.Value <= HealthRemainingAtMostTrophyDefinition.Value)
				{
					base.FirstUnitToUnlockTrophy = item2.Key;
					isCompleted = true;
					return;
				}
			}
		}
		isCompleted = false;
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthRemainingAtMost need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthRemainingAtMost should have as first argument a string !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthRemainingAtMost should have as second argument a float !", CLogLevel.DETAILED);
			return;
		}
		if (!base.ProgressionPerUnitId.ContainsKey(key))
		{
			base.ProgressionPerUnitId.Add(key, (int)num);
		}
		else
		{
			base.ProgressionPerUnitId[key] = (int)num;
		}
		OnValueChange();
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (HealthRemainingAtMostTrophyDefinition.Target == "All")
		{
			text += string.Format("    The total shouldn't exceed {0}, current Total Value : {1} => {2}\r\n", HealthRemainingAtMostTrophyDefinition.Value, GetTotal(), ((float)GetTotal() <= (float)HealthRemainingAtMostTrophyDefinition.Value) ? "Completed" : "Incompleted");
		}
		foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
		{
			text += string.Format("    Health point of {0} should not exceed {1} currently {2} HP = {3} => {4}\r\n", GetUnitName(item.Key), HealthRemainingAtMostTrophyDefinition.Value, item.Key, item.Value, (item.Value <= HealthRemainingAtMostTrophyDefinition.Value) ? "Completed" : "Incompleted");
		}
		return text;
	}
}
