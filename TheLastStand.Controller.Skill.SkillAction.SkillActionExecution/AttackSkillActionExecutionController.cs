using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit.Pathfinding;
using TheLastStand.Database.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;
using TheLastStand.View.Sound;
using UnityEngine;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class AttackSkillActionExecutionController : SkillActionExecutionController
{
	private float blowDamagesTotal;

	private int blowKillsCount;

	private int activeMomentumTilesBeforeSkillEffects;

	private readonly List<TheLastStand.Model.Unit.Unit> checkedDeadUnits = new List<TheLastStand.Model.Unit.Unit>();

	public AttackSkillActionExecution AttackSkillActionExecution => base.SkillActionExecution as AttackSkillActionExecution;

	public AttackSkillActionExecutionController(TheLastStand.Model.Skill.Skill skill)
		: base(skill)
	{
		AttackSkillActionExecutionView attackSkillActionExecutionView = new AttackSkillActionExecutionView();
		base.SkillActionExecution = new AttackSkillActionExecution(this, attackSkillActionExecutionView, skill);
		attackSkillActionExecutionView.SkillExecution = base.SkillActionExecution;
	}

	protected override List<SkillActionResultDatas> ApplyEffects(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		IEnumerable<Tile> affectedTiles2 = affectedTiles.Concat(surroundingTiles);
		FreezeIsolationStateOnTiles(freeze: true, affectedTiles2);
		List<SkillActionResultDatas> result = base.ApplyEffects(caster, skill, affectedTiles, surroundingTiles);
		FreezeIsolationStateOnTiles(freeze: false, affectedTiles2);
		return result;
	}

	protected override void ApplySkillEffects(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, Tile currentTargetTile, List<SkillActionResultDatas> resultData, TileObjectSelectionManager.E_Orientation specificOrientation)
	{
		if (caster is TheLastStand.Model.Unit.Unit unit)
		{
			if (unit is PlayableUnit playableUnit)
			{
				activeMomentumTilesBeforeSkillEffects = playableUnit.MomentumTilesActive;
			}
			FollowSkillExecution(unit, skill, currentTargetTile, resultData);
		}
		base.ApplySkillEffects(caster, skill, currentTargetTile, resultData, specificOrientation);
	}

	protected override void PlaySkillSFXs(Tile currentTargetTile)
	{
		base.PlaySkillSFXs(currentTargetTile);
		AttackSkillAction attackSkillAction = (AttackSkillAction)AttackSkillActionExecution.Skill.SkillAction;
		if (attackSkillAction.HeroHitsEnemyCount > 0)
		{
			if (attackSkillAction.HeroHitsEnemyCount >= SkillManager.HitHardAudioClip.Item1 && SkillManager.HitHardAudioClip.Item2 != null)
			{
				ObjectPooler.GetPooledGameObject("Skill SFX Spatialized", ResourcePooler.LoadOnce<GameObject>("Prefab/Skills SFXs/Skill SFX Spatialized")).GetComponent<OneShotSound>().PlaySpatialized(SkillManager.HitHardAudioClip.Item2, currentTargetTile);
			}
			else if (attackSkillAction.HeroHitsEnemyCount >= SkillManager.HitMediumAudioClip.Item1 && SkillManager.HitMediumAudioClip.Item2 != null)
			{
				ObjectPooler.GetPooledGameObject("Skill SFX Spatialized", ResourcePooler.LoadOnce<GameObject>("Prefab/Skills SFXs/Skill SFX Spatialized")).GetComponent<OneShotSound>().PlaySpatialized(SkillManager.HitMediumAudioClip.Item2, currentTargetTile);
			}
			else if (attackSkillAction.HeroHitsEnemyCount >= SkillManager.HitSoftAudioClip.Item1 && SkillManager.HitSoftAudioClip.Item2 != null)
			{
				ObjectPooler.GetPooledGameObject("Skill SFX Spatialized", ResourcePooler.LoadOnce<GameObject>("Prefab/Skills SFXs/Skill SFX Spatialized")).GetComponent<OneShotSound>().PlaySpatialized(SkillManager.HitSoftAudioClip.Item2, currentTargetTile);
			}
		}
		if (attackSkillAction.HeroHitsHeroCount > 0)
		{
			if (EnemyUnitDatabase.HitByEnemySoundDefinitions.TryGetValue("PlayableUnits", out var value))
			{
				string soundId = value.GetSoundId(attackSkillAction.HeroHitsHeroCount);
				ObjectPooler.GetPooledComponent("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab).Play(ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/PlayableUnitHits/" + soundId));
			}
			else
			{
				TPSingleton<SkillManager>.Instance.LogWarning("No sound found", CLogLevel.DETAILED);
			}
		}
		if (attackSkillAction.HeroHitsBuildings.Count > 0)
		{
			if (EnemyUnitDatabase.HitByEnemySoundDefinitions.TryGetValue("Buildings", out var value2))
			{
				string soundId2 = value2.GetSoundId(attackSkillAction.HeroHitsBuildings.Count);
				ObjectPooler.GetPooledComponent("HitsSFX Spatialized", TPSingleton<PlayableUnitManager>.Instance.HitSFXSpatializedPrefab).PlaySpatialized(ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/BuildingHits/" + soundId2), currentTargetTile);
			}
			else
			{
				TPSingleton<SkillManager>.Instance.LogWarning("No sound found", CLogLevel.DETAILED);
			}
		}
	}

	protected override void ResetAfterExecution()
	{
		base.ResetAfterExecution();
		if (base.SkillActionExecution.Caster is PlayableUnit playableUnit)
		{
			playableUnit.LifetimeStats.LifetimeStatsController.TryOverrideMostUnitsKilledInOneBlow(blowKillsCount);
			playableUnit.LifetimeStats.LifetimeStatsController.TryOverrideBestBlow(blowDamagesTotal);
			if (base.SkillActionExecution.Skill.HasMomentum)
			{
				playableUnit.PlayableUnitController.DecreaseActiveMomentumTiles(activeMomentumTilesBeforeSkillEffects);
			}
		}
		activeMomentumTilesBeforeSkillEffects = 0;
		blowKillsCount = 0;
		blowDamagesTotal = 0f;
		checkedDeadUnits.Clear();
	}

	protected override void ResetBetweenMultiHits(ISkillCaster caster, TheLastStand.Model.Skill.Skill skill, List<SkillActionResultDatas> resultData, List<Tile> allTilesInAoe)
	{
		UpdateBlow(resultData);
		List<TheLastStand.Model.Unit.Unit> list = new List<TheLastStand.Model.Unit.Unit>();
		foreach (SkillActionResultDatas resultDatum in resultData)
		{
			list.AddRange(resultDatum.AffectedUnits);
		}
		base.ResetBetweenMultiHits(caster, skill, resultData, allTilesInAoe);
	}

	private void FollowSkillExecution(TheLastStand.Model.Unit.Unit caster, TheLastStand.Model.Skill.Skill skill, Tile currentTargetTile, List<SkillActionResultDatas> resultsDatas)
	{
		if (!skill.SkillAction.HasEffect("Follow"))
		{
			return;
		}
		foreach (SkillActionResultDatas resultsData in resultsDatas)
		{
			foreach (TheLastStand.Model.Unit.Unit affectedUnit in resultsData.AffectedUnits)
			{
				if (affectedUnit.IsDead && PathfindingController.CanCross(caster, currentTargetTile))
				{
					if (caster.Path == null)
					{
						caster.Path = new List<Tile>();
					}
					else
					{
						caster.Path.Clear();
					}
					caster.Path.Add(caster.OriginTile);
					caster.Path.Add(currentTargetTile);
					if (caster is PlayableUnit playableUnit)
					{
						TrophyManager.AppendValueToTrophiesConditions<TilesMovedUsingSkillsTrophyConditionController>(new object[2]
						{
							playableUnit.RandomId,
							TileMapController.DistanceBetweenTiles(currentTargetTile, caster.OriginTile)
						});
						playableUnit.PlayableUnitController.AddCrossedTiles(TileMapController.DistanceBetweenTiles(caster.OriginTile, currentTargetTile), skill.HasNoMomentum);
					}
					caster.UnitController.PrepareForMovement(playWalkAnim: true, followPathOrientation: false, skill.SkillDefinition.SkillCastFxDefinition.FollowFxDefinition.Speed.EvalToFloat(base.SkillActionExecution.CastFx.CastFXInterpreterContext), skill.SkillDefinition.SkillCastFxDefinition.FollowFxDefinition.Delay.EvalToFloat(base.SkillActionExecution.CastFx.CastFXInterpreterContext)).StartTask();
					return;
				}
			}
		}
	}

	private void FreezeIsolationStateOnTiles(bool freeze, IEnumerable<Tile> affectedTiles)
	{
		foreach (Tile affectedTile in affectedTiles)
		{
			TheLastStand.Model.Unit.Unit unit = affectedTile.Unit;
			if (unit != null && !unit.IsDead)
			{
				affectedTile.Unit.FreezeIsolationState(freeze);
			}
		}
	}

	private void FreezeIsolationStateOnUnits(bool freeze, IEnumerable<TheLastStand.Model.Unit.Unit> affectedUnits)
	{
		foreach (TheLastStand.Model.Unit.Unit affectedUnit in affectedUnits)
		{
			if (affectedUnit != null && !affectedUnit.IsDead)
			{
				affectedUnit.FreezeIsolationState(freeze);
			}
		}
	}

	private void UpdateBlow(List<SkillActionResultDatas> resultData)
	{
		for (int num = resultData.Count - 1; num >= 0; num--)
		{
			if (resultData[num] != null)
			{
				foreach (TheLastStand.Model.Unit.Unit affectedUnit in resultData[num].AffectedUnits)
				{
					if (affectedUnit != null && (affectedUnit.IsDead || affectedUnit.HasBeenExiled) && !checkedDeadUnits.Contains(affectedUnit))
					{
						blowKillsCount++;
						checkedDeadUnits.Add(affectedUnit);
					}
				}
				blowDamagesTotal += resultData[num].TotalDamagesToHealth;
			}
		}
	}
}
