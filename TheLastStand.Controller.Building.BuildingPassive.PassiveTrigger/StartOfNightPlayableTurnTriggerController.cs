using TPLib;
using TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;
using TheLastStand.Manager;
using TheLastStand.Model.Building.BuildingPassive.PassiveTrigger;

namespace TheLastStand.Controller.Building.BuildingPassive.PassiveTrigger;

public class StartOfNightPlayableTurnTriggerController : PassiveTriggerController
{
	public StartOfNightPlayableTurnTrigger StartOfNightPlayableTurnTrigger => base.PassiveTrigger as StartOfNightPlayableTurnTrigger;

	public StartOfNightPlayableTurnTriggerController(StartOfNightPlayableTurnTriggerDefinition definition)
		: base(definition)
	{
		base.PassiveTrigger = new StartOfNightPlayableTurnTrigger(definition, this);
	}

	public override bool UpdateAndCheckTrigger(bool OnLoad)
	{
		return TPSingleton<GameManager>.Instance.Game.CurrentNightHour == 1;
	}
}
