using System.Collections.Generic;
using System.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Serialization.SpawnWave;

namespace TheLastStand.Model.Unit.Enemy;

public class SpawnWaveInfos
{
	public List<EnemyUnitTemplateDefinition> EnemiesToSpawn;

	public int RotationsCount;

	public string SpawnDirectionDefinitionId;

	public string SpawnWaveDefinitionId;

	public SpawnWaveInfos(SpawnWave spawnWave)
	{
		RotationsCount = spawnWave.RotationsCount;
		EnemiesToSpawn = spawnWave.RemainingEnemiesToSpawn.Concat(spawnWave.RemainingEliteEnemiesToSpawn).ToList();
		SpawnDirectionDefinitionId = spawnWave.SpawnDirectionsDefinition.Id;
		SpawnWaveDefinitionId = spawnWave.SpawnWaveDefinition.Id;
	}

	public SpawnWaveInfos(SerializedSpawnWaveContainer.RerolledSpawnWave rerolledSpawnWave)
	{
		RotationsCount = rerolledSpawnWave.RotationsCount;
		SpawnWaveDefinitionId = rerolledSpawnWave.SpawnWaveDefinitionId;
		SpawnDirectionDefinitionId = rerolledSpawnWave.SpawnDirectionDefinitionId;
		EnemiesToSpawn = new List<EnemyUnitTemplateDefinition>();
		rerolledSpawnWave.EnemiesId.All(delegate(string x)
		{
			EnemiesToSpawn.Add(EnemyUnitDatabase.EnemyUnitTemplateDefinitions[x]);
			return true;
		});
	}
}
