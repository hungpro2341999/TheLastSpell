using System.Collections;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class ExileAllEnemiesCutsceneController : CutsceneController
{
	public ExileAllEnemiesCutsceneDefinition ExileAllEnemiesCutsceneDefinition => base.CutsceneDefinition as ExileAllEnemiesCutsceneDefinition;

	public ExileAllEnemiesCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (!ExileAllEnemiesCutsceneDefinition.OnlyIfVictoryTriggered || TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory)
		{
			TPSingleton<EnemyUnitManager>.Instance.ExileAllUnits(countAsKills: false, disableDieAnim: false, new List<TheLastStand.Model.Unit.Unit> { cutsceneData.Unit });
		}
		yield break;
	}
}
