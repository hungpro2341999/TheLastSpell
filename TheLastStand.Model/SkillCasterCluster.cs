using System.Collections.Generic;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Model;

public class SkillCasterCluster
{
	public Vector3 WorldPositionFocus { get; set; }

	public int ClusterOrder { get; }

	public bool CanBeMerged { get; }

	public Vector2 MaxDistance { get; }

	public List<SkillCasterAttackGroup> SkillCasterAttackGroups { get; } = new List<SkillCasterAttackGroup>();

	public SkillCasterAttackGroup.E_Target TargetType { get; }

	public HashSet<PlayableUnit> TargetedPlayableUnits { get; }

	public SkillCasterCluster(Vector3 worldPositionFocus, SkillCasterAttackGroup.E_Target targetType, HashSet<PlayableUnit> targetedPlayableUnits = null, int clusterOrder = -1, Vector2 maxDistance = default(Vector2), bool canBeMerged = true)
	{
		WorldPositionFocus = worldPositionFocus;
		ClusterOrder = clusterOrder;
		MaxDistance = maxDistance;
		TargetType = targetType;
		TargetedPlayableUnits = targetedPlayableUnits ?? new HashSet<PlayableUnit>();
		CanBeMerged = canBeMerged;
	}

	public SkillCasterCluster(Tile tileFocus, SkillCasterAttackGroup.E_Target targetType, HashSet<PlayableUnit> targetedPlayableUnits = null, int clusterOrder = -1, Vector2 maxDistance = default(Vector2), bool canBeMerged = true)
		: this((Vector2)tileFocus.TileView.transform.position, targetType, targetedPlayableUnits, clusterOrder, maxDistance, canBeMerged)
	{
	}

	public bool TryGetFirstComputedGoalWithPreCastFXs(out ComputedGoal computedGoalWithPreCastFXs)
	{
		foreach (SkillCasterAttackGroup skillCasterAttackGroup in SkillCasterAttackGroups)
		{
			foreach (ComputedGoal item in skillCasterAttackGroup.GoalsToExecute)
			{
				if (item.Goal.Skill.SkillAction.SkillActionExecution.PreCastFx?.CastFxDefinition != null)
				{
					computedGoalWithPreCastFXs = item;
					return true;
				}
			}
		}
		computedGoalWithPreCastFXs = null;
		return false;
	}
}
