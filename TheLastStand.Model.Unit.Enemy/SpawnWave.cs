using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.TileMap;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.SpawnWave;
using TheLastStand.View.Unit;

namespace TheLastStand.Model.Unit.Enemy;

public class SpawnWave : ISerializable, IDeserializable
{
	public bool IsPaused;

	public readonly FormulaInterpreterContext Interpreter = new FormulaInterpreterContext();

	public readonly List<EliteEnemyUnitTemplateDefinition> RemainingEliteEnemiesToSpawn = new List<EliteEnemyUnitTemplateDefinition>();

	public readonly List<EnemyUnitTemplateDefinition> RemainingEnemiesToSpawn = new List<EnemyUnitTemplateDefinition>();

	public readonly List<SpawnPointInfo> SpawnPointsInfo = new List<SpawnPointInfo>();

	public readonly SpawnWaveController SpawnWaveController;

	public readonly SpawnWaveView SpawnWaveView;

	public int CurrentCustomNightHour { get; set; }

	public bool IsLastSpawnTurn
	{
		get
		{
			foreach (KeyValuePair<int, float> item in SpawnWaveDefinition.TemporalDistribution)
			{
				if (item.Key > CurrentCustomNightHour)
				{
					return false;
				}
			}
			return true;
		}
	}

	public Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> RotatedProportionPerDirection { get; set; }

	public int RotationsCount { get; set; }

	public SpawnDirectionsDefinition SpawnDirectionsDefinition { get; set; }

	public int SpawnsCount { get; set; }

	public SpawnWaveDefinition SpawnWaveDefinition { get; private set; }

	public bool SpawnMightBeStuck
	{
		get
		{
			if (IsLastSpawnTurn && SuccessfulSpawnCountThisTurn == 0 && (!SpawnWaveDefinition.IsBossWave || TPSingleton<BossManager>.Instance.VictoryConditionIsToFinishWave))
			{
				return TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits;
			}
			return false;
		}
	}

	public int SuccessfulSpawnCountThisTurn { get; set; }

	public int UnableToSpawnCount { get; set; }

	public float EnemyWeightModifierXpRatio { get; set; } = 1f;

	public SpawnWave(SpawnWaveDefinition waveDefinition, SpawnWaveController spawnWaveController, SpawnWaveView spawnWaveView)
	{
		SpawnWaveDefinition = waveDefinition;
		SpawnWaveController = spawnWaveController;
		SpawnWaveView = spawnWaveView;
	}

	public static string GetLocalizedDirectionName(SpawnDirectionsDefinition.E_Direction direction)
	{
		return Localizer.Get(string.Format("{0}{1}", "SpawnWave_DirectionName_", direction));
	}

	public int GetSpawnPointCountInZone(TileFlagDefinition.E_TileFlagTag zoneTileFlag, SpawnDirectionsDefinition.E_Direction spawnDirection)
	{
		int num = 0;
		foreach (SpawnPointInfo item in SpawnPointsInfo)
		{
			if (item.Direction != spawnDirection)
			{
				continue;
			}
			foreach (List<Tile> spawnPoint in item.SpawnPoints)
			{
				num += spawnPoint.Count((Tile t) => t.HasTileFlagTag(zoneTileFlag));
			}
		}
		return num;
	}

	public float GetSpawnPointPercentageInZone(TileFlagDefinition.E_TileFlagTag zoneTileFlag, SpawnDirectionsDefinition.E_Direction spawnDirection)
	{
		float num = 0f;
		float num2 = 0f;
		foreach (SpawnPointInfo item in SpawnPointsInfo)
		{
			if (item.Direction != spawnDirection)
			{
				continue;
			}
			foreach (List<Tile> spawnPoint in item.SpawnPoints)
			{
				num2 += (float)spawnPoint.Count;
				num += (float)spawnPoint.Count((Tile t) => t.HasTileFlagTag(zoneTileFlag));
			}
		}
		if (num2 == 0f)
		{
			return 0f;
		}
		return num / num2;
	}

	public SpawnWave(SerializedSpawnWave serializedSpawnWave, SpawnWaveController spawnWaveController, SpawnWaveView spawnWaveView, int saveVersion)
	{
		SpawnWaveController = spawnWaveController;
		SpawnWaveView = spawnWaveView;
		Deserialize(serializedSpawnWave, saveVersion);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedSpawnWave serializedSpawnWave = container as SerializedSpawnWave;
		SpawnWaveDefinition = SpawnWaveManager.TryGetSpawnWaveDefinitionOrRandom(serializedSpawnWave.SpawnWaveDefinitionId, out var success);
		IsPaused = serializedSpawnWave.IsPaused;
		CurrentCustomNightHour = serializedSpawnWave.CurrentCustomNightHour;
		EnemyWeightModifierXpRatio = serializedSpawnWave.EnemyWeightModifierXpRatio;
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night || success)
		{
			SpawnsCount = serializedSpawnWave.SpawnsCount;
			try
			{
				foreach (string remainingEliteEnemiesToSpawnId in serializedSpawnWave.RemainingEliteEnemiesToSpawnIds)
				{
					RemainingEliteEnemiesToSpawn.Add(EnemyUnitDatabase.EliteEnemyUnitTemplateDefinitions[remainingEliteEnemiesToSpawnId]);
				}
				foreach (string remainingEnemiesToSpawnId in serializedSpawnWave.RemainingEnemiesToSpawnIds)
				{
					RemainingEnemiesToSpawn.Add(EnemyUnitDatabase.EnemyUnitTemplateDefinitions[remainingEnemiesToSpawnId]);
				}
			}
			catch (Exception arg)
			{
				TPSingleton<SpawnWaveManager>.Instance.LogError($"Exception caught while trying to load enemy definitions for SpawnWave, trying to regenerate entire spawn wave. Exception:\n{arg}");
				RemainingEliteEnemiesToSpawn.Clear();
				RemainingEnemiesToSpawn.Clear();
				SpawnWaveController.GenerateTierEnemies(this);
			}
		}
		else
		{
			SpawnsCount = SpawnWaveController.ComputeCountWithExternalModifiers(this, SpawnWaveManager.SpawnDefinition);
			SpawnWaveController.GenerateTierEnemies(this);
		}
		RotationsCount = serializedSpawnWave.RotationsCount;
		SpawnDirectionsDefinition = SpawnWaveDatabase.SpawnDirectionDefinitions[serializedSpawnWave.SpawnDirectionsDefinitionId];
		UnableToSpawnCount = serializedSpawnWave.UnableToSpawnCount;
	}

	public ISerializedData Serialize()
	{
		return new SerializedSpawnWave
		{
			SpawnWaveDefinitionId = SpawnWaveDefinition.Id,
			IsPaused = IsPaused,
			CurrentCustomNightHour = CurrentCustomNightHour,
			RemainingEliteEnemiesToSpawnIds = RemainingEliteEnemiesToSpawn.Select((EliteEnemyUnitTemplateDefinition u) => u.EliteId).ToList(),
			RemainingEnemiesToSpawnIds = RemainingEnemiesToSpawn.Select((EnemyUnitTemplateDefinition u) => u.Id).ToList(),
			RotationsCount = RotationsCount,
			SpawnDirectionsDefinitionId = SpawnDirectionsDefinition.Id,
			SpawnPointsInfo = SpawnPointsInfo.Select((SpawnPointInfo x) => x.Serialize() as SerializedSpawnPointInfo).ToList(),
			SpawnsCount = SpawnsCount,
			UnableToSpawnCount = UnableToSpawnCount,
			EnemyWeightModifierXpRatio = EnemyWeightModifierXpRatio
		};
	}
}
