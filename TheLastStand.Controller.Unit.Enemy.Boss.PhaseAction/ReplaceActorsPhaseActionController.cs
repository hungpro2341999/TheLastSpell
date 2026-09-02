using System.Collections;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Boss;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy.Boss.PhaseAction;

public class ReplaceActorsPhaseActionController : ABossPhaseActionController
{
	private List<IBossPhaseActor> availableActorsToReplace;

	private UnitCreationSettings currentUnitCreationSettings;

	private ActorDefinition currentActorToSpawnDefinition;

	private TheLastStand.Framework.Serialization.Definition currentActorTypeToSpawnDefinition;

	public ReplaceActorsPhaseActionDefinition ReplaceActorsPhaseActionDefinition => base.ABossPhaseActionDefinition as ReplaceActorsPhaseActionDefinition;

	public ReplaceActorsPhaseActionController(ABossPhaseActionDefinition aBossPhaseActionDefinition, BossPhaseHandler bossPhaseHandlerParent, int actionIndex)
		: base(aBossPhaseActionDefinition, bossPhaseHandlerParent, actionIndex)
	{
	}

	public override IEnumerator Execute()
	{
		PickRandomActorToSpawn();
		InitActorsToReplace();
		if (availableActorsToReplace.Count == 0)
		{
			yield break;
		}
		if (ReplaceActorsPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = false;
			ACameraView.AllowUserZoom = false;
			ACameraView.Zoom(zoomIn: true);
		}
		int currentReplacedActorsNb = 0;
		int actorsToReplaceNb = ((ReplaceActorsPhaseActionDefinition.Amount <= 0) ? availableActorsToReplace.Count : ReplaceActorsPhaseActionDefinition.Amount);
		while (availableActorsToReplace.Count > 0 && currentReplacedActorsNb < actorsToReplaceNb)
		{
			int randomRange = RandomManager.GetRandomRange(this, 0, availableActorsToReplace.Count);
			IBossPhaseActor currentBossPhaseActor = availableActorsToReplace[randomRange];
			Tile tile = currentBossPhaseActor.OriginTile;
			if (ReplaceActorsPhaseActionDefinition.CameraFocus && currentBossPhaseActor != null)
			{
				ACameraView.MoveTo(currentBossPhaseActor.TileObjectView.GameObject.transform.position);
			}
			if (!(currentBossPhaseActor is EnemyUnit enemyUnit))
			{
				if (currentBossPhaseActor is TheLastStand.Model.Building.Building building)
				{
					BuildingManager.DestroyBuilding(building.OriginTile, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: false);
					if (ReplaceActorsPhaseActionDefinition.WaitDeathAnim)
					{
						yield return building.BuildingView.WaitUntilDestructionAnimationIsFinished;
					}
				}
			}
			else
			{
				enemyUnit.EnemyUnitController.PrepareForDeath();
				enemyUnit.EnemyUnitView.PlayDieAnim();
				if (ReplaceActorsPhaseActionDefinition.WaitDeathAnim)
				{
					yield return enemyUnit.EnemyUnitView.WaitUntilDeathCanBeFinalized;
				}
			}
			availableActorsToReplace.Remove(currentBossPhaseActor);
			if (ReplaceActorsPhaseActionDefinition.HasMultipleReplacementId && currentReplacedActorsNb > 0)
			{
				PickRandomActorToSpawn();
			}
			switch (currentActorToSpawnDefinition.ActorType)
			{
			case DamageableType.Enemy:
			{
				EnemyUnit enemyUnit2 = EnemyUnitManager.CreateEnemyUnit(currentActorTypeToSpawnDefinition as EnemyUnitTemplateDefinition, tile, currentUnitCreationSettings);
				if (currentUnitCreationSettings.WaitSpawnAnim)
				{
					yield return new WaitUntil(() => enemyUnit2.EnemyUnitView.AreAnimationsInitialized);
					yield return enemyUnit2.EnemyUnitView.WaitUntilAnimatorStateIsIdle;
				}
				break;
			}
			case DamageableType.Boss:
				yield return BossManager.CreateBossUnit(currentActorTypeToSpawnDefinition as BossUnitTemplateDefinition, tile, currentUnitCreationSettings);
				break;
			case DamageableType.Building:
			{
				TheLastStand.Model.Building.Building building2 = BuildingManager.CreateBuilding(currentActorTypeToSpawnDefinition as BuildingDefinition, tile, updateView: true, playSound: true, instantly: false, triggerEvent: true, currentUnitCreationSettings.BossPhaseActorId);
				if (currentUnitCreationSettings.WaitSpawnAnim)
				{
					yield return building2.BuildingView.WaitUntilConstructionAnimationIsFinished;
				}
				break;
			}
			}
			currentReplacedActorsNb++;
		}
		if (ReplaceActorsPhaseActionDefinition.CameraFocus)
		{
			ACameraView.AllowUserPan = true;
			ACameraView.AllowUserZoom = true;
		}
	}

