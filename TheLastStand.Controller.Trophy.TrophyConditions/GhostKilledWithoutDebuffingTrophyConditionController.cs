using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class GhostKilledWithoutDebuffingTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "GhostKilledWithoutDebuffing";

	public GhostKilledWithoutDebuffingTrophyDefinition GhostKilledWithoutDebuffingTrophyDefinition => base.TrophyConditionDefinition as GhostKilledWithoutDebuffingTrophyDefinition;

	public GhostKilledWithoutDebuffingTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type GhostKilledWithoutDebuffing need only 2 argument", CLogLevel.DETAILED);
		}
		else if (!(args[0] is PlayableUnit playableUnit))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type GhostKilledWithoutDebuffing should have as first argument a Model.Unit.PlayableUnit !", CLogLevel.DETAILED);
		}
		else if (!(args[1] is int item))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type GhostKilledWithoutDebuffing should have as second argument an int !", CLogLevel.DETAILED);
		}
		else if (!playableUnit.PlayableUnitController.DebuffedByGhosts.Contains(item))
		{
			if (!base.ProgressionPerUnitId.ContainsKey(playableUnit.RandomId))
			{
				base.ProgressionPerUnitId.Add(playableUnit.RandomId, 1);
			}
			else
			{
				base.ProgressionPerUnitId[playableUnit.RandomId]++;
			}
			OnValueChange();
		}
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type GhostKilledWithoutDebuffing need only 2 argument", CLogLevel.DETAILED);
		}
		else if (!(args[0] is PlayableUnit playableUnit))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type GhostKilledWithoutDebuffing should have as first argument a Model.Unit.PlayableUnit !", CLogLevel.DETAILED);
		}
		else if (!(args[1] is int item))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type GhostKilledWithoutDebuffing should have as second argument an int !", CLogLevel.DETAILED);
		}
		else if (!playableUnit.PlayableUnitController.DebuffedByGhosts.Contains(item))
		{
			if (!base.ProgressionPerUnitId.ContainsKey(playableUnit.RandomId))
			{
				base.ProgressionPerUnitId.Add(playableUnit.RandomId, 1);
			}
			else
			{
				base.ProgressionPerUnitId[playableUnit.RandomId] = 1;
			}
			OnValueChange();
		}
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (GhostKilledWithoutDebuffingTrophyDefinition.Target == "All")
		{
			text += $"    Hero(es) has killed {GetTotal()} Ghost(s) without being debuffed. \r\n";
		}
		foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
		{
			text += $"    {GetUnitName(item.Key)} killed {item.Value} Ghost(s) without being debuffed. \r\n";
		}
		return text;
	}
}
