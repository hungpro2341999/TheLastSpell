using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.Serialization.SpawnWave;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Enemy;

public class SpawnWaveController
{
	public readonly SpawnWave SpawnWave;

	public SpawnWaveController(SpawnWaveDefinition definition, SpawnWaveView spawnWaveView, bool isReroll = false, SpawnDirectionsDefinition spawnDirectionsDefinition = null)
	{
		SpawnWave = new SpawnWave(definition, this, spawnWaveView);
		SpawnWave.SpawnWaveView.SpawnWave = SpawnWave;
		float num = SpawnWaveManager.SpawnDefinition.SpawnsCountPerWave.EvalToFloat(TPSingleton<SpawnWaveManager>.Instance.SpawnWaveInterpreterObject);
		SpawnWave.SpawnsCount = ComputeCountWithExternalModifiers(Mathf.RoundToInt(num * SpawnWave.SpawnWaveDefinition.SpawnsCountMultiplier));
		TPSingleton<SpawnWaveManager>.Instance.Log("Spawn wave: " + SpawnWave.SpawnWaveDefinition.Id, CLogLevel.MAJOR);
		TPSingleton<SpawnWaveManager>.Instance.Log($"I'm evaluating the formula {SpawnWaveManager.SpawnDefinition.SpawnsCountPerWave} to the value {num}.\nMultiplying this value with the scm {SpawnWave.SpawnWaveDefinition.SpawnsCountMultiplier}, rounding and obtaining a SpawnCount of {SpawnWave.SpawnsCount}", CLogLevel.DETAILED);
		BossManager.ResetPhases();
		if (definition.IsBossWave)
		{
			BossManager.Init(definition.WaveEnemiesDefinition.BossWaveSettings.BossUnitTemplateId);
		}
		PopulateSpawnWave(isReroll);
		SpawnWave.SpawnDirectionsDefinition = spawnDirectionsDefinition ?? SpawnWaveManager.GetRandomSpawnWaveDirection(isReroll);
		if (spawnDirectionsDefinition != null)
		{
			SpawnWave.RotationsCount = 0;
		}
		else
		{
			List<int> list = SpawnWaveManager.AllowedRotationCountsPerDirection[SpawnWave.SpawnDirectionsDefinition];
			if (isReroll && list.Count > 1)
			{
				list.Remove(SpawnWaveManager.RerolledSpawnWave.RotationsCount);
			}
			SpawnWave.RotationsCount = RandomManager.GetRandomElement(TPSingleton<SpawnWaveManager>.Instance, list);
		}
		SpawnWave.RotatedProportionPerDirection = RotateDirection(SpawnWave.SpawnDirectionsDefinition.SpawnDirectionsInfo, SpawnWave.RotationsCount);
		TPSingleton<SpawnWaveManager>.Instance.Log(string.Format("Spawn direction pattern: {0} rotated {1} {2} clockwise.", SpawnWave.SpawnDirectionsDefinition.Id, SpawnWave.RotationsCount, (SpawnWave.RotationsCount > 1) ? "times" : "time"));
		if (SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count != SpawnWave.SpawnsCount)
		{
			TPSingleton<SpawnWaveManager>.Instance.LogWarning($"Something may have gone very wrong in the generation of spawn wave! I expected a spawn count of {SpawnWave.SpawnsCount} but the final number of enemies will be {SpawnWave.RemainingEnemiesToSpawn.Count} + {SpawnWave.RemainingEliteEnemiesToSpawn.Count}(elites).");
			TPSingleton<SpawnWaveManager>.Instance.Log($"Evaluated {SpawnWaveManager.SpawnDefinition.SpawnsCountPerWave} to {num}, multiplied by {SpawnWave.SpawnWaveDefinition.SpawnsCountMultiplier}, rounded and obtained {SpawnWave.SpawnsCount}", CLogLevel.DETAILED);
		}
		ComputeSpawnPoints();
	}

	public SpawnWaveController(SerializedSpawnWaveContainer serializedSpawnWaveContainer, SpawnWaveView spawnWaveView, int saveVersion)
	{
		SpawnWave = new SpawnWave(serializedSpawnWaveContainer.CurrentSpawnWave, this, spawnWaveView, saveVersion);
		SpawnWave.SpawnWaveView.SpawnWave = SpawnWave;
		TPSingleton<SpawnWaveManager>.Instance.Log("Spawn wave: " + SpawnWave.SpawnWaveDefinition.Id, CLogLevel.MAJOR);
		BossManager.ResetPhases();
		if (SpawnWave.SpawnWaveDefinition.IsBossWave)
		{
			BossManager.Init(SpawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.BossWaveSettings.BossUnitTemplateId, setCurrentBossPhase: false);
		}
		SpawnWave.RotatedProportionPerDirection = RotateDirection(SpawnWave.SpawnDirectionsDefinition.SpawnDirectionsInfo, SpawnWave.RotationsCount);
		TPSingleton<SpawnWaveManager>.Instance.Log(string.Format("Spawn direction pattern: {0} rotated {1} {2} clockwise.", SpawnWave.SpawnDirectionsDefinition.Id, SpawnWave.RotationsCount, (SpawnWave.RotationsCount > 1) ? "times" : "time"));
		ComputeSpawnPoints(serializedSpawnWaveContainer.CurrentSpawnWave, saveVersion);
	}

