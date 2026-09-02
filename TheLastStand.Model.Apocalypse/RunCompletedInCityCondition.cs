using TPLib;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Manager.WorldMap;

namespace TheLastStand.Model.Apocalypse;

public class RunCompletedInCityCondition : ApocalypseUnlockCondition
{
	public override bool IsValid
	{
		get
		{
			if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id == ((RunCompletedInCityConditionDefinition)base.Definition).CityDefinitionId)
			{
				return TPSingleton<WorldMapCityManager>.Instance.SelectedCity.NumberOfWins >= ((RunCompletedInCityConditionDefinition)base.Definition).RunCompleted;
			}
			return false;
		}
	}

	public RunCompletedInCityCondition(RunCompletedInCityConditionDefinition definition)
		: base(definition)
	{
	}
}
