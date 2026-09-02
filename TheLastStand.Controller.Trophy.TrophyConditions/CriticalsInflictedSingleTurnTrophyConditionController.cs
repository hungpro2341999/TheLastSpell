using System.Collections.Generic;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class CriticalsInflictedSingleTurnTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "CriticalsInflictedSingleTurn";

	public CriticalsInflictedSingleTurnTrophyDefinition CriticalsInflictedSingleTurnTrophyDefinition => base.TrophyConditionDefinition as CriticalsInflictedSingleTurnTrophyDefinition;

	public CriticalsInflictedSingleTurnTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override string ToString()
	{
		string text = "\r\n";
		if (CriticalsInflictedSingleTurnTrophyDefinition.Target == "All")
		{
			text += $"    Hero(es) achieved a total of {GetTotal()} Critical Hits this turn. \r\n";
		}
		foreach (KeyValuePair<int, int> item in base.ProgressionPerUnitId)
		{
			text += $"    {GetUnitName(item.Key)} achieved a total of {item.Value} Critical Hits this turn. \r\n";
		}
		return text;
	}
}
