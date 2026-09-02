using System.Collections;
using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class DestroyActorPhaseActionController : ABossPhaseActionController
{
	public DestroyActorPhaseActionDefinition DestroyActorPhaseActionDefinition => base.ABossPhaseActionDefinition as DestroyActorPhaseActionDefinition;

	public DestroyActorPhaseActionController(DestroyActorPhaseActionDefinition destroyActorPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(destroyActorPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		string actorId = DestroyActorPhaseActionDefinition.ActorId;
		if (!TPSingleton<BossManager>.Instance.BossPhaseActors.TryGetValue(actorId, out var actors))
		{
			yield break;
		}
		bool cameraWasZoomed = ACameraView.IsZoomedIn;
		if (DestroyActorPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			ACameraView.Zoom(zoomIn: true);
		}
		if ((TPSingleton<BossManager>.Instance.CurrentBossPhase?.BossPhaseDefinition.ActorDefinitions.GetValueOrDefault(actorId))?.GetCorrespondingDefinition() is BuildingDefinition buildingDefinition && (buildingDefinition.BlueprintModuleDefinition.Category & BuildingDefinition.E_BuildingCategory.LitBrazier) == BuildingDefinition.E_BuildingCategory.LitBrazier)
		{
			TPSingleton<BuildingManager>.Instance.ExtinguishBraziers();
		}
		else
		{
			int actorsNbAfterDestroy = 0;
			if (DestroyActorPhaseActionDefinition.Amount > 0)
			{
				actorsNbAfterDestroy = Mathf.Max(actors.Count - DestroyActorPhaseActionDefinition.Amount, 0);
			}
			for (int i = actors.Count - 1; i >= actorsNbAfterDestroy; i--)
			{
				if (actors[i] is EnemyUnit enemyUnit)
				{
					if (DestroyActorPhaseActionDefinition.CameraFocus)
					{
						ACameraView.MoveTo(enemyUnit.DamageableView.GameObject.transform.position);
					}
					enemyUnit.EnemyUnitController.PrepareForDeath();
					enemyUnit.EnemyUnitView.PlayDieAnim();
					if (DestroyActorPhaseActionDefinition.WaitDeathAnim)
					{
						yield return enemyUnit.UnitView.WaitUntilDeathCanBeFinalized;
					}
				}
				else if (actors[i] is TheLastStand.Model.Building.Building building)
				{
					if (DestroyActorPhaseActionDefinition.CameraFocus)
					{
						ACameraView.MoveTo(building.BuildingView.transform.position);
					}
					BuildingManager.DestroyBuilding(building.OriginTile, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: false);
					if (DestroyActorPhaseActionDefinition.WaitDeathAnim)
					{
						yield return building.BuildingView.WaitUntilDestructionAnimationIsFinished;
					}
				}
			}
		}
		if (DestroyActorPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = true;
			ACameraView.AllowUserZoom = true;
			ACameraView.Zoom(cameraWasZoomed);
		}
	}
}