	public int ComputeDistanceMaxFromCenterWithModifiers()
	{
		return (from y in SpawnWaveManager.SpawnDefinition.DistanceMaxFromCenterPerDays
			where y.Key <= TPSingleton<GameManager>.Instance.Game.DayNumber + 1
			orderby y.Key descending
			select y).FirstOrDefault().Value + TPSingleton<MetaUpgradesManager>.Instance.ComputeDistanceMaxFromCenterModifiers();
	}

	public void PopulateSpawnWave(bool isReroll = false)
	{
		GenerateTierEnemies(SpawnWave, isReroll);
		List<EnemyUnitTemplateDefinition> collection = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, SpawnWave.RemainingEnemiesToSpawn).ToList();
		SpawnWave.RemainingEnemiesToSpawn.Clear();
		SpawnWave.RemainingEnemiesToSpawn.AddRange(collection);
		List<EliteEnemyUnitTemplateDefinition> collection2 = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, SpawnWave.RemainingEliteEnemiesToSpawn).ToList();
		SpawnWave.RemainingEliteEnemiesToSpawn.Clear();
		SpawnWave.RemainingEliteEnemiesToSpawn.AddRange(collection2);
	}

	public Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> RotateDirection(Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> proportionPerDirection, int rotationsCount)
	{
		Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> dictionary = new Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer>(SpawnDirectionsDefinition.SharedDirectionComparer);
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value = null;
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value2 = null;
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value3 = null;
		SpawnDirectionsDefinition.SpawnDirectionInfoContainer value4 = null;
		switch (rotationsCount)
		{
		case 0:
		case 4:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value2);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value3);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value4);
			break;
		case 1:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value2);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value3);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value4);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value);
			break;
		case 2:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value3);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value4);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value2);
			break;
		case 3:
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Top, out value4);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Right, out value);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Bottom, out value2);
			proportionPerDirection.TryGetValue(SpawnDirectionsDefinition.E_Direction.Left, out value3);
			break;
		}
		if (value != null && value.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Top, value);
		}
		if (value2 != null && value2.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Right, value2);
		}
		if (value3 != null && value3.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Bottom, value3);
		}
		if (value4 != null && value4.Count > 0)
		{
			dictionary.Add(SpawnDirectionsDefinition.E_Direction.Left, value4);
		}
		return dictionary;
	}

	public IEnumerator SpawnEnemies()
	{
		if (SpawnWave.IsPaused)
		{
			yield break;
		}
		SpawnWave.CurrentCustomNightHour++;
		TPSingleton<SpawnWaveManager>.Instance.Log($"Start of spawn. Spawn wave remaining enemies : {SpawnWave.RemainingEnemiesToSpawn.Count} (+{SpawnWave.RemainingEliteEnemiesToSpawn.Count} elites) / Living enemies : {TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count} (elites included).");
		TPSingleton<SpawnWaveManager>.Instance.Log($"Current custom hour = {SpawnWave.CurrentCustomNightHour}, vs {TPSingleton<GameManager>.Instance.Game.CurrentNightHour}");
		if (!SpawnWave.SpawnWaveDefinition.TemporalDistribution.TryGetValue(SpawnWave.CurrentCustomNightHour, out var value) && SpawnWave.UnableToSpawnCount == 0)
		{
			yield break;
		}
		TPSingleton<SpawnWaveManager>.Instance.Log($"By definition for night hour {TPSingleton<GameManager>.Instance.Game.CurrentNightHour} of wave {SpawnWave.SpawnWaveDefinition.Id}, turnSpawnsCount is {value}.", CLogLevel.DETAILED);
		if (SpawnWave.SpawnsCount == 0)
		{
			SpawnWave.SpawnsCount = SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count;
			TPSingleton<SpawnWaveManager>.Instance.LogError($"ZERO enemies in the SpawnCount for the SWC! This is NOT correct. Making that number {SpawnWave.SpawnsCount}. This is likely due to a save corruption.\nPlease send your logs to a developer on discord", CLogLevel.DETAILED);
		}
		SpawnWave.SuccessfulSpawnCountThisTurn = 0;
		value = (float)SpawnWave.SpawnsCount * value / SpawnWave.SpawnWaveDefinition.TemporalDistributionTotalWeight;
		if (SpawnWave.UnableToSpawnCount > 0)
		{
			TPSingleton<SpawnWaveManager>.Instance.Log($"Adding {SpawnWave.UnableToSpawnCount} enemies that could not be spawned during the previous turn.", CLogLevel.MAJOR);
			value += (float)SpawnWave.UnableToSpawnCount;
			SpawnWave.UnableToSpawnCount = 0;
		}
		TPSingleton<SpawnWaveManager>.Instance.Log($"Trying to spawn {Mathf.RoundToInt(value)} enemies");
		if (value / (float)SpawnWave.SpawnsCount > BarkManager.ManyEnemyUnitSpawnPercentage)
		{
			for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num >= 0; num--)
			{
				TPSingleton<BarkManager>.Instance.AddPotentialBark("ManyEnemyUnitSpawn", TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num], BarkManager.DelayPostNewCycleAndEnemiesSpawn);
			}
			TPSingleton<BarkManager>.Instance.Display();
		}
		Dictionary<SpawnDirectionsDefinition.E_Direction, int> dictionary = new Dictionary<SpawnDirectionsDefinition.E_Direction, int>(SpawnDirectionsDefinition.SharedDirectionComparer);
		int num2 = Mathf.RoundToInt(value);
		float num3 = 0f;
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item in SpawnWave.RotatedProportionPerDirection)
		{
			num3 += (float)item.Value.TotalProportion;
		}
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item2 in SpawnWave.RotatedProportionPerDirection)
		{
			int num4 = Mathf.RoundToInt(value * (float)item2.Value.TotalProportion / num3);
			if (num4 > 0)
			{
				TPSingleton<SpawnWaveManager>.Instance.Log($"Calculated {value} * {item2.Value} / {num3} = {num4} spawns for direction {item2.Key}");
				dictionary.Add(item2.Key, num4);
				num2 -= num4;
			}
		}
		while (num2 > 0)
		{
			TPSingleton<SpawnWaveManager>.Instance.Log($"There are {num2} spawns remaining: spawn them at random direction.");
			float num5 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0f, num3);
			foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item3 in SpawnWave.RotatedProportionPerDirection)
			{
				num5 -= (float)item3.Value.TotalProportion;
				if (num5 < 0f)
				{
					if (dictionary.ContainsKey(item3.Key))
					{
						dictionary[item3.Key]++;
					}
					else
					{
						dictionary.Add(item3.Key, 1);
					}
					num2--;
					break;
				}
			}
		}
		if (SpawnWave.IsLastSpawnTurn)
		{
			int num6 = dictionary.Sum((KeyValuePair<SpawnDirectionsDefinition.E_Direction, int> kvp) => kvp.Value);
			int num7 = SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count - num6;
			if (num7 > 0)
			{
				if (dictionary.Count == 0)
				{
					int count = SpawnWave.RotatedProportionPerDirection.Count;
					int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, count);
					dictionary.Add(SpawnWave.RotatedProportionPerDirection.ElementAt(randomRange).Key, num7);
				}
				else
				{
					int randomRange2 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, dictionary.Count);
					dictionary[dictionary.ElementAt(randomRange2).Key] += num7;
				}
			}
		}
		TaskGroup spawnTasks = new TaskGroup();
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, int> item4 in dictionary)
		{
			Task task = new CoroutineTask(TPSingleton<NightTurnsManager>.Instance, SpawnEnemiesFromDirection(item4.Value, item4.Key));
			spawnTasks.AddTask(task);
		}
		spawnTasks.OnCompleteAction = delegate
		{
			spawnTasks = null;
		};
		spawnTasks.Run();
		yield return new WaitUntil(() => spawnTasks == null);
		TPSingleton<SpawnWaveManager>.Instance.Log($"End of spawn. Spawn wave remaining enemies : {SpawnWave.RemainingEnemiesToSpawn.Count} (+{SpawnWave.RemainingEliteEnemiesToSpawn.Count}elites) / Living enemies : {TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count}.");
	}

	public static int ComputeCountWithExternalModifiers(SpawnWave spawnWave, SpawnDefinition spawnDefinition)
	{
		return ComputeCountWithExternalModifiers(Mathf.RoundToInt(spawnDefinition.SpawnsCountPerWave.EvalToFloat(TPSingleton<SpawnWaveManager>.Instance.SpawnWaveInterpreterObject) * spawnWave.SpawnWaveDefinition.SpawnsCountMultiplier));
	}

	private static int ComputeCountWithExternalModifiers(int originalCount)
	{
		float currentWavePercentageModifier = SpawnWaveManager.CurrentWavePercentageModifier;
		return originalCount + Mathf.RoundToInt((float)originalCount * currentWavePercentageModifier * 0.01f);
	}

	public void RecomputeSpawnPoints()
	{
		SpawnWave.SpawnPointsInfo.Clear();
		ComputeSpawnPoints();
	}

	private void ComputeSpawnPoints()
	{
		foreach (KeyValuePair<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> item in SpawnWave.RotatedProportionPerDirection)
		{
			if (item.Value == null)
			{
				continue;
			}
			foreach (SpawnDirectionsDefinition.SpawnDirectionInfo item2 in item.Value)
			{
				if (item2.Proportion > 0)
				{
					ComputeSpawnPoints(item.Key, item2);
				}
			}
		}
	}

	private void ComputeSpawnPoints(SerializedSpawnWave serializedSpawnWave, int saveVersion)
	{
		for (int num = serializedSpawnWave.SpawnPointsInfo.Count - 1; num >= 0; num--)
		{
			SpawnPointInfo item = new SpawnPointInfo(serializedSpawnWave.SpawnPointsInfo[num], saveVersion);
			SpawnWave.SpawnPointsInfo.Add(item);
		}
	}

	private List<Tile> ComputeSpawnPoint(Tile baseTile, SpawnDirectionsDefinition.E_Direction spawnDirectionDefinition)
	{
		List<Tile> list = new List<Tile> { baseTile };
		Vector2Int v = SpawnWaveManager.SpawnDefinition.SpawnPointRect;
		if (spawnDirectionDefinition == SpawnDirectionsDefinition.E_Direction.Left || spawnDirectionDefinition == SpawnDirectionsDefinition.E_Direction.Right)
		{
			v = v.Swap();
		}
		List<Tile> list2 = TileMapController.GetTilesInRect(new RectInt(baseTile.Position.x - v.x, baseTile.Position.y - v.y, v.x * 2, v.y * 2)).ToList();
		for (int num = list2.Count - 1; num >= 0; num--)
		{
			Tile tile = list2[num];
			if (tile == baseTile || !tile.HasFog || !tile.IsCrossable || tile.Unit != null || tile.Building != null)
			{
				list2.RemoveAt(num);
			}
		}
		list2 = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, list2).ToList();
		for (int i = 0; i < Mathf.Min(list2.Count, SpawnWaveManager.SpawnDefinition.SpawnPointsPerGroup - 1); i++)
		{
			list.Add(list2[i]);
		}
		return list;
	}

	private void ComputeSpawnPoints(SpawnDirectionsDefinition.E_Direction direction, SpawnDirectionsDefinition.SpawnDirectionInfo spawnDirectionInfo)
	{
		SpawnPointInfo spawnPointInfo = new SpawnPointInfo(direction, spawnDirectionInfo);
		SpawnWave.SpawnPointsInfo.Add(spawnPointInfo);
		List<Tile> list = new List<Tile>();
		Tile centerTile = TileMapController.GetCenterTile();
		int num = ComputeDistanceMaxFromCenterWithModifiers();
		int num2;
		int value;
		int num3;
		int value2;
		switch (direction)
		{
		case SpawnDirectionsDefinition.E_Direction.Bottom:
			num2 = centerTile.X - num;
			value = centerTile.X + num;
			num3 = centerTile.Y - TPSingleton<FogManager>.Instance.Fog.DensityValue;
			value2 = num3;
			break;
		case SpawnDirectionsDefinition.E_Direction.Top:
			num2 = centerTile.X - num;
			value = centerTile.X + num;
			num3 = centerTile.Y + TPSingleton<FogManager>.Instance.Fog.DensityValue;
			value2 = num3;
			break;
		case SpawnDirectionsDefinition.E_Direction.Left:
			num2 = centerTile.X - TPSingleton<FogManager>.Instance.Fog.DensityValue;
			value = num2;
			num3 = centerTile.Y - num;
			value2 = centerTile.Y + num;
			break;
		case SpawnDirectionsDefinition.E_Direction.Right:
			num2 = centerTile.X + TPSingleton<FogManager>.Instance.Fog.DensityValue;
			value = num2;
			num3 = centerTile.Y - num;
			value2 = centerTile.Y + num;
			break;
		default:
			TPSingleton<SpawnWaveManager>.Instance.LogError("Unexpected direction in ComputeSpawnPoints : " + direction);
			return;
		}
		num2 = Mathf.Clamp(num2, 0, TPSingleton<TileMapManager>.Instance.TileMap.Width);
		value = Mathf.Clamp(value, 0, TPSingleton<TileMapManager>.Instance.TileMap.Width);
		num3 = Mathf.Clamp(num3, 0, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		value2 = Mathf.Clamp(value2, 0, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		if (!spawnDirectionInfo.EquidistantSpawnPoints)
		{
			for (int i = num2; i <= value; i++)
			{
				for (int j = num3; j <= value2; j++)
				{
					Tile tile = TileMapManager.GetTile(i, j);
					if (tile != null && tile.Unit == null)
					{
						TheLastStand.Model.Building.Building building = tile.Building;
						if ((building == null || !building.IsObstacle) && tile.IsCrossable)
						{
							list.Add(tile);
						}
					}
				}
			}
		}
		else
		{
			TPSingleton<SpawnWaveManager>.Instance.Log($"Getting equidistant tiles on rotated direction {direction} in spawn wave {SpawnWave.SpawnDirectionsDefinition.Id} generation.", CLogLevel.DETAILED);
			if (direction == SpawnDirectionsDefinition.E_Direction.Top || direction == SpawnDirectionsDefinition.E_Direction.Bottom)
			{
				num2 += spawnDirectionInfo.EquidistanceBordersMargin;
				value -= spawnDirectionInfo.EquidistanceBordersMargin;
			}
			else
			{
				num3 += spawnDirectionInfo.EquidistanceBordersMargin;
				value2 -= spawnDirectionInfo.EquidistanceBordersMargin;
			}
			foreach (Tile item2 in TileMapController.GetEquidistantTilesOnSegment(TileMapManager.GetTile(num2, num3), TileMapManager.GetTile(value, value2), spawnDirectionInfo.SpawnPointsPerDirectionCount).ToList())
			{
				Tile closestTileFillingConditions = TileMapManager.GetClosestTileFillingConditions(10, item2, (Tile o) => o.Unit == null && (o.Building == null || !o.Building.IsObstacle) && o.IsCrossable && !o.HasFog);
				if (closestTileFillingConditions != null)
				{
					list.Add(closestTileFillingConditions);
				}
			}
		}
		if (list.Count == 0)
		{
			TPSingleton<SpawnWaveManager>.Instance.LogError($"No valid tile found for spawn point! Is using EquidistantSpawnPoints: {spawnDirectionInfo.EquidistantSpawnPoints}.", CLogLevel.MAJOR);
			return;
		}
		list = RandomManager.Shuffle(TPSingleton<SpawnWaveManager>.Instance, list).ToList();
		int num4 = list.Count - 1;
		while (num4 >= 0)
		{
			spawnPointInfo.SpawnPointsBases.Add(list[num4]);
			List<Tile> item = ComputeSpawnPoint(list[num4], direction);
			spawnPointInfo.SpawnPoints.Add(item);
			if (spawnPointInfo.SpawnPoints.Count < spawnDirectionInfo.SpawnPointsPerDirectionCount)
			{
				num4--;
				continue;
			}
			break;
		}
	}

	public static void GenerateElites(SpawnWave spawnWave, out float elitesGenerationTotalExperience)
	{
		elitesGenerationTotalExperience = 0f;
		List<int> list = new List<int>();
		foreach (KeyValuePair<string, int> eliteEnemyUnitTemplateDefinition2 in spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EliteEnemyUnitTemplateDefinitions)
		{
			if (!EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions.TryGetValue(eliteEnemyUnitTemplateDefinition2.Key, out var eliteDefinition))
			{
				TPSingleton<SpawnWaveManager>.Instance.LogError("Elite Enemy template id " + eliteEnemyUnitTemplateDefinition2.Key + " not found in database. Abort spawn.");
				continue;
			}
			if (EnemyUnitDatabase.UnintegratedElites.Contains(eliteDefinition.EliteId))
			{
				while (list.Count < eliteDefinition.Tier)
				{
					list.Add(0);
				}
				list[eliteDefinition.Tier - 1] += eliteEnemyUnitTemplateDefinition2.Value;
				continue;
			}
			int num = -1;
			int num2 = 0;
			while (++num < spawnWave.RemainingEnemiesToSpawn.Count && num2 < eliteEnemyUnitTemplateDefinition2.Value)
			{
				if (spawnWave.RemainingEnemiesToSpawn[num].Id == eliteDefinition.Id)
				{
					num2++;
				}
			}
			int num3 = eliteEnemyUnitTemplateDefinition2.Value - num2;
			if (num3 > 0)
			{
				TPSingleton<SpawnWaveManager>.Instance.LogWarning($"Tried to spawn {eliteEnemyUnitTemplateDefinition2.Value} {eliteEnemyUnitTemplateDefinition2.Key} but only found {num2} suitable enemies to replace ({eliteDefinition.Id}).");
				while (list.Count < eliteDefinition.Tier)
				{
					list.Add(0);
				}
				list[eliteDefinition.Tier - 1] += num3;
			}
			while (num2-- > 0)
			{
				EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = spawnWave.RemainingEnemiesToSpawn.First((EnemyUnitTemplateDefinition enemyDefinition) => enemyDefinition.Id == eliteDefinition.Id);
				spawnWave.RemainingEnemiesToSpawn.Remove(enemyUnitTemplateDefinition);
				spawnWave.RemainingEliteEnemiesToSpawn.Add(eliteDefinition);
				elitesGenerationTotalExperience -= GetEnemyExperience(enemyUnitTemplateDefinition);
				elitesGenerationTotalExperience += GetEnemyExperience(eliteDefinition);
			}
		}
		if (!spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EliteOverrideSpawnDefinition)
		{
			foreach (KeyValuePair<int, Node> item in (SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions.Count >= TPSingleton<GameManager>.Instance.Game.DayNumber + 1) ? SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions[TPSingleton<GameManager>.Instance.Game.DayNumber] : SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions[SpawnWaveManager.SpawnDefinition.ElitesPerDayDefinitions.Count - 1])
			{
				while (list.Count < item.Key)
				{
					list.Add(0);
				}
				list[item.Key - 1] += item.Value.EvalToInt(spawnWave.Interpreter);
			}
		}
		if (list.Count > 0)
		{
			Dictionary<int, List<EnemyUnitTemplateDefinition>> dictionary = new Dictionary<int, List<EnemyUnitTemplateDefinition>>();
			foreach (EnemyUnitTemplateDefinition item2 in spawnWave.RemainingEnemiesToSpawn)
			{
				if (dictionary.ContainsKey(item2.Tier - 1))
				{
					dictionary[item2.Tier - 1].Add(item2);
					continue;
				}
				dictionary.Add(item2.Tier - 1, new List<EnemyUnitTemplateDefinition> { item2 });
			}
			for (int num4 = list.Count - 1; num4 >= 0; num4--)
			{
				int num5 = list[num4];
				if (dictionary.ContainsKey(num4))
				{
					while (num5 > 0 && dictionary[num4].Count > 0)
					{
						int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, dictionary[num4].Count);
						if (EnemyUnitDatabase.EnemyToEliteIds.TryGetValue(dictionary[num4][randomRange].Id, out var value) && !EnemyUnitDatabase.UnintegratedElites.Contains(value))
						{
							num5--;
							EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition = EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions[value];
							EnemyUnitTemplateDefinition enemyUnitTemplateDefinition2 = dictionary[num4][randomRange];
							elitesGenerationTotalExperience += GetEnemyExperience(eliteEnemyUnitTemplateDefinition);
							elitesGenerationTotalExperience -= GetEnemyExperience(enemyUnitTemplateDefinition2);
							spawnWave.RemainingEliteEnemiesToSpawn.Add(eliteEnemyUnitTemplateDefinition);
							spawnWave.RemainingEnemiesToSpawn.Remove(enemyUnitTemplateDefinition2);
						}
						dictionary[num4].RemoveAt(randomRange);
					}
				}
				if (num5 > 0)
				{
					if (num4 > 0)
					{
						TPSingleton<SpawnWaveManager>.Instance.LogWarning($"{num5} elites could not be spawned on tier {num4 + 1}. Adding them to the lower tier.", CLogLevel.MAJOR);
						list[num4 - 1] += num5;
					}
					else
					{
						TPSingleton<SpawnWaveManager>.Instance.LogError($"{num5} elites could not be spawned at all.", CLogLevel.MAJOR);
					}
				}
			}
		}
		TPSingleton<SpawnWaveManager>.Instance.Log($"{spawnWave.RemainingEliteEnemiesToSpawn.Count} enemies have been tranformed into elites.");
	}

	public static void GenerateTierEnemies(SpawnWave spawnWave, bool isReroll = false)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EnemyUnitTemplateDefinitions);
		List<string> list = TPSingleton<MetaUpgradesManager>.Instance.GetUnavailableEnemiesIds();
		if (SpawnWaveManager.SpawnDefinition.DisallowedEnemies != null)
		{
			list.AddRange(SpawnWaveManager.SpawnDefinition.DisallowedEnemies);
			list = list.Distinct().ToList();
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			if (dictionary.ContainsKey(list[num]))
			{
				TPSingleton<SpawnWaveManager>.Instance.Log("Removed " + list[num] + " from wave generation pool because it's blocked by a meta upgrade.");
				dictionary.Remove(list[num]);
			}
		}
		Dictionary<int, List<EnemyUnitTemplateDefinition>> enemyUnitTemplatesByTierDefinitionsCopy = EnemyUnitDatabase.GetEnemyUnitTemplatesByTierDefinitionsCopy();
		foreach (List<EnemyUnitTemplateDefinition> value in enemyUnitTemplatesByTierDefinitionsCopy.Values)
		{
			foreach (string key in dictionary.Keys)
			{
				for (int num2 = value.Count - 1; num2 >= 0; num2--)
				{
					if (key == value[num2].Id)
					{
						value.Remove(value[num2]);
						break;
					}
				}
			}
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				for (int num4 = value.Count - 1; num4 >= 0; num4--)
				{
					if (list[num3] == value[num4].Id)
					{
						TPSingleton<SpawnWaveManager>.Instance.Log("Removed " + list[num3] + " from wave generation pool because it's blocked by a meta upgrade.");
						value.Remove(value[num4]);
						break;
					}
				}
			}
			if (!isReroll)
			{
				continue;
			}
			foreach (EnemyUnitTemplateDefinition item in SpawnWaveManager.RerolledSpawnWave.EnemiesToSpawn)
			{
				for (int num5 = value.Count - 1; num5 >= 0; num5--)
				{
					if (item.Id == value[num5].Id && value.Count > spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EnemyTierDefinitions.Count)
					{
						TPSingleton<SpawnWaveManager>.Instance.Log("removing " + item.Id + " from wave generation pool because it was present in previously rerolled wave", CLogLevel.DETAILED);
						value.Remove(value[num5]);
						break;
					}
				}
			}
		}
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		bool flag = ApocalypseManager.CurrentApocalypse.EnemiesSpawnWaveWeightMultiplier.Count > 0;
		if (flag)
		{
			foreach (KeyValuePair<string, int> item2 in dictionary)
			{
				dictionary2.Add(item2.Key, item2.Value);
			}
		}
		foreach (string item3 in new List<string>(dictionary.Keys))
		{
			if (ApocalypseManager.CurrentApocalypse.EnemiesSpawnWaveWeightMultiplier.ContainsKey(item3))
			{
				dictionary[item3] = Mathf.RoundToInt((float)dictionary[item3] * ApocalypseManager.CurrentApocalypse.EnemiesSpawnWaveWeightMultiplier[item3]);
			}
		}
		string text = "<b>Added enemies by tier :</b>\n";
		foreach (Tuple<int, float> enemyTierDefinition in spawnWave.SpawnWaveDefinition.WaveEnemiesDefinition.EnemyTierDefinitions)
		{
			int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, enemyUnitTemplatesByTierDefinitionsCopy[enemyTierDefinition.Item1].Count);
			EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = enemyUnitTemplatesByTierDefinitionsCopy[enemyTierDefinition.Item1][randomRange];
			float num6 = 1f;
			if (ApocalypseManager.CurrentApocalypse.EnemiesSpawnWaveWeightMultiplier.ContainsKey(enemyUnitTemplateDefinition.Id))
			{
				num6 = ApocalypseManager.CurrentApocalypse.EnemiesSpawnWaveWeightMultiplier[enemyUnitTemplateDefinition.Id];
			}
			dictionary.Add(enemyUnitTemplateDefinition.Id, Mathf.RoundToInt((float)enemyUnitTemplateDefinition.Weight * num6 * enemyTierDefinition.Item2));
			if (flag)
			{
				dictionary2.Add(enemyUnitTemplateDefinition.Id, Mathf.RoundToInt((float)enemyUnitTemplateDefinition.Weight * enemyTierDefinition.Item2));
			}
			enemyUnitTemplatesByTierDefinitionsCopy[enemyTierDefinition.Item1].Remove(enemyUnitTemplateDefinition);
			text = text + enemyUnitTemplateDefinition.Id + "\n";
		}
		TPSingleton<SpawnWaveManager>.Instance.Log(text ?? "", CLogLevel.DETAILED);
		CreateEnemyUnitsToSpawn(spawnWave, dictionary, out var totalExperience);
		GenerateElites(spawnWave, out var elitesGenerationTotalExperience);
		spawnWave.EnemyWeightModifierXpRatio = 1f;
		string text2 = string.Empty;
		if (flag)
		{
			CreateEnemyUnitsToSpawn(spawnWave, dictionary2, out var totalExperience2, addToSpawnWave: false);
			spawnWave.EnemyWeightModifierXpRatio = (totalExperience2 + elitesGenerationTotalExperience) / (totalExperience + elitesGenerationTotalExperience);
			text2 = $", totalExperienceBeforeModifiers: {totalExperience2}";
		}
		TPSingleton<SpawnWaveManager>.Instance.Log($"Spawn wave generation, EnemyWeightModifierXpRatio:{spawnWave.EnemyWeightModifierXpRatio}, elitesTotalExperience: {elitesGenerationTotalExperience}, totalExperience: {totalExperience}{text2}", CLogLevel.MAJOR, forcePrintInUnity: true);
	}

	private static void CreateEnemyUnitsToSpawn(SpawnWave spawnWave, Dictionary<string, int> enemyUnitIdsWithWeights, out float totalExperience, bool addToSpawnWave = true)
	{
		totalExperience = 0f;
		float num = 0f;
		foreach (int value2 in enemyUnitIdsWithWeights.Values)
		{
			num += (float)value2;
		}
		for (int i = 0; i < spawnWave.SpawnsCount; i++)
		{
			string text = string.Empty;
			float num2 = 0f;
			foreach (KeyValuePair<string, int> enemyUnitIdsWithWeight in enemyUnitIdsWithWeights)
			{
				num2 += (float)(spawnWave.SpawnsCount * enemyUnitIdsWithWeight.Value) / num;
				if ((float)i < num2)
				{
					text = enemyUnitIdsWithWeight.Key;
					break;
				}
			}
			if (!(text != string.Empty))
			{
				continue;
			}
			if (EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(text, out var value))
			{
				totalExperience += GetEnemyExperience(value);
				if (addToSpawnWave)
				{
					spawnWave.RemainingEnemiesToSpawn.Add(value);
				}
			}
			else if (addToSpawnWave)
			{
				TPSingleton<SpawnWaveManager>.Instance.LogError("Enemy template id " + text + " not found in database");
			}
		}
	}

	private static float GetEnemyExperience(EnemyUnitTemplateDefinition enemyUnitTemplateDefinition)
	{
		return enemyUnitTemplateDefinition.ExperienceGain.Min;
	}

	private static float GetEnemyExperience(EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition)
	{
		float num = eliteEnemyUnitTemplateDefinition.ExperienceGain.Min;
		if (eliteEnemyUnitTemplateDefinition.ModifiedStats.TryGetValue(UnitStatDefinition.E_Stat.ExperienceGain, out var value))
		{
			num = EnemyUnitStat.GetValueWithStatModifier(num, value.PercentageModifier, value.FlatModifier);
		}
		return num;
	}

	private void GetSpawnableTile(Tile spawnPoint, Tile currentTile, Tile centerTile, int maxDistanceFromSpawnPoint, List<Tile> processedTiles, ref Tile[] tiles, ref Tile targetTile, UnitTemplateDefinition unitTemplateDefinition)
	{
		processedTiles.Add(currentTile);
		if (TileMapController.DistanceBetweenTiles(spawnPoint, currentTile) > maxDistanceFromSpawnPoint)
		{
			return;
		}
		if ((Mathf.Abs(currentTile.X - centerTile.X) > TPSingleton<FogManager>.Instance.Fog.DensityValue || Mathf.Abs(currentTile.Y - centerTile.Y) > TPSingleton<FogManager>.Instance.Fog.DensityValue) && unitTemplateDefinition.CanSpawnOn(currentTile))
		{
			targetTile = currentTile;
			return;
		}
		Vector2Int vector2Int = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, 4) switch
		{
			1 => new Vector2Int(1, -1), 
			2 => new Vector2Int(-1, 1), 
			3 => new Vector2Int(-1, -1), 
			_ => new Vector2Int(1, 1), 
		};
		for (int num = currentTile.X - vector2Int.x; num >= currentTile.X + vector2Int.x; num--)
		{
			for (int num2 = currentTile.Y - vector2Int.x; num2 >= currentTile.Y + vector2Int.y; num2--)
			{
				GetSpawnableTile(num, num2, spawnPoint, currentTile, centerTile, maxDistanceFromSpawnPoint, processedTiles, ref tiles, ref targetTile, unitTemplateDefinition);
				if (targetTile != null)
				{
					return;
				}
			}
		}
	}

	private void GetSpawnableTile(int x, int y, Tile spawnPoint, Tile currentTile, Tile centerTile, int maxDistanceFromSpawnPoint, List<Tile> processedTiles, ref Tile[] tiles, ref Tile targetTile, UnitTemplateDefinition unitTemplateDefinition)
	{
		if ((x != currentTile.X || y != currentTile.Y) && (x == currentTile.X || y == currentTile.Y) && x >= 0 && x < TPSingleton<TileMapManager>.Instance.TileMap.Width && y >= 0 && y < TPSingleton<TileMapManager>.Instance.TileMap.Height && tiles[x * TPSingleton<TileMapManager>.Instance.TileMap.Height + y] != null && !processedTiles.Contains(tiles[x * TPSingleton<TileMapManager>.Instance.TileMap.Height + y]))
		{
			GetSpawnableTile(spawnPoint, tiles[x * TPSingleton<TileMapManager>.Instance.TileMap.Height + y], centerTile, maxDistanceFromSpawnPoint, processedTiles, ref tiles, ref targetTile, unitTemplateDefinition);
		}
	}

	private IEnumerator SpawnEnemiesFromDirection(int spawnsCount, SpawnDirectionsDefinition.E_Direction direction)
	{
		string debugString = $"---- Spawn enemies from {direction} ----";
		List<SpawnPointInfo> spawnPointInfoForDirection = SpawnWave.SpawnPointsInfo.Where((SpawnPointInfo x) => x.Direction == direction).ToList();
		float num = spawnPointInfoForDirection.Sum((SpawnPointInfo x) => x.SpawnDirectionInfo.Proportion);
		int[] spawnPointInfoIndexThreshold = new int[spawnPointInfoForDirection.Count];
		int num2 = 0;
		int previousIndexReached = 0;
		for (int num3 = 0; num3 < spawnPointInfoForDirection.Count; num3++)
		{
			float num4 = (float)spawnPointInfoForDirection[num3].SpawnDirectionInfo.Proportion / num;
			spawnPointInfoIndexThreshold[num3] = num2 + Mathf.RoundToInt((float)spawnsCount * num4);
			num2 = spawnPointInfoIndexThreshold[num3];
		}
		int spawnIndex = 0;
		while (spawnIndex < spawnsCount)
		{
			debugString += $"\nSpawn #{spawnIndex} : ";
			int index = previousIndexReached;
			for (int num5 = previousIndexReached; num5 < spawnPointInfoIndexThreshold.Length; num5++)
			{
				if (spawnPointInfoIndexThreshold[num5] > spawnIndex)
				{
					previousIndexReached = num5;
					index = num5;
					break;
				}
			}
			SpawnPointInfo spawnPointInfo = spawnPointInfoForDirection[index];
			if (SpawnWave.RemainingEnemiesToSpawn.Count == 0 && SpawnWave.RemainingEliteEnemiesToSpawn.Count == 0)
			{
				debugString += "\nNo spawn remaining in the wave.";
				break;
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, SpawnWave.RemainingEnemiesToSpawn.Count + SpawnWave.RemainingEliteEnemiesToSpawn.Count);
			EnemyUnitTemplateDefinition enemyUnitTemplateDefinition = ((randomRange < SpawnWave.RemainingEnemiesToSpawn.Count) ? SpawnWave.RemainingEnemiesToSpawn[randomRange] : SpawnWave.RemainingEliteEnemiesToSpawn[randomRange - SpawnWave.RemainingEnemiesToSpawn.Count]);
			Tile[] tiles = TPSingleton<TileMapManager>.Instance.TileMap.Tiles;
			int randomRange2 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, spawnPointInfo.SpawnPoints.Count);
			List<Tile> list = spawnPointInfo.SpawnPoints[randomRange2];
			int randomRange3 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, list.Count);
			Tile tile = list[randomRange3];
			Tile centerTile = TileMapController.GetCenterTile();
			Tile targetTile = null;
			int num6 = 0;
			int num7 = 0;
			do
			{
				List<Tile> processedTiles = new List<Tile>();
				GetSpawnableTile(tile, tile, centerTile, num6++, processedTiles, ref tiles, ref targetTile, enemyUnitTemplateDefinition);
				num7++;
			}
			while (targetTile == null && num7 < 20);
			if (targetTile == null)
			{
				TPSingleton<SpawnWaveManager>.Instance.Log("No valid tile found to spawn an enemy, keeping him aside for next turn.", CLogLevel.MAJOR, forcePrintInUnity: true);
				SpawnWave.UnableToSpawnCount++;
			}
			else
			{
				if (enemyUnitTemplateDefinition is EliteEnemyUnitTemplateDefinition eliteEnemyUnitTemplateDefinition)
				{
					debugString += $"Selected tile {tile} (index {randomRange3}) for selected Elite Enemy Id {eliteEnemyUnitTemplateDefinition.EliteId}.";
					EnemyUnitManager.CreateEliteEnemyUnit(eliteEnemyUnitTemplateDefinition, targetTile, new UnitCreationSettings());
					SpawnWave.RemainingEliteEnemiesToSpawn.Remove(eliteEnemyUnitTemplateDefinition);
				}
				else
				{
					debugString += $"Selected tile {tile} (index {randomRange3}) for selected Enemy Id {enemyUnitTemplateDefinition.Id}.";
					EnemyUnitManager.CreateEnemyUnit(enemyUnitTemplateDefinition, targetTile, new UnitCreationSettings());
					SpawnWave.RemainingEnemiesToSpawn.Remove(enemyUnitTemplateDefinition);
				}
				SpawnWave.SuccessfulSpawnCountThisTurn++;
				yield return null;
			}
			int num8 = spawnIndex + 1;
			spawnIndex = num8;
		}
		TPSingleton<SpawnWaveManager>.Instance.Log(debugString, CLogLevel.DETAILED);
	}
}
