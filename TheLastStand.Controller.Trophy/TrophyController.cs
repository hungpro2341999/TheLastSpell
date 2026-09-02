using TPLib;
using TheLastStand.Definition.Trophy;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy;

public class TrophyController
{
	public TrophyDefinition TrophyDefinition { get; }

	public TheLastStand.Model.Trophy.Trophy Trophy { get; }

	public TrophyController(TrophyDefinition trophyDefinition)
	{
		TrophyDefinition = trophyDefinition;
		Trophy = new TheLastStand.Model.Trophy.Trophy(trophyDefinition, this);
	}

	public override string ToString()
	{
		string empty = string.Empty;
		if (TrophyDefinition.Condition is HeroesTrophyConditionDefinition heroesTrophyConditionDefinition)
		{
			string text = ((heroesTrophyConditionDefinition.Target == "One") ? "Single Hero" : "All heroes");
			empty = "<b>" + TrophyDefinition.Id + "</b> (" + text + ") :";
		}
		else
		{
			empty = "<b>" + TrophyDefinition.Id + "</b> :";
		}
		return empty + $"{Trophy.TrophyConditionController}";
	}

	public bool IsCompleted(bool isDefeat)
	{
		if (!isDefeat || !TrophyDefinition.IsLostOnDefeat)
		{
			return Trophy.TrophyConditionController.IsCompleted;
		}
		return false;
	}

	public bool IsCompletedWithoutUpdate(bool isDefeat)
	{
		if (!isDefeat || !TrophyDefinition.IsLostOnDefeat)
		{
			return Trophy.TrophyConditionController.IsCompletedWithoutUpdate;
		}
		return false;
	}

	public bool IsOverrided(bool isDefeat)
	{
		foreach (TheLastStand.Model.Trophy.Trophy trophy in TPSingleton<TrophyManager>.Instance.Trophies)
		{
			if (trophy.TrophyDefinition.TrophyToOverride == TrophyDefinition.Id && trophy.TrophyController.IsCompleted(isDefeat))
			{
				return true;
			}
		}
		return false;
	}
}
