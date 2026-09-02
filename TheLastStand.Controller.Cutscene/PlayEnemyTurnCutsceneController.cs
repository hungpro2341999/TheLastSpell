using System.Collections;
using TPLib;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class PlayEnemyTurnCutsceneController : CutsceneController
{
	public PlayEnemyTurnCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		TPSingleton<GameManager>.Instance.Game.NightTurn = Game.E_NightTurn.EnemyUnits;
		while (BuildingManager.MagicCircle.DamageableModule.Health > 0f)
		{
			EnemyUnitManager.StartTurn();
			yield return TPSingleton<NightTurnsManager>.Instance.PlayTurn();
		}
	}
}
