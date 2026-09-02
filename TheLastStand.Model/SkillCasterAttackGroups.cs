using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TPLib.Log;
using TheLastStand.Dev;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Model;

public class SkillCasterAttackGroups : List<SkillCasterCluster>
{
	public static bool MaxDistancesAreInBound(KMeansClustersInfo clusters)
	{
		return !clusters.maxDistanceByCluster.Any((Vector2 distance) => Mathf.Abs(distance.x) > CameraView.CameraVision.ColliderSize.x / 2f || Mathf.Abs(distance.y) > CameraView.CameraVision.ColliderSize.y / 2f);
	}

	public void Init(List<ComputedGoal> goalsToExecute)
	{
		if (goalsToExecute.Count == 0)
		{
			return;
		}
		List<ComputedGoal> list = goalsToExecute.Where((ComputedGoal goal) => goal.TargetType == SkillCasterAttackGroup.E_Target.ATTACK_HERO || goal.TargetType == SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE).ToList();
		List<ComputedGoal> list2 = goalsToExecute.Except(list).ToList();
		KMeansClustersInfo kMeansClustersInfo = null;
		if (list2.Count > 0)
		{
			int[] array = GenerateBaseKClusterCentroids(list2.Select((ComputedGoal computedGoal) => computedGoal.TargetTileInfo.Tile).ToList());
			int num = Mathf.Max(0, array.Length);
			int num2 = Mathf.Max(0, (num - num % 2) / 2 - 1);
			int totalWeight = Mathf.Max(1, TPSingleton<SectorManager>.Instance.TargetFocusCameraWeight + TPSingleton<SectorManager>.Instance.CasterFocusCameraWeight);
			List<Vector2> data = list2.Select((ComputedGoal computedGoal) => (TileMapView.GetTileCenter(computedGoal.TargetTileInfo.Tile) * TPSingleton<SectorManager>.Instance.TargetFocusCameraWeight + TileMapView.GetTileCenter(computedGoal.Goal.Owner.OriginTile) * TPSingleton<SectorManager>.Instance.CasterFocusCameraWeight) / totalWeight).ToList();
			while (kMeansClustersInfo == null || !MaxDistancesAreInBound(kMeansClustersInfo))
			{
				kMeansClustersInfo = KMeans.GetBestClusters(data, ++num2, 10, 50, null, MaxDistancesAreInBound);
			}
		}
		Enrich(list2, kMeansClustersInfo, list);
		MergeClustersIfNeeded();
		SortByClusterThenTargetType();
	}

