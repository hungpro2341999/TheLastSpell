using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Controller.WorldMap;
using TheLastStand.Database.Meta;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.DLC;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Apocalypse;
using TheLastStand.View.WorldMap;
using UnityEngine;

namespace TheLastStand.Model.WorldMap;

public class WorldMapCity : ISerializable, IDeserializable
{
	public int CurrentGlyphPoints;

	public Dictionary<string, int> CompletedApocalypseModifiersStepIndex = new Dictionary<string, int>();

	public int MaxApocalypsePassed = -1;

	public int MaxNightReached;

	public int NumberOfRuns;

	public int NumberOfWins;

	public WorldMapCityView WorldMapCityView;

	public GlyphsConfig GlyphsConfig;

	public bool HaveWon => NumberOfWins > 0;

	public CityDefinition CityDefinition { get; private set; }

	public bool IsSelectable { get; private set; }

	public bool IsUnlocked { get; private set; }

	public bool IsLinkedCityUnlocked
	{
		get
		{
			if (!string.IsNullOrEmpty(CityDefinition.LinkedCityId))
			{
				return !TPSingleton<MetaUpgradesManager>.Instance.GetLockedCitiesIds().Contains(CityDefinition.LinkedCityId);
			}
			return true;
		}
	}

	public bool IsLinkedDLCOwned
	{
		get
		{
			if (!string.IsNullOrEmpty(CityDefinition.LinkedDLCId))
			{
				return TPSingleton<DLCManager>.Instance.IsDLCOwned(CityDefinition.LinkedDLCId);
			}
			return true;
		}
	}

	public bool IsVisible { get; private set; }

	public string RunId => $"{CityDefinition.Id}_{NumberOfRuns}";

	public WorldMapCityController WorldMapCityController { get; private set; }

	public WorldMapCity(CityDefinition definition, WorldMapCityController controller, WorldMapCityView view)
	{
		CityDefinition = definition;
		WorldMapCityController = controller;
		WorldMapCityView = view;
		GlyphsConfig = new GlyphsConfig
		{
			SelectedGlyphs = new List<GlyphDefinition>()
		};
		RefreshIsVisible();
		RefreshIsUnlocked();
	}

	public void CheckRefreshIsUnlocked()
	{
		bool isUnlocked = IsUnlocked;
		if (RefreshIsUnlocked() != isUnlocked && WorldMapCityView != null)
		{
			WorldMapCityView.RefreshAnimation();
		}
	}

	public int GetCustomModeBonusPoints()
	{
		return Mathf.Max(0, CurrentGlyphPoints - CityDefinition.MaxGlyphPoints);
	}

	public bool RefreshIsSelectable()
	{
		IsSelectable = !CityDefinition.Hidden && (!CityDefinition.IsTutorialMap || TPSingleton<WorldMapCityManager>.Instance.Cities.Count((WorldMapCity c) => !c.CityDefinition.Hidden && c.IsVisible && c != this) <= 0);
		return IsSelectable;
	}

	public bool RefreshIsUnlocked()
	{
		if (CityDefinition.IsStoryMap)
		{
			IsUnlocked = true;
		}
		else if (!string.IsNullOrEmpty(CityDefinition.LinkedDLCId))
		{
			IsUnlocked = IsLinkedDLCOwned && !TPSingleton<MetaUpgradesManager>.Instance.GetLockedCitiesIds().Contains(CityDefinition.Id);
		}
		else
		{
			IsUnlocked = !TPSingleton<MetaUpgradesManager>.Instance.GetLockedCitiesIds().Contains(CityDefinition.Id);
		}
		return IsUnlocked;
	}

	public bool RefreshIsVisible()
	{
		if (CityDefinition.IsStoryMap)
		{
			IsVisible = !TPSingleton<MetaUpgradesManager>.Instance.GetLockedCitiesIds().Contains(CityDefinition.Id);
		}
		else if (!string.IsNullOrEmpty(CityDefinition.LinkedCityId))
		{
			IsVisible = !TPSingleton<MetaUpgradesManager>.Instance.GetLockedCitiesIds().Contains(CityDefinition.LinkedCityId);
		}
		else
		{
			IsVisible = !TPSingleton<MetaUpgradesManager>.Instance.GetLockedCitiesIds().Contains(CityDefinition.Id);
		}
		return IsVisible;
	}

