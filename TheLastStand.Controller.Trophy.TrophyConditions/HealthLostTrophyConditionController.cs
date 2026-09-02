using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class HealthLostTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "HealthLost";

	public HealthLostTrophyDefinition HealthLostDefinition => base.TrophyConditionDefinition as HealthLostTrophyDefinition;

	public HealthLostTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthLost need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthLost should have as first argument a string !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthLost should have as second argument a float !", CLogLevel.DETAILED);
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
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthLost need only 2 arguments", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthLost should have as first argument a string !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type HealthLost should have as second argument a float !", CLogLevel.DETAILED);
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
}
