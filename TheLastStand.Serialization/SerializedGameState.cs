using System;
using System.Collections.Generic;
using TheLastStand.Manager;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.Serialization.Building;
using TheLastStand.Serialization.Item;
using TheLastStand.Serialization.Meta;
using TheLastStand.Serialization.Sink;
using TheLastStand.Serialization.SpawnWave;
using TheLastStand.Serialization.Trophy;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedGameState : SerializedContainer
{
	public SerializedApocalypse Apocalypse;

	public SerializedBuildings Buildings;

	public List<string> DLCsInUse;

	public SerializedFog Fog;

	public SerializedPanic Panic;

	public SerializedGame Game;

	public SerializedItems Inventory;

	public SerializedMetaConditionsContext MetaConditionsRunContext;

	public List<string> ModsInUse;

	public SerializedPlayableUnits PlayableUnits;

	public SerializedEnemyUnits EnemyUnits;

	public SerializedBossData BossData;

	public SerializedRandoms Random;

	public SerializedResources Resources;

	public SerializedSpawnWaveContainer SpawnWaveContainer;

	public float TotalTimeSpent;

	public SerializedLUT SerializedLut;

	public SerializedTrophies Trophies;

	public SerializedNightReport SerializedNightReport;

	public SerializedGlyphsContainer SerializedGlyphsContainer;

	public SerializedGameAnalytics SerializedGameAnalytics;

	public SerializedSinks Sinks;

	public override byte GetSaveVersion()
	{
		return SaveManager.GameSaveVersion;
	}
}
