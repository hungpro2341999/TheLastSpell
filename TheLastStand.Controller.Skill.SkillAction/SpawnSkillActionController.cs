using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Skill.SkillAction;

public class SpawnSkillActionController : SkillActionController
{
	public SpawnSkillAction SpawnSkillAction => base.SkillAction as SpawnSkillAction;

	public SpawnSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new SpawnSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new SpawnSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	public bool CanDestroyBuilding(TheLastStand.Model.Building.Building building)
	{
		if (building == null)
		{
			return true;
		}
		return SpawnSkillAction.SpawnSkillActionDefinition.BuildingIdsToDestroy.Contains(building.Id);
	}

	public void ComputeUnitsToSpawn()
	{
		if (!SpawnSkillAction.ComputedUnitsToSpawn)
		{
			if (!SpawnSkillAction.SpawnSkillActionDefinition.IsByAmount)
			{
				SpawnSkillAction.UnitToSpawnByWeight = ComputeEnemyToSpawnByWeight();
			}
			SpawnSkillAction.ComputedUnitsToSpawn = true;
		}
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
		KillSkillEffectDefinition effect;
		if (unit != null && !unit.IsDead)
		{
			return SpawnSkillAction.TryGetFirstEffect<KillSkillEffectDefinition>("Kill", out effect);
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		SpawnSkillAction.ComputedUnitsToSpawn = false;
		SpawnSkillAction.UnitToSpawnByWeight = null;
	}

	public bool ValidateCandidateTargetTile(Tile candidateTargetTile)
	{
		if (SpawnSkillAction.SpawnSkillActionDefinition.IsByAmount)
		{
			return true;
		}
		if (SpawnSkillAction.UnitToSpawnByWeight != null)
		{
			UnitTemplateDefinition unitTemplateDefinition = EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.GetValueOrDefault(SpawnSkillAction.UnitToSpawnByWeight.Item1) ?? EnemyUnitDatabase.EnemyUnitTemplateDefinitions.GetValueOrDefault(SpawnSkillAction.UnitToSpawnByWeight.Item1);
			if (CanDestroyBuilding(candidateTargetTile.Building))
			{
				return unitTemplateDefinition?.CanSpawnOn(candidateTargetTile, isPhaseActor: false, ignoreUnits: false, ignoreBuildings: true) ?? false;
			}
			return false;
		}
		return false;
	}

	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas resultData = new SkillActionResultDatas();
		bool flag = IsUnitAffected(targetTile);
		IsBuildingAffected(targetTile);
		if (flag && SpawnSkillAction.TryGetFirstEffect<KillSkillEffectDefinition>("Kill", out var effect))
		{
			ApplySkillEffectKill(caster, targetTile.Unit, effect, resultData);
		}
		if (SpawnSkillAction.SpawnSkillActionDefinition.IsByAmount)
		{
			SpawnEnemiesByAmount(caster, ref resultData);
			SpawnRandomEnemiesByAmount(caster, ref resultData);
		}
		else
		{
			SpawnAnEnemyByWeight(targetTile, ref resultData);
		}
		if (caster is BattleModule battleModule && battleModule.BuildingParent.IsCrystal && flag)
		{
			TPSingleton<AchievementManager>.Instance.HandleCrystalCorruptedEnemy();
		}
		return resultData;
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}

