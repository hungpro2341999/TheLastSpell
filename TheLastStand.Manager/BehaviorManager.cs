using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using TheLastStand.View.Sound;
using UnityEngine;

namespace TheLastStand.Manager;

public abstract class BehaviorManager<T> : Manager<T> where T : SerializedMonoBehaviour
{
	protected SkillCasterAttackGroups skillGroups = new SkillCasterAttackGroups();

	private Dictionary<string, AudioClipsPerId> skillGroupAudioClips = new Dictionary<string, AudioClipsPerId>();

	[SerializeField]
	protected bool turboMode;

	public TargetWaitSettings[] WaitBeforeSkillGroupDurations = new TargetWaitSettings[8];

	public TargetWaitSettings[] WaitAfterSkillGroupDurations = new TargetWaitSettings[8];

	public Dictionary<IDamageable, GroupTargetingInfo> CurrentlyTargetedDamageables = new Dictionary<IDamageable, GroupTargetingInfo>();

	public abstract List<IBehaviorModel> BehaviorModels { get; }

	public bool IsDone { get; protected set; } = true;

	public virtual void ComputeGoals(List<IBehaviorModel> sortedCasters)
	{
		Log("Computing goals for casters", CLogLevel.MAJOR);
		foreach (IBehaviorModel sortedCaster in sortedCasters)
		{
			if (!(sortedCaster is EnemyUnit enemyUnit) || (!enemyUnit.IsDeathRattling && !enemyUnit.IsDead))
			{
				sortedCaster.BehaviorController.ComputeCurrentGoals(CurrentlyTargetedDamageables);
			}
		}
		CurrentlyTargetedDamageables.Clear();
	}

	public virtual IEnumerator ExecuteBehaviorModelsSkillsCoroutine()
	{
		return ExecuteBehaviorModelsSkillsCoroutine(BehaviorModels);
	}

	public virtual IEnumerator ExecuteBehaviorModelsSkillsCoroutine(List<IBehaviorModel> behaviors)
	{
		Log("Executing " + GetType().Name + " BehaviorModels skills", CLogLevel.MAJOR);
		ACameraView.AllowUserZoom = false;
		ExecuteTurnForBehaviorModels(behaviors);
		Log("Waiting for " + GetType().Name + " to be done...");
		yield return new WaitUntil(() => IsDone);
		ACameraView.AllowUserZoom = true;
		Log(GetType().Name + " is done!");
	}

	public void ExecuteSkillsForGroup(SkillCasterAttackGroup attackGroup)
	{
		foreach (ComputedGoal item in attackGroup.GoalsToExecute)
		{
			if (!(item.Goal.Owner is IDamageable { IsDead: not false }))
			{
				item.Goal.Owner.BehaviorController.ExecuteGoal(item);
			}
		}
	}

	public virtual void ExecuteTurnForBehaviorModels(List<IBehaviorModel> behaviors)
	{
		IsDone = false;
		List<IBehaviorModel> skillCasters = RemoveSkippedBehaviours(behaviors);
		skillCasters = SortBehaviors(skillCasters);
		ComputeGoals(skillCasters);
		GatherUnitsByGroups(skillCasters);
		if (skillGroups.Count > 0)
		{
			Log("Executing turn for skillGroups.", CLogLevel.MAJOR);
			TPSingleton<T>.Instance.StartCoroutine(PrepareSkillsForGroups());
		}
		else
		{
			Log("Behaviors skill groups are all empty, end of turn.", CLogLevel.MAJOR);
			IsDone = true;
		}
	}

