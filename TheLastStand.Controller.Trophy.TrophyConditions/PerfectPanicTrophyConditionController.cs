using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class PerfectPanicTrophyConditionController : ValueIntTrophyConditionController
{
	public override string Name => "PerfectPanic";

	public PerfectPanicTrophyDefinition PerfectPanicTrophyDefinition => base.TrophyConditionDefinition as PerfectPanicTrophyDefinition;

	public PerfectPanicTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 1)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type PerfectPanic need only 1 argument", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type PerfectPanic should have as first argument a float !", CLogLevel.DETAILED);
			return;
		}
		base.ValueProgression += (int)num;
		OnValueChange();
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 1)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type PerfectPanic need only 1 argument", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type PerfectPanic should have as first argument a float !", CLogLevel.DETAILED);
			return;
		}
		base.ValueProgression = (int)num;
		OnValueChange();
	}

	public override string ToString()
	{
		return $"\r\nPanic Value shouldn't exceed 0 (current : {base.ValueProgression}) if the current index Day is {PerfectPanicTrophyDefinition.Value} or over.";
	}

	protected override void CheckCompleteState()
	{
		isCompleted = base.ValueProgression == 0 && TPSingleton<GameManager>.Instance.Game.DayNumber >= PerfectPanicTrophyDefinition.Value;
	}
}