	private void Enrich(List<ComputedGoal> data, KMeansClustersInfo clusters, List<ComputedGoal> specificTargetGoals)
	{
		if (specificTargetGoals != null)
		{
			for (int i = 0; i < specificTargetGoals.Count; i++)
			{
				IBehaviorModel owner = specificTargetGoals[i].Goal.Owner;
				ComputedGoal computedGoal = specificTargetGoals[i];
				Tile tile = computedGoal.TargetTileInfo.Tile;
				SkillCasterAttackGroup.E_Target targetType = computedGoal.TargetType;
				SkillCasterCluster skillCasterCluster = null;
				if (computedGoal.CanBeMerged)
				{
					using Enumerator enumerator = GetEnumerator();
					while (enumerator.MoveNext())
					{
						SkillCasterCluster current = enumerator.Current;
						if (current.TargetType == targetType && current.CanBeMerged && (current.TargetedPlayableUnits.Any((PlayableUnit x) => computedGoal.TargetPlayableUnits.Contains(x)) || targetType == SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE))
						{
							skillCasterCluster = current;
							break;
						}
					}
				}
				if (skillCasterCluster == null)
				{
					skillCasterCluster = targetType switch
					{
						SkillCasterAttackGroup.E_Target.ATTACK_HERO => new SkillCasterCluster(GetAverageUnitWorldPosition(computedGoal.TargetPlayableUnits), targetType, computedGoal.TargetPlayableUnits, -1, default(Vector2), computedGoal.CanBeMerged), 
						SkillCasterAttackGroup.E_Target.MAGIC_CIRCLE => new SkillCasterCluster(BuildingManager.MagicCircle.OriginTile, targetType, computedGoal.TargetPlayableUnits, -1, default(Vector2), computedGoal.CanBeMerged), 
						_ => new SkillCasterCluster(tile.OriginTile, targetType, computedGoal.TargetPlayableUnits, -1, default(Vector2), computedGoal.CanBeMerged), 
					};
					Add(skillCasterCluster);
				}
				else if (computedGoal.TargetPlayableUnits.Count != 0)
				{
					skillCasterCluster.TargetedPlayableUnits.AddRange(computedGoal.TargetPlayableUnits);
					skillCasterCluster.WorldPositionFocus = GetAverageUnitWorldPosition(skillCasterCluster.TargetedPlayableUnits);
				}
				SkillCasterAttackGroup skillCasterAttackGroup = skillCasterCluster.SkillCasterAttackGroups.FirstOrDefault((SkillCasterAttackGroup attackGroup) => attackGroup.SkillSoundId == computedGoal.Goal.Skill.SkillDefinition.SoundId);
				if (skillCasterAttackGroup == null)
				{
					skillCasterAttackGroup = new SkillCasterAttackGroup(computedGoal);
					skillCasterCluster.SkillCasterAttackGroups.Add(skillCasterAttackGroup);
				}
				else if (!skillCasterAttackGroup.GoalsToExecute.Contains(computedGoal))
				{
					skillCasterAttackGroup.GoalsToExecute.Add(computedGoal);
				}
				owner.Log($"Added into skillGroup cluster {skillCasterCluster.ClusterOrder} with target {targetType} and target tile {tile}", CLogLevel.DETAILED);
			}
		}
		if (clusters == null)
		{
			return;
		}
		Vector2 lastCameraPosition = this.LastOrDefault((SkillCasterCluster x) => x.TargetType == SkillCasterAttackGroup.E_Target.ATTACK_HERO)?.WorldPositionFocus ?? this.LastOrDefault()?.WorldPositionFocus ?? ACameraView.MainCam.transform.position;
		List<Vector2> list = clusters.means.ToList();
		int[] clusterOrder = new int[clusters.means.Length];
		for (int num = 0; num < clusters.means.Length; num++)
		{
			lastCameraPosition = list.OrderBy((Vector2 x) => (x - lastCameraPosition).magnitude).First();
			list.Remove(lastCameraPosition);
			clusterOrder[Array.IndexOf(clusters.means, lastCameraPosition)] = num;
		}
		for (int num2 = 0; num2 < clusters.clusterIdByData.Length; num2++)
		{
			IBehaviorModel owner2 = data[num2].Goal.Owner;
			ComputedGoal computedGoal2 = data[num2];
			Tile tile2 = computedGoal2.TargetTileInfo.Tile;
			SkillCasterAttackGroup.E_Target targetType2 = computedGoal2.TargetType;
			int clusterId = clusters.clusterIdByData[num2];
			SkillCasterCluster skillCasterCluster2 = null;
			if (computedGoal2.CanBeMerged)
			{
				skillCasterCluster2 = this.FirstOrDefault((SkillCasterCluster casterCluster) => casterCluster.ClusterOrder == clusterOrder[clusterId] && casterCluster.TargetType == targetType2 && casterCluster.CanBeMerged);
			}
			if (skillCasterCluster2 == null)
			{
				skillCasterCluster2 = new SkillCasterCluster((computedGoal2.Goal.Skill.SkillAction.SkillActionExecution.PreCastFx?.CastFxDefinition != null) ? tile2.TileView.transform.position : ((Vector3)clusters.means[clusterId]), targetType2, null, clusterOrder[clusterId], clusters.maxDistanceByCluster[clusterId], computedGoal2.CanBeMerged);
				Add(skillCasterCluster2);
			}
			SkillCasterAttackGroup skillCasterAttackGroup2 = skillCasterCluster2.SkillCasterAttackGroups.FirstOrDefault((SkillCasterAttackGroup attackGroup) => attackGroup.SkillSoundId == computedGoal2.Goal.Skill.SkillDefinition.SoundId);
			if (skillCasterAttackGroup2 == null)
			{
				skillCasterAttackGroup2 = new SkillCasterAttackGroup(computedGoal2);
				skillCasterCluster2.SkillCasterAttackGroups.Add(skillCasterAttackGroup2);
			}
			else if (!skillCasterAttackGroup2.GoalsToExecute.Contains(computedGoal2))
			{
				skillCasterAttackGroup2.GoalsToExecute.Add(computedGoal2);
			}
			owner2.Log($"Added into skillGroup cluster {skillCasterCluster2.ClusterOrder} with target {targetType2} and target tile {tile2}", CLogLevel.DETAILED);
		}
	}

