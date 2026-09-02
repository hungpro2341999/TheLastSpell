using System.Collections;
using TPLib;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class PlayCutscenePhaseActionController : ABossPhaseActionController
{
	public PlayCutscenePhaseActionDefinition PlayCutscenePhaseAction => base.ABossPhaseActionDefinition as PlayCutscenePhaseActionDefinition;

	public PlayCutscenePhaseActionController(ABossPhaseActionDefinition aBossPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(aBossPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		GenericCutsceneView genericCutsceneView = TPSingleton<CutsceneManager>.Instance.GetGenericCutsceneView();
		genericCutsceneView.Init(PlayCutscenePhaseAction.CutsceneId, new CutsceneData(TPSingleton<WorldMapCityManager>.Instance.SelectedCity));
		CutsceneManager.PlayCutscene(genericCutsceneView);
		yield return genericCutsceneView.WaitUntilIsOver;
	}
}
