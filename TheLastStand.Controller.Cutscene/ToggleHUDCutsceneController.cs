using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class ToggleHUDCutsceneController : CutsceneController
{
	public ToggleHUDCutsceneDefinition ToggleHUDCutsceneDefinition => base.CutsceneDefinition as ToggleHUDCutsceneDefinition;

	public ToggleHUDCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (!ToggleHUDCutsceneDefinition.OnlyIfVictoryTriggered || TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory)
		{
			yield return TPSingleton<UIManager>.Instance.ToggleUICoroutine(ToggleHUDCutsceneDefinition.Display);
		}
	}
}
