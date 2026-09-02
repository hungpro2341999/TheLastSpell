using TPLib;
using TheLastStand.Definition.Tutorial;
using TheLastStand.Manager.Item;

namespace TheLastStand.Controller.Tutorial;

public class IsWeaponRestrictionAvailableTutorialConditionController : TutorialConditionController
{
	protected IsWeaponRestrictionAvailableTutorialConditionDefinition IsWeaponRestrictionAvailableConditionDefinition => base.ConditionDefinition as IsWeaponRestrictionAvailableTutorialConditionDefinition;

	public IsWeaponRestrictionAvailableTutorialConditionController(TutorialConditionDefinition conditionDefinition)
		: base(conditionDefinition)
	{
	}

	public override bool IsValid()
	{
		bool isAvailable = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsAvailable;
		if (!base.ConditionDefinition.Invert)
		{
			return isAvailable;
		}
		return !isAvailable;
	}
}
