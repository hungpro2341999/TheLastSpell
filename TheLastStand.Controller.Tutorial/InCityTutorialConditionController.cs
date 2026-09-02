using TPLib;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager.WorldMap;

namespace TheLastStand.Controller.Tutorial;

public class InCityTutorialConditionController : TutorialConditionController
{
	protected InCityTutorialConditionDefinition InCityConditionDefinition => base.ConditionDefinition as InCityTutorialConditionDefinition;

	public InCityTutorialConditionController(TutorialConditionDefinition conditionDefinition)
		: base(conditionDefinition)
	{
	}

	public override bool IsValid()
	{
		bool flag = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id == InCityConditionDefinition.CityId;
		if (!base.ConditionDefinition.Invert)
		{
			return flag;
		}
		return !flag;
	}
}
