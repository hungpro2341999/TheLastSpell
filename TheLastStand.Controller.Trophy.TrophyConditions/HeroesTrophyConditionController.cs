using TPLib;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Trophy;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public abstract class HeroesTrophyConditionController : TrophyConditionController
{
	protected HeroesTrophyConditionDefinition HeroesTrophyConditionDefinition => base.TrophyConditionDefinition as HeroesTrophyConditionDefinition;

	protected int FirstUnitToUnlockTrophy { get; set; }

	public HeroesTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public abstract object GetTotal();

	public string GetUnitName(int id)
	{
		return TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Find((PlayableUnit x) => x.RandomId == id)?.PlayableUnitName;
	}
}
