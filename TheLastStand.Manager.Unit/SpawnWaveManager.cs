using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Unit.Enemy;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.SpawnWave;
using TheLastStand.View.Seer;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Manager.Unit;

public class SpawnWaveManager : Manager<SpawnWaveManager>, ISerializable, IDeserializable
{
	public class SpawnWaveInterpreter
	{
		public float Night => TPSingleton<GameManager>.Instance.Game.DayNumber + 1;

		public int Multiplier
		{
			get
			{
				if (TPSingleton<GameManager>.Instance.Game.DayNumber >= SpawnDefinition.SpawnsCountMultipliers.Count)
				{
					TPSingleton<SpawnWaveManager>.Instance.LogWarning($"SpawnsCountMultiplier isn't defined for this night : {TPSingleton<GameManager>.Instance.Game.DayNumber + 1}", CLogLevel.MAJOR);
					return SpawnDefinition.SpawnsCountMultipliers[SpawnDefinition.SpawnsCountMultipliers.Count - 1];
				}
				return SpawnDefinition.SpawnsCountMultipliers[TPSingleton<GameManager>.Instance.Game.DayNumber];
			}
		}
	}

	public class StringToSpawnWaveIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(SpawnWaveDatabase.WaveDefinitions.Keys);
	}

	public class StringToSpawnDirectionIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(SpawnWaveDatabase.SpawnDirectionDefinitions.Keys);
	}

	[SerializeField]
	private SpawnWaveView spawnWaveView;

	private bool initialized;

	private SpawnWave currentSpawnWave;

	private SpawnWaveInfos rerolledSpawnWave;

	public static bool AliveSeer { get; private set; }

	public static float EnemyWeightModifierXpRatioForXpDistribution { get; private set; }

	public static string LastPlayedSpawnWaveId { get; set; }

	public static SpawnWave CurrentSpawnWave
	{
		get
		{
			return TPSingleton<SpawnWaveManager>.Instance.currentSpawnWave;
		}
		set
		{
			TPSingleton<SpawnWaveManager>.Instance.currentSpawnWave = value;
		}
	}

	public static float CurrentWavePercentageModifier
	{
		get
		{
			int num = 0;
			num += ApocalypseManager.CurrentApocalypse.ExtraPercentageOfEnemies;
			if (GlyphManager.TryGetGlyphEffects(out List<GlyphDecreaseEnemiesCountEffectDefinition> glyphEffects))
			{
				for (int num2 = glyphEffects.Count - 1; num2 >= 0; num2--)
				{
					num -= glyphEffects[num2].Percentage;
				}
			}
			return num;
		}
	}

	public static float CurrentWaveEnemyWeightModifierXpRatio
	{
		get
		{
			if (CurrentSpawnWave == null)
			{
				return 1f;
			}
			return CurrentSpawnWave.EnemyWeightModifierXpRatio;
		}
	}

	public static bool DisplayAllEnemyTiers { get; private set; }

	public static bool DetailedSpawnWaveArrows { get; private set; }

	public static bool DisplayQuantities { get; private set; }

	public static SpawnWaveInfos RerolledSpawnWave
	{
		get
		{
			return TPSingleton<SpawnWaveManager>.Instance.rerolledSpawnWave;
		}
		set
		{
			TPSingleton<SpawnWaveManager>.Instance.rerolledSpawnWave = value;
		}
	}

	public static SpawnDefinition SpawnDefinition => SpawnWaveDatabase.SpawnDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.SpawnDefinitionId];

	public static Dictionary<SpawnDirectionsDefinition, List<int>> AllowedRotationCountsPerDirection { get; private set; }

	public static Dictionary<int, Dictionary<string, int>> DirectionDefinitionsPerStartingDay { get; private set; }

	public static SpawnWaveView SpawnWaveView => TPSingleton<SpawnWaveManager>.Instance.spawnWaveView;

	public bool RerollFromSave { get; set; }

	public SpawnWaveInterpreter SpawnWaveInterpreterObject { get; private set; }

	public static void GenerateSpawnWave(bool isReroll = false)
	{
		RandomManager.SaveState(TPSingleton<SpawnWaveManager>.Instance);
		if (isReroll)
		{
			RerolledSpawnWave = new SpawnWaveInfos(CurrentSpawnWave);
		}
		else if (!TPSingleton<SpawnWaveManager>.Instance.RerollFromSave)
		{
			RerolledSpawnWave = null;
		}
		isReroll = isReroll || TPSingleton<SpawnWaveManager>.Instance.RerollFromSave;
		CurrentSpawnWave = new SpawnWaveController(GetRandomWaveDefinition(isReroll), SpawnWaveView, isReroll).SpawnWave;
		TPSingleton<TrophyManager>.Instance.SpawnWaveDuration = CurrentSpawnWave.SpawnWaveDefinition.Duration;
		TPSingleton<SpawnWaveManager>.Instance.RerollFromSave = false;
	}

	public static void DeserializeSpawnWave(SerializedSpawnWaveContainer serializedSpawnWaveContainer, int saveVersion)
	{
		RandomManager.SaveState(TPSingleton<SpawnWaveManager>.Instance);
		RerolledSpawnWave = null;
		CurrentSpawnWave = new SpawnWaveController(serializedSpawnWaveContainer, SpawnWaveView, saveVersion).SpawnWave;
		TPSingleton<TrophyManager>.Instance.SpawnWaveDuration = CurrentSpawnWave.SpawnWaveDefinition.Duration;
		TPSingleton<SpawnWaveManager>.Instance.RerollFromSave = false;
	}

	public static void OverrideCurrentSpawnWave(string spawnWaveId, string spawnDirectionId)
	{
		if (SpawnWaveDatabase.WaveDefinitions.TryGetValue(spawnWaveId, out var value))
		{
			RandomManager.SaveState(TPSingleton<SpawnWaveManager>.Instance);
			SpawnDirectionsDefinition valueOrDefault = SpawnWaveDatabase.SpawnDirectionDefinitions.GetValueOrDefault(spawnDirectionId);
			RerolledSpawnWave = new SpawnWaveInfos(CurrentSpawnWave);
			CurrentSpawnWave = new SpawnWaveController(value, SpawnWaveView, isReroll: true, valueOrDefault).SpawnWave;
			TPSingleton<TrophyManager>.Instance.SpawnWaveDuration = CurrentSpawnWave.SpawnWaveDefinition.Duration;
			TPSingleton<SpawnWaveManager>.Instance.RerollFromSave = false;
		}
		else
		{
			TPSingleton<SpawnWaveManager>.Instance.LogError("Tried to override the current spawn wave, but the specified ID was not found in the Database. Skipping override.");
		}
	}

	public static void RefreshEnemyWeightModifierXpRatio()
	{
		if (CurrentSpawnWave != null)
		{
			EnemyWeightModifierXpRatioForXpDistribution = CurrentSpawnWave.EnemyWeightModifierXpRatio;
			TPSingleton<SpawnWaveManager>.Instance.Log($"Refreshing Enemy weight modifier xp ratio: {EnemyWeightModifierXpRatioForXpDistribution}", CLogLevel.MAJOR, forcePrintInUnity: true);
		}
	}

	public static SpawnWaveDefinition TryGetSpawnWaveDefinitionOrRandom(string spawnWaveDefinitionId, out bool success)
	{
		SpawnWaveDefinition valueOrDefault = SpawnWaveDatabase.WaveDefinitions.GetValueOrDefault(spawnWaveDefinitionId);
		success = valueOrDefault != null;
		return valueOrDefault ?? GetRandomWaveDefinition();
	}

	public static SpawnWaveDefinition GetRandomWaveDefinition(bool isReroll = false)
	{
		Dictionary<string, int> dictionary = null;
		int num = 0;
		foreach (KeyValuePair<int, Dictionary<string, int>> spawnWavesPerDayDefinition in SpawnDefinition.SpawnWavesPerDayDefinitions)
		{
			if (spawnWavesPerDayDefinition.Key <= TPSingleton<GameManager>.Instance.Game.DayNumber + 1 && (dictionary == null || num < spawnWavesPerDayDefinition.Key))
			{
				dictionary = spawnWavesPerDayDefinition.Value;
				num = spawnWavesPerDayDefinition.Key;
			}
		}
		string[] lockedWavesIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedWavesIds();
		foreach (string key in lockedWavesIds)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary.Remove(key);
			}
		}
		if (isReroll && dictionary.Count > 1)
		{
			dictionary.Remove(RerolledSpawnWave.SpawnWaveDefinitionId);
		}
		int num2 = 0;
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			num2 += item.Value;
		}
		int num3 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, num2);
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			num3 -= item2.Value;
			if (num3 < 0)
			{
				if (SpawnWaveDatabase.WaveDefinitions.TryGetValue(item2.Key, out var value))
				{
					return value;
				}
				TPSingleton<SpawnWaveManager>.Instance.LogError("I'm a lonely spawn wave manager trying to get a random spawn wave definition, and this is my story:", CLogLevel.DETAILED);
				TPSingleton<SpawnWaveManager>.Instance.LogError("I've been looking in the following definition list: " + string.Join(", ", dictionary.Keys) + " and I finally settled on the definition " + item2.Key, CLogLevel.DETAILED);
				TPSingleton<SpawnWaveManager>.Instance.LogError("But it doesn't exist! Here's what exists in the definition SWD: " + string.Join(", ", SpawnWaveDatabase.WaveDefinitions.Keys), CLogLevel.DETAILED);
				TPSingleton<SpawnWaveManager>.Instance.LogError("Please notice me :(", CLogLevel.DETAILED);
				throw new Database<SpawnWaveDatabase>.MissingAssetException(item2.Key);
			}
		}
		TPSingleton<SpawnWaveManager>.Instance.LogError("No valid spawn waves found!", CLogLevel.DETAILED);
		return null;
	}

	public static SpawnDirectionsDefinition GetRandomSpawnWaveDirection(bool isReroll = false)
	{
		Dictionary<string, int> dictionary = null;
		int num = 0;
		foreach (KeyValuePair<int, Dictionary<string, int>> spawnDirectionsPerDayDefinition in SpawnDefinition.SpawnDirectionsPerDayDefinitions)
		{
			if (spawnDirectionsPerDayDefinition.Key <= TPSingleton<GameManager>.Instance.Game.DayNumber + 1 && (dictionary == null || num < spawnDirectionsPerDayDefinition.Key))
			{
				dictionary = spawnDirectionsPerDayDefinition.Value;
				num = spawnDirectionsPerDayDefinition.Key;
			}
		}
		if (isReroll && dictionary.Count > 1)
		{
			dictionary.Remove(RerolledSpawnWave.SpawnDirectionDefinitionId);
		}
		int num2 = 0;
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			num2 += item.Value;
		}
		int num3 = RandomManager.GetRandomRange(TPSingleton<SpawnWaveManager>.Instance, 0, num2 - 1);
		foreach (KeyValuePair<string, int> item2 in dictionary)
		{
			num3 -= item2.Value;
			if (num3 < 0)
			{
				return SpawnWaveDatabase.SpawnDirectionDefinitions[item2.Key];
			}
		}
		TPSingleton<SpawnWaveManager>.Instance.LogError("No valid spawn waves found!", CLogLevel.DETAILED);
		return null;
	}

	public static void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		if (previousState == Game.E_State.NightReport)
		{
			TPSingleton<SpawnWaveManager>.Instance.spawnWaveView.Refresh();
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedSpawnWaveContainer serializedSpawnWaveContainer)
		{
			DetailedSpawnWaveArrows = serializedSpawnWaveContainer.DisplayDangerIndicators;
			RerolledSpawnWave = ((serializedSpawnWaveContainer.LastRerolledSpawnWave != null) ? new SpawnWaveInfos(serializedSpawnWaveContainer.LastRerolledSpawnWave) : null);
			RerollFromSave = serializedSpawnWaveContainer.LastRerolledSpawnWave != null;
			LastPlayedSpawnWaveId = serializedSpawnWaveContainer.LastPlayedSpawnWaveId;
		}
		else
		{
			DisplayAllEnemyTiers = false;
			DetailedSpawnWaveArrows = false;
			DisplayQuantities = false;
			LastPlayedSpawnWaveId = string.Empty;
		}
		AliveSeer = TPSingleton<BuildingManager>.Instance.Buildings.Any((TheLastStand.Model.Building.Building o) => o.Id == "Seer");
	}

	public void Init()
	{
		if (!initialized)
		{
			SpawnWaveInterpreterObject = new SpawnWaveInterpreter();
			PrecomputeDirections();
			PrecomputeRotationCountsPerDirections();
			initialized = true;
		}
	}

	public void OnSeerBuiltOrDestroyed(bool built)
	{
		AliveSeer = built;
		SetDisplayAllEnemyTiers(DisplayAllEnemyTiers && built, refreshView: false);
		SetDetailedSpawnWaveArrows(DetailedSpawnWaveArrows && built, refreshView: false);
		SetDisplayQuantities(DisplayQuantities && built);
		SpawnWaveView.Refresh();
	}

	public void OnTurnStart()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			SetDetailedSpawnWaveArrows(state: false);
		}
		SpawnWaveView.Refresh();
	}

	public ISerializedData Serialize()
	{
		return new SerializedSpawnWaveContainer
		{
			DisplayDangerIndicators = DetailedSpawnWaveArrows,
			CurrentSpawnWave = (CurrentSpawnWave?.Serialize() as SerializedSpawnWave),
			LastRerolledSpawnWave = ((RerolledSpawnWave != null) ? new SerializedSpawnWaveContainer.RerolledSpawnWave
			{
				SpawnWaveDefinitionId = RerolledSpawnWave.SpawnWaveDefinitionId,
				RotationsCount = RerolledSpawnWave.RotationsCount,
				SpawnDirectionDefinitionId = RerolledSpawnWave.SpawnDirectionDefinitionId,
				EnemiesId = rerolledSpawnWave.EnemiesToSpawn.Select((EnemyUnitTemplateDefinition x) => x.Id).ToList()
			} : null),
			LastPlayedSpawnWaveId = LastPlayedSpawnWaveId
		};
	}

	public void SetDisplayAllEnemyTiers(bool state, bool refreshView = true)
	{
		DisplayAllEnemyTiers = state;
		if (refreshView)
		{
			SpawnWaveView.Refresh();
		}
	}

	public void SetDetailedSpawnWaveArrows(bool state, bool refreshView = true)
	{
		DetailedSpawnWaveArrows = state;
		if (refreshView)
		{
			SpawnWaveView.Refresh();
		}
	}

	public void SetDisplayQuantities(bool state)
	{
		DisplayQuantities = state;
		if (DisplayQuantities)
		{
			TPSingleton<SeerPreviewDisplay>.Instance.DisplayQuantitiesOnRevealedEnemies();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private static void ClearWavesByForbiddenSides(Dictionary<string, int> spawnWavesDirectionDefinitionsIds, int dayNumber)
	{
		List<string> list = new List<string>();
		List<SpawnDirectionsDefinition.E_Direction> overridenForbiddenDirectionsForDayNumber = SpawnDefinition.GetOverridenForbiddenDirectionsForDayNumber(dayNumber);
		List<SpawnDirectionsDefinition.E_Direction> list2 = ((overridenForbiddenDirectionsForDayNumber == null || overridenForbiddenDirectionsForDayNumber.Count == 0) ? TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.ForbiddenDirections : overridenForbiddenDirectionsForDayNumber);
		foreach (KeyValuePair<string, int> spawnWavesDirectionDefinitionsId in spawnWavesDirectionDefinitionsIds)
		{
			SpawnDirectionsDefinition spawnDirectionsDefinition = SpawnWaveDatabase.SpawnDirectionDefinitions[spawnWavesDirectionDefinitionsId.Key];
			if (spawnDirectionsDefinition.SpawnDirectionsInfo.Count > 4 - list2.Count)
			{
				list.Add(spawnWavesDirectionDefinitionsId.Key);
			}
			else if (spawnDirectionsDefinition.SpawnDirectionsInfo.Count == 2 && list2.Count == 2)
			{
				bool num = list2[0].IsOppositeTo(list2[1]);
				bool flag = spawnDirectionsDefinition.SpawnDirectionsInfo.ElementAt(0).Key.IsOppositeTo(spawnDirectionsDefinition.SpawnDirectionsInfo.ElementAt(1).Key);
				if (num != flag)
				{
					list.Add(spawnWavesDirectionDefinitionsId.Key);
				}
			}
		}
		foreach (string item in list)
		{
			TPSingleton<SpawnWaveManager>.Instance.Log("Removing " + item + " due to forbidden sides filter.", CLogLevel.DETAILED);
			spawnWavesDirectionDefinitionsIds.Remove(item);
		}
	}

	private void PrecomputeDirections()
	{
		DirectionDefinitionsPerStartingDay = new Dictionary<int, Dictionary<string, int>>();
		foreach (KeyValuePair<int, Dictionary<string, int>> spawnDirectionsPerDayDefinition in SpawnDefinition.SpawnDirectionsPerDayDefinitions)
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>(spawnDirectionsPerDayDefinition.Value);
			ClearWavesByForbiddenSides(dictionary, spawnDirectionsPerDayDefinition.Key);
			DirectionDefinitionsPerStartingDay.Add(spawnDirectionsPerDayDefinition.Key, dictionary);
		}
	}

	private void PrecomputeRotationCountsPerDirections()
	{
		AllowedRotationCountsPerDirection = new Dictionary<SpawnDirectionsDefinition, List<int>>();
		foreach (KeyValuePair<int, Dictionary<string, int>> item in DirectionDefinitionsPerStartingDay)
		{
			List<SpawnDirectionsDefinition.E_Direction> overridenForbiddenDirectionsForDayNumber = SpawnDefinition.GetOverridenForbiddenDirectionsForDayNumber(item.Key);
			List<SpawnDirectionsDefinition.E_Direction> source = ((overridenForbiddenDirectionsForDayNumber == null || overridenForbiddenDirectionsForDayNumber.Count == 0) ? TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.ForbiddenDirections : overridenForbiddenDirectionsForDayNumber);
			foreach (KeyValuePair<string, int> item2 in item.Value)
			{
				SpawnDirectionsDefinition spawnDirectionsDefinition = SpawnWaveDatabase.SpawnDirectionDefinitions[item2.Key];
				if (AllowedRotationCountsPerDirection.ContainsKey(spawnDirectionsDefinition))
				{
					continue;
				}
				List<int> list = new List<int>();
				Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer> rotatedProportionPerDirection = new Dictionary<SpawnDirectionsDefinition.E_Direction, SpawnDirectionsDefinition.SpawnDirectionInfoContainer>(SpawnDirectionsDefinition.SharedDirectionComparer)
				{
					{
						SpawnDirectionsDefinition.E_Direction.Top,
						spawnDirectionsDefinition.SpawnDirectionsInfo.GetValueOrDefault(SpawnDirectionsDefinition.E_Direction.Top)
					},
					{
						SpawnDirectionsDefinition.E_Direction.Right,
						spawnDirectionsDefinition.SpawnDirectionsInfo.GetValueOrDefault(SpawnDirectionsDefinition.E_Direction.Right)
					},
					{
						SpawnDirectionsDefinition.E_Direction.Bottom,
						spawnDirectionsDefinition.SpawnDirectionsInfo.GetValueOrDefault(SpawnDirectionsDefinition.E_Direction.Bottom)
					},
					{
						SpawnDirectionsDefinition.E_Direction.Left,
						spawnDirectionsDefinition.SpawnDirectionsInfo.GetValueOrDefault(SpawnDirectionsDefinition.E_Direction.Left)
					}
				};
				for (int i = 0; i < 4; i++)
				{
					SpawnDirectionsDefinition.SpawnDirectionInfoContainer value = rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Top];
					SpawnDirectionsDefinition.SpawnDirectionInfoContainer value2 = rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Right];
					SpawnDirectionsDefinition.SpawnDirectionInfoContainer value3 = rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Bottom];
					SpawnDirectionsDefinition.SpawnDirectionInfoContainer value4 = rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Left];
					rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Top] = value4;
					rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Right] = value;
					rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Bottom] = value2;
					rotatedProportionPerDirection[SpawnDirectionsDefinition.E_Direction.Left] = value3;
					if (!source.Any((SpawnDirectionsDefinition.E_Direction o) => rotatedProportionPerDirection[o] != null && rotatedProportionPerDirection[o].TotalProportion > 0))
					{
						list.Add(i + 1);
					}
				}
				AllowedRotationCountsPerDirection.Add(spawnDirectionsDefinition, list);
				TPSingleton<SpawnWaveManager>.Instance.Log("Valid rotation count in " + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + " for DirectionDefinition " + spawnDirectionsDefinition.Id + " are " + string.Join(",", list) + ".", CLogLevel.DETAILED);
			}
		}
	}

	[DevConsoleCommand(Name = "OverrideCurrentSpawnWave")]
	public static void Debug_OverrideCurrentSpawnWave([StringConverter(typeof(StringToSpawnWaveIdConverter))] string spawnId, [StringConverter(typeof(StringToSpawnDirectionIdConverter))] string directionId)
	{
		OverrideCurrentSpawnWave(spawnId, directionId);
		SpawnWaveView.Refresh();
	}

	[DevConsoleCommand(Name = "GenerateNewWave")]
	public static void Debug_GenerateNewWave(bool isReroll = false)
	{
		GenerateSpawnWave(isReroll);
		SpawnWaveView.Refresh();
	}

	[DevConsoleCommand(Name = "RotateWaveTo")]
	public static void Debug_RotateWaveTo(int rotation)
	{
		rotation = Mathf.Clamp(rotation, 0, 3);
		CurrentSpawnWave.RotatedProportionPerDirection = CurrentSpawnWave.SpawnWaveController.RotateDirection(CurrentSpawnWave.SpawnDirectionsDefinition.SpawnDirectionsInfo, rotation);
		CurrentSpawnWave.SpawnWaveController.RecomputeSpawnPoints();
		CurrentSpawnWave.RotationsCount = rotation;
		SpawnWaveView.Refresh();
	}

	[DevConsoleCommand(Name = "RotateWaveAddingValue")]
	public static void Debug_RotateWaveAddingValue(int value = 1)
	{
		int num = (CurrentSpawnWave.RotationsCount + value) % 4;
		if (num < 0)
		{
			num = 4 + num;
		}
		CurrentSpawnWave.RotatedProportionPerDirection = CurrentSpawnWave.SpawnWaveController.RotateDirection(CurrentSpawnWave.SpawnDirectionsDefinition.SpawnDirectionsInfo, num);
		CurrentSpawnWave.SpawnWaveController.RecomputeSpawnPoints();
		CurrentSpawnWave.RotationsCount = num;
		SpawnWaveView.Refresh();
	}

	[DevConsoleCommand(Name = "SeerToggleDetailedInfos")]
	public static void Debug_SeerToggleDetailedInfos(bool value = true)
	{
		TPSingleton<SpawnWaveManager>.Instance.SetDetailedSpawnWaveArrows(value);
	}
}