	public bool EnsureUnitTargeting(IBehaviorModel caster, int goalIndex)
	{
		ComputedGoal computedGoal = caster.CurrentGoals[goalIndex];
		if (computedGoal == null || computedGoal.Goal == null || computedGoal.TargetTileInfo == null)
		{
			return false;
		}
		computedGoal.Goal.Cooldown = computedGoal.Goal.GoalDefinition.Cooldown;
		Tile tile = null;
		TheLastStand.Model.Building.Building building = computedGoal.TargetTileInfo.Tile.Building;
		TheLastStand.Model.Unit.Unit unit = computedGoal.TargetTileInfo.Tile.Unit;
		ValidTargets.Constraints value;
		Tile tile3;
		switch (computedGoal.TargetTileType)
		{
		case SkillCasterAttackGroup.E_Target.ATTACK_BUILDING:
			if (building == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			goto IL_0120;
		case SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE:
			if (building == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			goto IL_0120;
		case SkillCasterAttackGroup.E_Target.ATTACK_ENEMY:
			if (unit == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			if (unit != caster)
			{
				goto IL_0229;
			}
			goto default;
		case SkillCasterAttackGroup.E_Target.ATTACK_HERO:
			if (unit == null)
			{
				goto case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY;
			}
			if (unit != caster)
			{
				goto IL_0229;
			}
			goto default;
		case SkillCasterAttackGroup.E_Target.ATTACK_EMPTY:
		{
			ValidTargets validTargets = computedGoal.Goal.Skill.SkillDefinition.ValidTargets;
			if ((validTargets == null || validTargets.EmptyTiles) && (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange || computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(computedGoal.TargetTileInfo.Tile)))
			{
				tile = computedGoal.TargetTileInfo.Tile;
			}
			break;
		}
		default:
			{
				if (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange || computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.IsInLineOfSight(computedGoal.TargetTileInfo.Tile))
				{
					tile = computedGoal.TargetTileInfo.Tile;
				}
				break;
			}
			IL_0120:
			if (computedGoal.Goal.Skill.SkillDefinition.ValidTargets == null || (computedGoal.Goal.Skill.SkillDefinition.ValidTargets.Buildings != null && computedGoal.Goal.Skill.SkillDefinition.ValidTargets.Buildings.TryGetValue(building.BuildingDefinition.Id, out value) && (!value.MustBeEmpty || unit == null)))
			{
				Tile tile2 = (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange ? EnsureBuildingTile(computedGoal.TargetType, computedGoal.TargetTileInfo.Tile) : EnsureBuildingTileInLOS(computedGoal.TargetType, building, computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles));
				if (tile2 != null && computedGoal.Goal.Skill.SkillAction.SkillActionController.IsBuildingAffected(tile2))
				{
					tile = tile2;
				}
			}
			break;
			IL_0229:
			tile3 = (computedGoal.Goal.Skill.SkillDefinition.InfiniteRange ? computedGoal.TargetTileInfo.Tile : computedGoal.Goal.Skill.SkillAction.SkillActionExecution.InRangeTiles.GetTileInLineOfSight(unit));
			if (tile3 != null && computedGoal.Goal.Skill.SkillAction.SkillActionController.IsUnitAffected(tile3))
			{
				tile = tile3;
			}
			break;
		}
		if (tile == null)
		{
			caster.LogWarning("Ended up with a null targetTile! Skipping my turn, something might be wrong here.");
			return false;
		}
		if (tile != computedGoal.TargetTileInfo.Tile)
		{
			caster.CurrentGoals[goalIndex] = new ComputedGoal(computedGoal.Goal, new SkillTargetedTileInfo(tile, computedGoal.TargetTileInfo.Orientation));
		}
		caster.TargetTile = tile;
		caster.Log($"Set target tile to : {tile.Position}", CLogLevel.DETAILED);
		return true;
	}

	public void GatherUnitsByGroups(List<IBehaviorModel> skillCasters)
	{
		skillGroups.Clear();
		List<ComputedGoal> goalsToExecute = GetGoalsToExecute(skillCasters);
		skillGroups.Init(goalsToExecute);
	}

	public virtual IEnumerator MoveCameraAndExecute(SkillCasterCluster skillCasterCluster)
	{
		if (skillCasterCluster.TryGetFirstComputedGoalWithPreCastFXs(out var computedGoal))
		{
			ACameraView.MoveTo(((MonoBehaviour)computedGoal.Goal.Holder.TileObjectView).transform.position, CameraView.AnimationMoveSpeed);
			yield return SharedYields.WaitForSeconds(CameraView.AnimationMoveSpeed);
			float num = computedGoal.Goal.Skill.SkillAction.SkillActionExecution.SkillExecutionController.PlayPreCastFxs();
			if (num > 0f)
			{
				yield return SharedYields.WaitForSeconds(num);
			}
		}
		Log($"{skillCasterCluster.SkillCasterAttackGroups.Count} SkillCasterAttackGroups for cluster {skillCasterCluster.ClusterOrder} with target {skillCasterCluster.TargetType}, moving camera to {skillCasterCluster.WorldPositionFocus}", CLogLevel.DETAILED);
		switch (skillCasterCluster.TargetType)
		{
		case SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE:
			ACameraView.MoveTo(BuildingManager.MagicCircle.BuildingView.transform.position, CameraView.AnimationMoveSpeed);
			break;
		case SkillCasterAttackGroup.E_Target.ATTACK_HERO:
		case SkillCasterAttackGroup.E_Target.ATTACK_BUILDING:
		case SkillCasterAttackGroup.E_Target.ATTACK_ENEMY:
		case SkillCasterAttackGroup.E_Target.GENERIC:
		case SkillCasterAttackGroup.E_Target.SPAWN:
		case SkillCasterAttackGroup.E_Target.OTHER:
			ACameraView.MoveTo(skillCasterCluster.WorldPositionFocus, CameraView.AnimationMoveSpeed);
			break;
		}
		Log($"Waiting for {WaitBeforeSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration}s. --- BeforeSkillGroup", CLogLevel.DETAILED);
		yield return SharedYields.WaitForSeconds(WaitBeforeSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration);
		yield return ExecuteSkillsForGroups(skillCasterCluster);
		TPSingleton<EffectTimeEventManager>.Instance.InvokeEvent(E_EffectTime.OnBehaviorClusterExecutionEnd);
		Log($"Waiting for {WaitAfterSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration}s. --- AfterSkillGroup", CLogLevel.DETAILED);
		yield return SharedYields.WaitForSeconds(WaitAfterSkillGroupDurations[(uint)skillCasterCluster.TargetType].Duration);
		yield return TPSingleton<PlayableUnitManager>.Instance.WaitUntilDeathSequences;
	}

	public virtual IEnumerator PrepareSkillsForGroups()
	{
		if (!turboMode)
		{
			IsDone = false;
			Log("Starting skill preparation for groups");
			for (int skillGroupIndex = 0; skillGroupIndex < skillGroups.Count; skillGroupIndex++)
			{
				yield return MoveCameraAndExecute(skillGroups[skillGroupIndex]);
			}
			IsDone = true;
		}
	}

	public virtual List<IBehaviorModel> SortBehaviors(List<IBehaviorModel> skillCasters, bool shuffleList = true)
	{
		new List<IBehaviorModel>();
		return (from caster in skillCasters
			orderby caster.BehaviourDefinition.GoalsComputingOrder, TileMapController.DistanceBetweenTiles(TileMapController.GetCenterTile(), caster.OriginTile)
			select caster).ThenBy((IBehaviorModel _) => (!shuffleList) ? 1 : RandomManager.GetRandomRange(TPSingleton<T>.Instance, 0, skillCasters.Count)).ToList();
	}

	protected bool CheckIfSkillGroupsAreDoneWithSkillExecution(List<SkillCasterAttackGroup> skillCasterAttackGroups)
	{
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in skillCasterAttackGroups)
		{
			foreach (ComputedGoal item in skillCasterAttackGroup.GoalsToExecute)
			{
				if (item.Goal.Owner.IsExecutingSkill)
				{
					return false;
				}
			}
		}
		return true;
	}

	protected virtual IEnumerator ExecuteSkillsForGroups(SkillCasterCluster skillCasterCluster)
	{
		Log($"Executing skills for SkillCasterAttackGroups in Cluster {skillCasterCluster.ClusterOrder} with target {skillCasterCluster.TargetType}");
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in skillCasterCluster.SkillCasterAttackGroups)
		{
			ExecuteSkillsForGroup(skillCasterAttackGroup);
		}
		ExecuteSkillSound(skillCasterCluster.SkillCasterAttackGroups);
		yield return new WaitUntil(() => CheckIfSkillGroupsAreDoneWithSkillExecution(skillCasterCluster.SkillCasterAttackGroups));
	}

	protected List<ComputedGoal> GetGoalsToExecute(List<IBehaviorModel> skillCasters)
	{
		List<ComputedGoal> list = new List<ComputedGoal>();
		foreach (IBehaviorModel skillCaster in skillCasters)
		{
			for (int i = 0; i < skillCaster.CurrentGoals.Length; i++)
			{
				if (EnsureUnitTargeting(skillCaster, i) && skillCaster.CurrentGoals[i].Goal.GoalDefinition.SkillId != "SkipTurn" && skillCaster.CurrentGoals[i].Goal.GoalDefinition.SkillId != "GargoyleSkipTurn2")
				{
					list.Add(skillCaster.CurrentGoals[i]);
				}
			}
		}
		return list;
	}

	protected List<IBehaviorModel> RemoveSkippedBehaviours(List<IBehaviorModel> behaviors, bool updateSkippedTurns = true)
	{
		List<IBehaviorModel> list = new List<IBehaviorModel>(behaviors);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (list[num].TurnsToSkipOnSpawn > 0)
			{
				if (updateSkippedTurns)
				{
					list[num].TurnsToSkipOnSpawn--;
				}
				list.RemoveAt(num);
			}
		}
		return list;
	}