	private void InitActorsToReplace()
	{
		availableActorsToReplace = new List<IBossPhaseActor>();
		for (int i = 0; i < ReplaceActorsPhaseActionDefinition.ActorsIds.Count; i++)
		{
			string text = ReplaceActorsPhaseActionDefinition.ActorsIds[i];
			bool flag = false;
			if (!TPSingleton<BossManager>.Instance.BossPhaseActors.TryGetValue(text, out var value))
			{
				value = new List<IBossPhaseActor>();
				TryAddNonActorsToActorsList(value, text);
				if (value.Count == 0)
				{
					continue;
				}
				flag = true;
			}
			if (!flag)
			{
				TryAddNonActorsToActorsList(value, text);
			}
			if (value.Count > 0)
			{
				availableActorsToReplace.AddRange(value);
			}
		}
	}

	private void PickRandomActorToSpawn()
	{
		currentUnitCreationSettings = ReplaceActorsPhaseActionDefinition.GetUnitCreationSettings();
		currentActorToSpawnDefinition = TPSingleton<BossManager>.Instance.CurrentBossPhase?.BossPhaseDefinition.ActorDefinitions.GetValueOrDefault(currentUnitCreationSettings.BossPhaseActorId);
		currentActorTypeToSpawnDefinition = currentActorToSpawnDefinition?.GetCorrespondingDefinition();
	}

	private bool TryAddNonActorsToActorsList(List<IBossPhaseActor> actors, string actorToReplaceId)
	{
		if (!ReplaceActorsPhaseActionDefinition.IncludeNonActor)
		{
			return false;
		}
		ActorDefinition actorDefinition = TPSingleton<BossManager>.Instance.CurrentBossPhase?.BossPhaseDefinition.ActorDefinitions.GetValueOrDefault(actorToReplaceId);
		if (actorDefinition == null)
		{
			return false;
		}
		switch (actorDefinition.ActorType)
		{
		case DamageableType.Enemy:
			foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
			{
				if (enemyUnit.EnemyUnitTemplateDefinition.Id == actorDefinition.UnitId && string.IsNullOrEmpty(enemyUnit.BossPhaseActorId))
				{
					actors.Add(enemyUnit);
				}
			}
			break;
		case DamageableType.Boss:
			foreach (BossUnit bossUnit in TPSingleton<BossManager>.Instance.BossUnits)
			{
				if (bossUnit.EnemyUnitTemplateDefinition.Id == actorDefinition.UnitId && string.IsNullOrEmpty(bossUnit.BossPhaseActorId))
				{
					actors.Add(bossUnit);
				}
			}
			break;
		case DamageableType.Building:
			foreach (TheLastStand.Model.Building.Building item in BuildingManager.GetBuildingsById(actorDefinition.UnitId))
			{
				if (string.IsNullOrEmpty(item.BossPhaseActorId))
				{
					actors.Add(item);
				}
			}
			break;
		}
		return true;
	}
}