	private Tuple<string, UnitCreationSettings> ComputeEnemyToSpawnByWeight()
	{
		string item = string.Empty;
		UnitCreationSettings unitCreationSettings = null;
		int max = SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByWeight.Sum((EnemySpawnData enemySpawnData) => enemySpawnData.Weight);
		int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, max);
		int num = 0;
		foreach (EnemySpawnData item2 in SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByWeight)
		{
			if (randomRange >= num && randomRange < item2.Weight + num)
			{
				item = item2.Id;
				unitCreationSettings = item2.UnitCreationSettings;
				break;
			}
			num += item2.Weight;
		}
		return new Tuple<string, UnitCreationSettings>(item, unitCreationSettings ?? new UnitCreationSettings());
	}

	private Tile GetSpawnTile(EnemySpawnData enemySpawnData, SkillActionResultDatas resultData)
	{
		UnitTemplateDefinition unitTemplateDefinition = EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.GetValueOrDefault(enemySpawnData.Id) ?? EnemyUnitDatabase.EnemyUnitTemplateDefinitions[enemySpawnData.Id];
		if (enemySpawnData.TileFlag == TileFlagDefinition.E_TileFlagTag.None)
		{
			if (base.SkillAction.SkillActionExecution.TargetTiles.Count <= 0)
			{
				return null;
			}
			return base.SkillAction.SkillActionExecution.TargetTiles[0].Tile;
		}
		return TileMapManager.GetRandomSpawnableTileWithFlag(enemySpawnData.TileFlag, unitTemplateDefinition, ValidatePredicate);
		bool ValidatePredicate(Tile tile)
		{
			if (CanDestroyBuilding(tile.Building))
			{
				return !resultData.UnitsToSpawnTarget.ContainsKey(tile);
			}
			return false;
		}
	}

	private void SpawnRandomEnemiesByAmount(ISkillCaster caster, ref SkillActionResultDatas resultData)
	{
		int count = SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies.Count;
		int num = SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemiesAmount.EvalToInt(GameManager.FormulaInterpreterContext);
		if (count == 0 && num > 0)
		{
			TPSingleton<SkillManager>.Instance.LogError("Trying to spawn random enemies by amount with an empty enemies list! Skill Id: " + base.SkillAction.Skill.SkillDefinition.Id);
			return;
		}
		int index = 0;
		for (int i = 0; i < num; i++)
		{
			int num2 = 0;
			for (int j = 0; j < count; j++)
			{
				num2 += SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[j].Weight;
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, num2);
			int num3 = 0;
			for (int k = 0; k < count; k++)
			{
				if (randomRange >= num3 && randomRange < SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[k].Weight + num3)
				{
					index = k;
					break;
				}
				num3 += SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[k].Weight;
			}
			EnemySpawnData enemySpawnData = SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemies[index];
			Tile spawnTile = GetSpawnTile(enemySpawnData, resultData);
			SpawnEnemy(enemySpawnData, spawnTile, caster, ref resultData);
		}
	}

	private void SpawnEnemiesByAmount(ISkillCaster caster, ref SkillActionResultDatas resultData)
	{
		if (SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByAmount.Count == 0)
		{
			if (SpawnSkillAction.SpawnSkillActionDefinition.RandomEnemiesAmount.EvalToInt(GameManager.FormulaInterpreterContext) == 0)
			{
				TPSingleton<SkillManager>.Instance.LogError("Trying to spawn enemies by amount with an empty enemies list and no random enemies to compensate! Skill Id: " + base.SkillAction.Skill.SkillDefinition.Id);
			}
			return;
		}
		for (int i = 0; i < SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByAmount.Count; i++)
		{
			EnemySpawnData enemySpawnData = SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByAmount[i];
			int num = enemySpawnData.Amount.EvalToInt(GameManager.FormulaInterpreterContext);
			for (int j = 0; j < num; j++)
			{
				Tile spawnTile = GetSpawnTile(enemySpawnData, resultData);
				SpawnEnemy(enemySpawnData, spawnTile, caster, ref resultData);
			}
		}
	}

	private void SpawnAnEnemyByWeight(Tile targetTile, ref SkillActionResultDatas resultData)
	{
		if (SpawnSkillAction.SpawnSkillActionDefinition.EnemiesByWeight.Count == 0)
		{
			TPSingleton<SkillManager>.Instance.LogError("Trying to spawn enemies by weight with an empty enemies list! Skill Id: " + base.SkillAction.Skill.SkillDefinition.Id);
		}
		else if (targetTile.Unit == null)
		{
			TheLastStand.Model.Building.Building building = targetTile.Building;
			if (building != null && !building.CanSpawnEnemyOnIt)
			{
				BuildingManager.DestroyBuilding(targetTile);
			}
			ComputeUnitsToSpawn();
			resultData.UnitsToSpawnTarget.Add(targetTile, (SpawnSkillAction.UnitToSpawnByWeight.Item1, SpawnSkillAction.UnitToSpawnByWeight.Item2));
		}
	}

	private void SpawnEnemy(EnemySpawnData enemySpawnData, Tile tile, ISkillCaster caster, ref SkillActionResultDatas resultData)
	{
		if (tile == null)
		{
			return;
		}
		TheLastStand.Model.Building.Building building = tile.Building;
		if (building != null && !building.CanSpawnEnemyOnIt)
		{
			BuildingManager.DestroyBuilding(tile);
		}
		if (caster is BossUnit { IsDeathRattling: false })
		{
			int sectorIndexForTile = TPSingleton<SectorManager>.Instance.GetSectorIndexForTile(tile);
			if (TPSingleton<BossManager>.Instance.RecentlySpawnedUnitsBySector[sectorIndexForTile].ContainsKey(base.SkillAction.Skill))
			{
				TPSingleton<BossManager>.Instance.RecentlySpawnedUnitsBySector[sectorIndexForTile][base.SkillAction.Skill].Item2.AddAtKey((enemySpawnData.Id, enemySpawnData.UnitCreationSettings), tile);
				return;
			}
			TPSingleton<BossManager>.Instance.RecentlySpawnedUnitsBySector[sectorIndexForTile].Add(base.SkillAction.Skill, new Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>(caster, new Dictionary<(string, UnitCreationSettings), List<Tile>> { 
			{
				(enemySpawnData.Id, enemySpawnData.UnitCreationSettings),
				new List<Tile> { tile }
			} }));
		}
		else
		{
			resultData.UnitsToSpawnTarget.Add(tile, (enemySpawnData.Id, enemySpawnData.UnitCreationSettings));
		}
	}
}
