using System.Collections.Generic;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Fog;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Model;

public class Fog
{
	public class LightFogTileInfo
	{
		public enum E_LightFogMode
		{
			None,
			Activated,
			Deactivated,
			Impeded
		}

		public E_LightFogMode Mode;

		public int Buffer;

		public LightFogTileInfo(E_LightFogMode mode)
		{
			Mode = mode;
			Buffer = 1;
		}
	}

	private int densityIndex;

	private int dailyUpdateFrenquency = 1;

	public Dictionary<Tile, LightFogTileInfo> LightFogTiles = new Dictionary<Tile, LightFogTileInfo>();

	public bool CanDecreaseFogDensity => densityIndex > 0;

	public bool CanIncreaseFogDensity => densityIndex < FogDefinition.FogDensities.Count - 1;

	public string DensityName => FogDefinition.FogDensities[DensityIndex].Name;

	public int DensityValue => FogDefinition.FogDensities[DensityIndex].Value;

	public FogDefinition FogDefinition { get; set; }

	public List<Tile> FogTiles { get; set; } = new List<Tile>();

	public List<Tile> TilesOutOfFog { get; set; } = new List<Tile>();

	public int DensityIndex
	{
		get
		{
			return densityIndex;
		}
		set
		{
			if (densityIndex != value)
			{
				PreviousDensityIndex = densityIndex;
				densityIndex = Mathf.Clamp(value, 0, FogDefinition.FogDensities.Count - 1);
			}
		}
	}

	public int DailyUpdateFrequency
	{
		get
		{
			return dailyUpdateFrenquency;
		}
		set
		{
			if (dailyUpdateFrenquency != value)
			{
				dailyUpdateFrenquency = Mathf.Max(1, value);
			}
		}
	}

	public bool HasBeenRepelled { get; set; }

	public int PreviousDensityIndex { get; private set; }

	public Fog(FogDefinition fogDefinition)
	{
		FogDefinition = fogDefinition;
		DensityIndex = fogDefinition.InitialDensityIndex;
		DailyUpdateFrequency = fogDefinition.IncreaseEveryXDays;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<FogModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			FogModifierMetaEffectDefinition[] array = effects;
			foreach (FogModifierMetaEffectDefinition fogModifierMetaEffectDefinition in array)
			{
				if (fogModifierMetaEffectDefinition.IncreaseEveryXDays > -1)
				{
					DailyUpdateFrequency = fogModifierMetaEffectDefinition.IncreaseEveryXDays;
				}
				if (fogModifierMetaEffectDefinition.InitialDensityIndex > -1)
				{
					DensityIndex = fogModifierMetaEffectDefinition.InitialDensityIndex;
				}
			}
		}
		DensityIndex += ApocalypseManager.CurrentApocalypse.StartingFogDensityModifier;
		DailyUpdateFrequency -= ApocalypseManager.CurrentApocalypse.DailyFogUpdateFrequencyModifier;
	}
}