	private int[] GenerateBaseKClusterCentroids(List<Tile> data)
	{
		Dictionary<CameraAreaOfInterest, int> dictionary = new Dictionary<CameraAreaOfInterest, int>();
		foreach (Tile datum in data)
		{
			Vector3 tileWorldPos = TileMapView.GetTileCenter(datum);
			IEnumerable<CameraAreaOfInterest> source = from x in TPSingleton<SectorManager>.Instance.Sectors
				where x.AreaCollider.OverlapPoint(tileWorldPos)
				orderby x.AreaWeight descending
				select x;
			CameraAreaOfInterest cameraAreaOfInterest = ((!source.Any()) ? TPSingleton<SectorManager>.Instance.Sectors.OrderBy((CameraAreaOfInterest x) => (x.transform.position - tileWorldPos).magnitude - x.AreaWeight).First() : source.First());
			if (!dictionary.ContainsKey(cameraAreaOfInterest))
			{
				dictionary.Add(cameraAreaOfInterest, data.IndexOf(datum));
				continue;
			}
			Vector3 vector = TileMapView.GetTileCenter(data[dictionary[cameraAreaOfInterest]]);
			if (Mathf.Abs((cameraAreaOfInterest.transform.position - tileWorldPos).magnitude) < Mathf.Abs((cameraAreaOfInterest.transform.position - vector).magnitude))
			{
				dictionary[cameraAreaOfInterest] = data.IndexOf(datum);
			}
		}
		return dictionary.Values.ToArray();
	}

	private Vector3 GetAverageUnitWorldPosition(HashSet<PlayableUnit> units)
	{
		Vector3 zero = Vector3.zero;
		foreach (PlayableUnit unit in units)
		{
			zero += unit.DamageableView.GameObject.transform.position;
		}
		return zero / units.Count;
	}

	private void MergeClustersIfNeeded()
	{
		List<SkillCasterCluster> clustersToRemove = new List<SkillCasterCluster>();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				SkillCasterCluster skillCasterCluster = enumerator.Current;
				if (skillCasterCluster.TargetType != SkillCasterAttackGroup.E_Target.ATTACK_HERO || !skillCasterCluster.CanBeMerged)
				{
					continue;
				}
				SkillCasterCluster skillCasterCluster2 = (from cluster in this
					where !clustersToRemove.Contains(cluster) && cluster.CanBeMerged && cluster.TargetType == SkillCasterAttackGroup.E_Target.ATTACK_HERO && cluster.TargetedPlayableUnits.Any((PlayableUnit unit) => skillCasterCluster.TargetedPlayableUnits.Contains(unit))
					orderby cluster.TargetedPlayableUnits.Count descending
					select cluster).FirstOrDefault();
				if (skillCasterCluster2 != null && skillCasterCluster2 != skillCasterCluster && skillCasterCluster2.TargetedPlayableUnits.Count >= skillCasterCluster.TargetedPlayableUnits.Count)
				{
					clustersToRemove.Add(skillCasterCluster);
					skillCasterCluster2.TargetedPlayableUnits.AddRange(skillCasterCluster.TargetedPlayableUnits);
					skillCasterCluster2.SkillCasterAttackGroups.AddRange(skillCasterCluster.SkillCasterAttackGroups);
					skillCasterCluster2.WorldPositionFocus = GetAverageUnitWorldPosition(skillCasterCluster2.TargetedPlayableUnits);
				}
			}
		}
		foreach (SkillCasterCluster item in clustersToRemove)
		{
			Remove(item);
		}
	}

	private void SortByClusterThenTargetType()
	{
		Sort(delegate(SkillCasterCluster x, SkillCasterCluster y)
		{
			int num = x.ClusterOrder.CompareTo(y.ClusterOrder);
			if (num == 0 || x.ClusterOrder == -1 || y.ClusterOrder == -1)
			{
				num = x.TargetType.CompareTo(y.TargetType);
			}
			return num;
		});
	}
}