	private Tile EnsureBuildingTile(SkillCasterAttackGroup.E_Target targetType, Tile defaultTile)
	{
		if (targetType != SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE)
		{
			return defaultTile;
		}
		return BuildingManager.MagicCircle.OriginTile;
	}

	private Tile EnsureBuildingTileInLOS(SkillCasterAttackGroup.E_Target targetType, ITileObject iTileObject, TilesInRangeInfos tilesInRangeInfos)
	{
		if (targetType != SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE || !tilesInRangeInfos.IsInLineOfSight(BuildingManager.MagicCircle.OriginTile))
		{
			return tilesInRangeInfos.GetTileInLineOfSight(iTileObject);
		}
		return BuildingManager.MagicCircle.OriginTile;
	}

	private void ExecuteSkillSound(List<SkillCasterAttackGroup> skillCasterAttackGroups)
	{
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in skillCasterAttackGroups)
		{
			PlaySkillSound(skillCasterAttackGroup.SkillSoundId, skillCasterAttackGroup.GoalsToExecute.Count, skillCasterAttackGroup.GoalsToExecute[0].Goal.Owner.OriginTile, skillCasterAttackGroup.GoalsToExecute[0].TargetTileInfo.Tile);
		}
	}

	protected void PlaySkillSound(string skillId, int behaviorsCount, Tile launchTile, Tile impactTile)
	{
		skillId = (EnemyUnitDatabase.SkillSoundIdOverrides.ContainsKey(skillId) ? EnemyUnitDatabase.SkillSoundIdOverrides[skillId] : skillId);
		if (!skillGroupAudioClips.ContainsKey(skillId))
		{
			ComputeSkillGroupAudioClips(skillId);
		}
		List<RangeDefinition> enemySkillSoundRangeDefinitions = EnemyUnitDatabase.EnemySkillSoundRangeDefinitions;
		int num = enemySkillSoundRangeDefinitions.Count - 1;
		while (num > 0 && behaviorsCount <= enemySkillSoundRangeDefinitions[num - 1].Range.y)
		{
			num--;
		}
		for (int num2 = num; num2 >= 0; num2--)
		{
			if (TryPlaySkillSound(skillId, launchTile, impactTile, enemySkillSoundRangeDefinitions[num2].Id))
			{
				return;
			}
		}
		LogWarning("No sound found for " + skillId);
	}

	protected virtual string GetSkillSoundClipPathFormat()
	{
		LogError("Skill sound path isn't defined for this behavior manager!");
		return string.Empty;
	}

	protected virtual string GetSkillSoundLaunchPathFormat()
	{
		LogError("Skill sound (launch) path isn't defined for this behavior manager!");
		return string.Empty;
	}

	protected virtual string GetSkillSoundImpactPathFormat()
	{
		LogError("Skill sound (impact) path isn't defined for this behavior manager!");
		return string.Empty;
	}

	protected virtual OneShotSound GetPooledSkillSoundAudioSource()
	{
		LogError("Skill sound audio source isn't defined for this behavior manager!");
		return null;
	}

	protected virtual OneShotSound GetSpatializedPooledSkillSoundAudioSource()
	{
		LogError("Skill sound audio source isn't defined for this behavior manager!");
		return null;
	}

	private void ComputeSkillGroupAudioClips(string skillGroupId)
	{
		AudioClipsPerId audioClipsPerId = new AudioClipsPerId();
		foreach (RangeDefinition enemySkillSoundRangeDefinition in EnemyUnitDatabase.EnemySkillSoundRangeDefinitions)
		{
			audioClipsPerId[enemySkillSoundRangeDefinition.Id] = new AudioClips
			{
				Clips = FetchAllSounds(string.Format(GetSkillSoundClipPathFormat(), skillGroupId, enemySkillSoundRangeDefinition.Id)),
				LaunchClips = FetchAllSounds(string.Format(GetSkillSoundLaunchPathFormat(), skillGroupId, enemySkillSoundRangeDefinition.Id)),
				ImpactClips = FetchAllSounds(string.Format(GetSkillSoundImpactPathFormat(), skillGroupId, enemySkillSoundRangeDefinition.Id))
			};
		}
		skillGroupAudioClips[skillGroupId] = audioClipsPerId;
	}

	private List<AudioClip> FetchAllSounds(string prefix)
	{
		List<AudioClip> list = new List<AudioClip>();
		int num = 1;
		while (true)
		{
			AudioClip audioClip = ResourcePooler<AudioClip>.LoadOnce(string.Format("{0}_{1}{2}", prefix, (num < 10) ? "0" : string.Empty, num), failSilently: true);
			if (!(audioClip != null))
			{
				break;
			}
			list.Add(audioClip);
			num++;
		}
		return list;
	}

	private bool TryPlaySkillSound(string skillId, Tile launchTile, Tile impactTile, string suffix)
	{
		bool result = false;
		if (skillGroupAudioClips[skillId][suffix].Clips.Count > 0)
		{
			result = true;
			OneShotSound pooledSkillSoundAudioSource = GetPooledSkillSoundAudioSource();
			pooledSkillSoundAudioSource.gameObject.name = skillId;
			pooledSkillSoundAudioSource.Play(skillGroupAudioClips[skillId][suffix].Clips.PickRandom());
		}
		if (skillGroupAudioClips[skillId][suffix].LaunchClips.Count > 0)
		{
			result = true;
			OneShotSound spatializedPooledSkillSoundAudioSource = GetSpatializedPooledSkillSoundAudioSource();
			spatializedPooledSkillSoundAudioSource.gameObject.name = skillId + "_Launch";
			spatializedPooledSkillSoundAudioSource.PlaySpatialized(skillGroupAudioClips[skillId][suffix].LaunchClips.PickRandom(), launchTile);
		}
		if (skillGroupAudioClips[skillId][suffix].ImpactClips.Count > 0)
		{
			result = true;
			OneShotSound spatializedPooledSkillSoundAudioSource2 = GetSpatializedPooledSkillSoundAudioSource();
			spatializedPooledSkillSoundAudioSource2.gameObject.name = skillId + "_Impact";
			spatializedPooledSkillSoundAudioSource2.PlaySpatialized(skillGroupAudioClips[skillId][suffix].ImpactClips.PickRandom(), impactTile);
		}
		return result;
	}
}
