using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class DamageInflictedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "DamageInflicted";

	public DamageInflictedTrophyDefinition DamageInflictedTrophyDefinition => base.TrophyConditionDefinition as DamageInflictedTrophyDefinition;

	public DamageInflictedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type DamageInflicted need only 1 argument", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type DamageInflicted should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type DamageInflicted should have as second argument a float !", CLogLevel.DETAILED);
			return;
		}
		if (base.ProgressionPerUnitId.ContainsKey(key))
		{
			base.ProgressionPerUnitId[key] += (int)num;
		}
		else
		{
			base.ProgressionPerUnitId.Add(key, (int)num);
		}
		OnValueChange();
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 2)
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type DamageInflicted need only 1 argument", CLogLevel.DETAILED);
			return;
		}
		if (!(args[0] is int key))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type DamageInflicted should have as first argument an int !", CLogLevel.DETAILED);
			return;
		}
		if (!(args[1] is float num))
		{
			TPSingleton<TrophyManager>.Instance.LogError("Trophy Condition of Type DamageInflicted should have as second argument a float !", CLogLevel.DETAILED);
			return;
		}
		if (base.ProgressionPerUnitId.ContainsKey(key))
		{
			base.ProgressionPerUnitId[key] = (int)num;
		}
		else
		{
			base.ProgressionPerUnitId.Add(key, (int)num);
		}
		OnValueChange();
	}
}
