using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class NoHealthLostTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "NoHealthLost";

	public override bool ShouldCheckCompletionBeforeUpdate => false;

	public NoHealthLostTrophyDefinition NoHealthLostTrophyDefinition => base.TrophyConditionDefinition as NoHealthLostTrophyDefinition;

	public NoHealthLostTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
		Clear();
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type NoHealthLost need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type NoHealthLost should have as first argument a string !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type NoHealthLost should have as second argument a float !", CLogLevel.DETAILED);
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

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type NoHealthLost need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type NoHealthLost should have as first argument a string !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type NoHealthLost should have as second argument a float !", CLogLevel.DETAILED);
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

	protected override void OnValueChange()
	{
		if (!IsCompleted && IsCompleted != base.PrevCompletedState)
		{
			string message = $"You failed a souls Trophy : {base.TrophyModel.TrophyController}";
			TPSingleton<TrophyManager>.Instance.Log(message);
		}
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (NoHealthLostTrophyDefinition.Target == "All")
		{
			text += string.Format("    All heroes lost {0} HP, TrophyState : {1} \r\n", GetTotal(), ((int)GetTotal() == 0 && TPSingleton<GameManager>.Instance.Game.DayNumber >= NoHealthLostTrophyDefinition.Value) ? "Completed" : "Incompleted");
		}
		foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
		{
			text += string.Format("    {0} lost {1} HP TrophyState : {2}\r\n", GetUnitName(item.Key), item.Value, ((float)item.Value == 0f && TPSingleton<GameManager>.Instance.Game.DayNumber >= NoHealthLostTrophyDefinition.Value) ? "Completed" : "Incompleted");
		}
		return text;
	}

	protected override void CheckCompleteState()
	{
		switch (NoHealthLostTrophyDefinition.Target)
		{
		case "All":
		{
			int num = 0;
			foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
			{
				num += item.Value;
			}
			isCompleted = num == 0 && TPSingleton<GameManager>.Instance.Game.DayNumber >= NoHealthLostTrophyDefinition.Value;
			break;
		}
		case "One":
		{
			foreach (KeyValuePair<int, int> item2 in base.ProgressionPerUnitId)
			{
				if (item2.Value == 0 && TPSingleton<GameManager>.Instance.Game.DayNumber >= NoHealthLostTrophyDefinition.Value)
				{
					isCompleted = true;
					break;
				}
			}
			break;
		}
		}
	}
}
