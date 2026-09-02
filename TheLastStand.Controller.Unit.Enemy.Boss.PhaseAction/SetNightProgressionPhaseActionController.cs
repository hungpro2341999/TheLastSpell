using System.Collections;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.View;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class SetNightProgressionPhaseActionController : ABossPhaseActionController
{
	public SetNightProgressionPhaseActionDefinition SetNightProgressionPhaseActionDefinition => base.ABossPhaseActionDefinition as SetNightProgressionPhaseActionDefinition;

	public SetNightProgressionPhaseActionController(ABossPhaseActionDefinition aBossPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(aBossPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		GameView.TopScreenPanel.TurnPanel.PhasePanel.SetNextRefreshNightSliderValue(SetNightProgressionPhaseActionDefinition.Value / 100f);
		yield break;
	}
}