	public string GetLockedCityText()
	{
		if (IsUnlocked)
		{
			return string.Empty;
		}
		switch (GetLinkedMetaUpgradeState())
		{
		case MetaUpgradesManager.E_MetaState.Locked:
		{
			if (CityDefinition.HasLinkedCity && CityDatabase.CityDefinitions.TryGetValue(CityDefinition.LinkedCityId, out var value))
			{
				return "<style=Bad>" + Localizer.Format("MapPanel_ReduxMap_Tooltip_Locked", value.Name) + "</style>";
			}
			break;
		}
		case MetaUpgradesManager.E_MetaState.Unlocked:
			return "<style=Bad>" + Localizer.Format("WorldMap_CantStartNewGame_MetaNotActivated", CityDefinition.Name) + "</style>";
		}
		return string.Empty;
	}

	public string GetMissingDLCText()
	{
		if (!CityDefinition.HasLinkedDLC)
		{
			return string.Empty;
		}
		DLCDefinition dLCFromId = TPSingleton<DLCManager>.Instance.GetDLCFromId(CityDefinition.LinkedDLCId);
		if (dLCFromId != null)
		{
			return Localizer.Format("MapPanel_ReduxMap_Tooltip_DLCNotOwned", dLCFromId.LocalizedName);
		}
		return string.Empty;
	}

	public MetaUpgradesManager.E_MetaState GetLinkedMetaUpgradeState()
	{
		MetaUpgrade upgrade;
		return MetaUpgradesManager.TryGetUpgradeState($"Unlock{CityDefinition.Id}City", out upgrade, includeFulfilledList: true);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (!(container is SerializedCity serializedCity))
		{
			return;
		}
		MaxApocalypsePassed = Mathf.Min(serializedCity.MaxApoPassed, ApocalypseManager.GetMaxApocalypseLevel());
		if (saveVersion >= 0 && saveVersion <= 11)
		{
			MaxNightReached = ((serializedCity.NumberOfWins > 1) ? CityDefinition.VictoryDaysCount : 0);
		}
		else
		{
			MaxNightReached = serializedCity.MaxNightReached;
		}
		NumberOfRuns = serializedCity.NumberOfRuns;
		if (serializedCity.NumberOfWins > 0)
		{
			NumberOfWins = serializedCity.NumberOfWins;
		}
		else
		{
			NumberOfWins = ((MaxApocalypsePassed >= 0) ? 1 : 0);
			if (NumberOfWins > 0)
			{
				TPSingleton<MetaConditionManager>.Instance.IncreaseRunsCompleted(isGameOver: false, this, MaxApocalypsePassed);
			}
		}
		foreach (string selectedGlyph in serializedCity.SelectedGlyphs)
		{
			if (GlyphDatabase.GlyphDefinitions.TryGetValue(selectedGlyph, out var value))
			{
				GlyphsConfig.SelectedGlyphs.Add(value);
			}
		}
		GlyphsConfig.CustomModeEnabled = serializedCity.CustomModeEnabled;
		CurrentGlyphPoints = GlyphsConfig.SelectedGlyphs.Sum((GlyphDefinition o) => o.Cost);
		CompletedApocalypseModifiersStepIndex.Clear();
		if (serializedCity.CompletedModifiersSteps != null)
		{
			foreach (SerializedApocalypseModifierStep completedModifiersStep in serializedCity.CompletedModifiersSteps)
			{
				if (!CompletedApocalypseModifiersStepIndex.ContainsKey(completedModifiersStep.ModifierId))
				{
					CompletedApocalypseModifiersStepIndex.Add(completedModifiersStep.ModifierId, completedModifiersStep.StepIndex);
				}
				else
				{
					TPSingleton<WorldMapCityManager>.Instance.LogError("Apocalypse modifier id '" + completedModifiersStep.ModifierId + "' already present when deserializing completed apocalypse modifiers, this shouldn't happen !");
				}
			}
		}
		if (saveVersion <= 13)
		{
			ApocalypseRetroCompatibilityController.ApplyRetroCompatibilityToWorldMapCity(saveVersion, this);
		}
	}

	public ISerializedData Serialize()
	{
		List<SerializedApocalypseModifierStep> list = new List<SerializedApocalypseModifierStep>();
		foreach (KeyValuePair<string, int> item in CompletedApocalypseModifiersStepIndex)
		{
			list.Add(new SerializedApocalypseModifierStep
			{
				ModifierId = item.Key,
				StepIndex = item.Value
			});
		}
		return new SerializedCity
		{
			Id = CityDefinition.Id,
			MaxApoPassed = MaxApocalypsePassed,
			MaxNightReached = MaxNightReached,
			NumberOfRuns = NumberOfRuns,
			NumberOfWins = NumberOfWins,
			SelectedGlyphs = GlyphsConfig.SelectedGlyphs.Select((GlyphDefinition glyph) => glyph.Id).ToList(),
			CustomModeEnabled = GlyphsConfig.CustomModeEnabled,
			CompletedModifiersSteps = list
		};
	}
}
