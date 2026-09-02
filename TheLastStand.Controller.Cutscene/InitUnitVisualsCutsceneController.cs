using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Definition;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Cutscene;

namespace TheLastStand.Controller.Cutscene;

public class InitUnitVisualsCutsceneController : CutsceneController
{
	public InitUnitVisualsCutsceneDefinition InitUnitVisualsCutsceneDefinition => base.CutsceneDefinition as InitUnitVisualsCutsceneDefinition;

	public InitUnitVisualsCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (cutsceneData.Unit == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("Tried to play a InitUnitVisualsCutsceneController with a null unit.");
			yield break;
		}
		EnemyUnit enemyUnit = cutsceneData.Unit as EnemyUnit;
		if (InitUnitVisualsCutsceneDefinition.WaitAppearanceDelay && enemyUnit != null && enemyUnit.EnemyUnitTemplateDefinition.AppearanceDelay > 0f)
		{
			yield return SharedYields.WaitForSeconds(enemyUnit.EnemyUnitTemplateDefinition.AppearanceDelay);
		}
		cutsceneData.Unit.UnitController.LookAtDirection(GameDefinition.E_Direction.South);
		cutsceneData.Unit.UnitView.InitVisuals(InitUnitVisualsCutsceneDefinition.PlaySpawnAnim);
		cutsceneData.Unit.UnitView.UpdatePosition();
		cutsceneData.Unit.UnitView.RefreshHudPositionInstantly();
		if (enemyUnit is EliteEnemyUnit eliteEnemyUnit)
		{
			eliteEnemyUnit.EliteEnemyUnitController.TriggerAffixes(E_EffectTime.OnCreationAfterViewInitialized);
		}
		if (InitUnitVisualsCutsceneDefinition.WaitSpawnAnim)
		{
			yield return cutsceneData.Unit.UnitView.WaitUntilAnimatorStateIsIdle;
		}
		if (InitUnitVisualsCutsceneDefinition.CastSpawnSkill && enemyUnit != null)
		{
			if (enemyUnit.EnemyUnitTemplateDefinition.CastSpawnSkillDelay > 0f)
			{
				yield return SharedYields.WaitForSeconds(enemyUnit.EnemyUnitTemplateDefinition.CastSpawnSkillDelay);
			}
			enemyUnit.EnemyUnitController.ExecuteSpawnGoals();
		}
	}
}
