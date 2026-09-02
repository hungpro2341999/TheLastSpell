using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.WorldMap;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Model;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.MetaShops;
using TheLastStand.View.WorldMap;
using TheLastStand.View.WorldMap.Apocalypse;
using TheLastStand.View.WorldMap.Glyphs;
using TheLastStand.View.WorldMap.ItemRestriction;
using UnityEngine;

namespace TheLastStand.Manager.WorldMap;

public class WorldMapCityManager : Manager<WorldMapCityManager>, ISerializable, IDeserializable
{
	[SerializeField]
	private WorldMapCityView cityViewPrefab;

	private WorldMapCity selectedCity;

	private bool hasBeenInitialized;

	public static bool CanSelectAnyCity
	{
		get
		{
			if (!TPSingleton<WeaponRestrictionsPanel>.Instance.Displayed && !TPSingleton<WeaponRestrictionsPanel>.Instance.OpeningOrClosing && !TPSingleton<ApocalypseMoreInfoPanel>.Instance.Displayed)
			{
				return !TPSingleton<ApocalypseMoreInfoPanel>.Instance.OpeningOrClosing;
			}
			return false;
		}
	}

	public LastRunInfo LastRunInfo { get; private set; }

	public WorldMapCity SelectedCity
	{
		get
		{
			return TPSingleton<WorldMapCityManager>.Instance.selectedCity;
		}
		set
		{
			if (TPSingleton<WorldMapCityManager>.Instance.selectedCity != value)
			{
				TPSingleton<WorldMapCityManager>.Instance.selectedCity?.WorldMapCityView.OnDeselection();
				TPSingleton<WorldMapCityManager>.Instance.selectedCity = value;
				TPSingleton<WorldMapCityManager>.Instance.selectedCity?.WorldMapCityView.OnSelection();
				WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.FOCUSED);
			}
		}
	}

	public List<WorldMapCity> Cities { get; private set; }

	public void RefreshCitiesUnlock()
	{
		foreach (WorldMapCity city in Cities)
		{
			city.CheckRefreshIsUnlocked();
		}
	}

	private static WorldMapCity CreateCity(CityDefinition cityDefinition, int saveVersion, SerializedCity cityElement = null)
	{
		WorldMapCity worldMapCity = new WorldMapCityController(cityElement, cityDefinition, null, saveVersion).WorldMapCity;
		InitCity(worldMapCity);
		return worldMapCity;
	}

	private static WorldMapCityView CreateCityView(CityDefinition cityDefinition, Transform parent)
	{
		WorldMapCityView worldMapCityView = UnityEngine.Object.Instantiate(TPSingleton<WorldMapCityManager>.Instance.cityViewPrefab, parent);
		worldMapCityView.gameObject.transform.localPosition = cityDefinition.WorldMapPosition;
		worldMapCityView.gameObject.name = cityDefinition.Id;
		return worldMapCityView;
	}

	private static void InitCity(WorldMapCity city)
	{
		TPSingleton<WorldMapCityManager>.Instance.Cities.Add(city);
	}

	private void GenerateInitialCities(SerializedCities serializedCities, int saveVersion)
	{
		foreach (KeyValuePair<string, CityDefinition> cityDefinition in CityDatabase.CityDefinitions)
		{
			try
			{
				SerializedCity cityElement = serializedCities?.Cities.FirstOrDefault((SerializedCity city) => city.Id == cityDefinition.Key);
				CreateCity(cityDefinition.Value, saveVersion, cityElement);
			}
			catch (Database<CityDatabase>.MissingAssetException arg)
			{
				LogError($"{arg}\nThis asset will be skipped.");
			}
		}
		if (serializedCities != null)
		{
			selectedCity = Cities.FirstOrDefault((WorldMapCity x) => x.CityDefinition.Id == serializedCities.SelectedCityId);
		}
	}

	private void Init()
	{
		if (!hasBeenInitialized)
		{
			hasBeenInitialized = true;
			ApplicationManager.Application.ApplicationController.ApplicationStateChangeEvent += OnApplicationStateChange;
		}
	}

	private void CreateCityViews()
	{
		foreach (WorldMapCity city in Cities)
		{
			city.RefreshIsVisible();
			city.RefreshIsUnlocked();
			city.WorldMapCityView = CreateCityView(city.CityDefinition, TPSingleton<WorldMapRefsManager>.Instance.CitiesParent);
			city.WorldMapCityView.WorldMapCity = city;
			city.WorldMapCityView.Init();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		TPSingleton<WorldMapCityManager>.Instance.Init();
	}

	public static void OnStateChange()
	{
		WorldMapStateManager.WorldMapState currentState = TPSingleton<WorldMapStateManager>.Instance.CurrentState;
		if (currentState != WorldMapStateManager.WorldMapState.EXPLORATION)
		{
			_ = 2;
			return;
		}
		TPSingleton<WorldMapCityManager>.Instance.selectedCity?.WorldMapCityView.OnDeselection();
		TPSingleton<WorldMapCityManager>.Instance.selectedCity = null;
	}

	public static void StartNewGame()
	{
		if (TPSingleton<OraculumView>.Instance.OpeningOrClosing || TPSingleton<GlyphSelectionPanel>.Instance.OpeningOrClosing)
		{
			return;
		}
		ApocalypseManager.SetApocalypse(ApocalypseManager.CurrentApocalypseModifierStepDefinitions.ToList());
		ApocalypseManager.LogCurrentApocalypse();
		if (Analytics.AllowedToSendData)
		{
			string[] usedOmens = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Select((GlyphDefinition glyphDefinition) => glyphDefinition.Id).ToArray();
			string[] activeWeaponFamiliesForAnalytics = TPSingleton<ItemRestrictionManager>.Instance.GetActiveWeaponFamiliesForAnalytics();
			string[] excludedWeaponFamiliesForAnalytics = TPSingleton<ItemRestrictionManager>.Instance.GetExcludedWeaponFamiliesForAnalytics();
			Analytics.SendStartRunEvent(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.RunId, TPSingleton<WorldMapCityManager>.Instance.selectedCity.CityDefinition.Id, ApocalypseManager.CurrentApocalypseLevel, usedOmens, activeWeaponFamiliesForAnalytics, excludedWeaponFamiliesForAnalytics);
		}
		if (!ApplicationManager.Application.HasSeenIntroduction)
		{
			ApplicationManager.Application.HasSeenIntroduction = true;
			AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("NewGame");
			return;
		}
		for (int num = 0; num < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; num++)
		{
			TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[num].FadeOut(TPSingleton<CanvasFadeManager>.Instance.FadeDuration);
		}
		TPSingleton<WorldMapCityManager>.Instance.ResetLastRunInfo();
		if (TPSingleton<WorldMapCityManager>.Instance.selectedCity.CityDefinition.PreGameCutscene != null && TPSingleton<WorldMapCityManager>.Instance.selectedCity.NumberOfRuns < 1)
		{
			AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("NewGame", TPSingleton<WorldMapCityManager>.Instance.selectedCity.CityDefinition.PreGameCutscene);
		}
		else
		{
			ApplicationManager.Application.ApplicationController.SetState("NewGame");
		}
	}

	public List<WorldMapCity> GetOrderedCities(Func<WorldMapCity, bool> predicate = null)
	{
		List<WorldMapCity> list = new List<WorldMapCity>();
		foreach (WorldMapCity city in Cities)
		{
			if (predicate == null || predicate(city))
			{
				list.Add(city);
			}
		}
		return list.OrderBy((WorldMapCity aCity) => aCity.CityDefinition.WorldMapUIOrderIndex).ToList();
	}

	public WorldMapCity GetPreviousCity(IEnumerable<WorldMapCity> cities, WorldMapCity city)
	{
		return cities.TakeWhile((WorldMapCity x) => x != city).DefaultIfEmpty(cities.ElementAt(cities.Count() - 1)).LastOrDefault();
	}

	public WorldMapCity GetNextCity(IEnumerable<WorldMapCity> cities, WorldMapCity city)
	{
		return cities.SkipWhile((WorldMapCity x) => x != city).Skip(1).DefaultIfEmpty(cities.ElementAt(0))
			.FirstOrDefault();
	}

	public void HandleGameOver(Game.E_GameOverCause cause)
	{
		if (selectedCity == null)
		{
			LogWarning("Called HandleGameOver but the selected city is null. Something might have been wrong in the game flow. Skipping", CLogLevel.NORMAL, forcePrintInUnity: true, printStackTrace: true);
			return;
		}
		selectedCity.NumberOfRuns++;
		bool num = SelectedCity.CityDefinition.IsTutorialMap && (TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying || cause == Game.E_GameOverCause.Abandon);
		LastRunInfo = new LastRunInfo(selectedCity, cause);
		if (num || cause == Game.E_GameOverCause.MagicSealsCompleted)
		{
			selectedCity.NumberOfWins++;
			ApocalypseManager.StoreApocalypseStateBeforeGameOver();
			ApocalypseManager.TryIncreaseMaxApocalypseIndexAvailable();
			selectedCity.WorldMapCityController.TryModifyingCompletedApocalypseModifiersSteps(ApocalypseManager.CurrentApocalypseModifierStepDefinitions);
			selectedCity.MaxApocalypsePassed = Mathf.Max(selectedCity.MaxApocalypsePassed, ApocalypseManager.CurrentApocalypseLevel);
			ApocalypseManager.RefreshTiersUnlockedState();
		}
	}

	public bool IsCurrentSelectedCityLastPlayedCity()
	{
		if (LastRunInfo != null)
		{
			return LastRunInfo.CityId == SelectedCity.CityDefinition.Id;
		}
		return false;
	}

	public void ResetLastRunInfo()
	{
		LastRunInfo = null;
	}

	public void SelectNextCity()
	{
		if (!CanSelectAnyCity)
		{
			return;
		}
		if (selectedCity == null)
		{
			SelectedCity = Cities.FirstOrDefault((WorldMapCity x) => x.IsVisible && x.IsSelectable && x.CityDefinition.IsStoryMap);
		}
		else
		{
			SelectedCity = GetNextCity(Cities.Where((WorldMapCity x) => x.IsVisible && x.IsSelectable && x.CityDefinition.IsStoryMap), selectedCity);
		}
	}

	public void SelectPreviousCity()
	{
		if (!CanSelectAnyCity)
		{
			return;
		}
		if (selectedCity == null)
		{
			SelectedCity = Cities.FirstOrDefault((WorldMapCity x) => x.IsVisible && x.IsSelectable && x.CityDefinition.IsStoryMap);
		}
		else
		{
			SelectedCity = GetPreviousCity(Cities.Where((WorldMapCity x) => x.IsVisible && x.IsSelectable && x.CityDefinition.IsStoryMap), selectedCity);
		}
	}

	public void SelectDLCMapCity()
	{
		if (selectedCity != null)
		{
			WorldMapCity selectedCityMapType = GetSelectedCityMapType(getStoryMap: false);
			if (selectedCityMapType != null)
			{
				SelectedCity = selectedCityMapType;
			}
		}
	}

	public void SelectStoryMapCity()
	{
		if (selectedCity != null)
		{
			WorldMapCity selectedCityMapType = GetSelectedCityMapType(getStoryMap: true);
			if (selectedCityMapType != null)
			{
				SelectedCity = selectedCityMapType;
			}
		}
	}

	public void SelectCity(WorldMapCity city)
	{
		if (CanSelectAnyCity && city.IsSelectable && SelectedCity == null && !ACameraView.IsZooming)
		{
			SelectedCity = city;
		}
	}

	public void OnApplicationStateChange(State state)
	{
		string text = state.GetName();
		if (text != null && text == "WorldMap")
		{
			TPSingleton<WorldMapCityManager>.Instance.selectedCity = null;
			CreateCityViews();
			TPSingleton<GlyphManager>.Instance.RemoveOverflowingGlyphs();
			for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeIn();
			}
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnWorldMapOpen);
		}
	}

	private WorldMapCity GetSelectedCityMapType(bool getStoryMap)
	{
		if (selectedCity == null)
		{
			return null;
		}
		string cityId = string.Empty;
		if (getStoryMap)
		{
			if (!selectedCity.CityDefinition.IsStoryMap && !string.IsNullOrEmpty(selectedCity.CityDefinition.LinkedCityId))
			{
				cityId = selectedCity.CityDefinition.LinkedCityId;
			}
			else if (selectedCity.CityDefinition.IsStoryMap)
			{
				return selectedCity;
			}
		}
		else if (selectedCity.CityDefinition.IsStoryMap && !string.IsNullOrEmpty(selectedCity.CityDefinition.LinkedCityId))
		{
			cityId = selectedCity.CityDefinition.LinkedCityId;
		}
		else if (!selectedCity.CityDefinition.IsStoryMap)
		{
			return selectedCity;
		}
		if (!string.IsNullOrEmpty(cityId))
		{
			WorldMapCity worldMapCity = Cities.Find((WorldMapCity city) => city.CityDefinition.Id == cityId);
			if (worldMapCity != null)
			{
				return worldMapCity;
			}
		}
		return null;
	}

	private bool AreAllPanelsClosed()
	{
		if (!TPSingleton<OraculumView>.Instance.Displayed && !TPSingleton<OraculumView>.Instance.OpeningOrClosing && !TPSingleton<GlyphSelectionPanel>.Instance.Displayed && !TPSingleton<GlyphSelectionPanel>.Instance.OpeningOrClosing && !TPSingleton<WeaponRestrictionsPanel>.Instance.Displayed && !TPSingleton<WeaponRestrictionsPanel>.Instance.OpeningOrClosing && !TPSingleton<ApocalypseSelectionPanel>.Instance.Displayed && !TPSingleton<ApocalypseSelectionPanel>.Instance.OpeningOrClosing && !TPSingleton<ApocalypseMoreInfoPanel>.Instance.Displayed)
		{
			return !TPSingleton<ApocalypseMoreInfoPanel>.Instance.OpeningOrClosing;
		}
		return false;
	}

	private void Update()
	{
		if (!(ApplicationManager.Application.State.GetName() == "WorldMap"))
		{
			return;
		}
		if (TPSingleton<WorldMapStateManager>.Instance != null && TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.APOCALYPSE_MAX_LEVEL_FOCUS)
		{
			if (InputManager.GetButtonDown(144))
			{
				TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.OnExitJoystickFocus(triggerChangeState: true);
			}
			return;
		}
		if (InputManager.GetButtonDown(97) && WorldMapUIManager.CanAccessMetaShops && AreAllPanelsClosed())
		{
			TPSingleton<MetaShopsManager>.Instance.OpenShops();
		}
		if (InputManager.GetButtonDown(139) && AreAllPanelsClosed())
		{
			TPSingleton<WeaponRestrictionsPanel>.Instance.Open();
		}
		if (!AreAllPanelsClosed())
		{
			return;
		}
		if (InputManager.GetButtonDown(100))
		{
			if (SelectedCity != null && SelectedCity.WorldMapCityView.CanStartGame && !GenericConsent.IsOpen)
			{
				for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
				{
					TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeOut(TPSingleton<CanvasFadeManager>.Instance.FadeDuration);
				}
				GameConfigurationsView.StartNewGameIfEnoughGlyph();
			}
			if (SelectedCity != null && !SelectedCity.WorldMapCityView.CanStartGame && !GenericConsent.IsOpen && SelectedCity.CityDefinition.HasLinkedDLC && !SelectedCity.IsLinkedDLCOwned)
			{
				GameConfigurationsView.OpenSelectedCityLinkedDLCStorePage();
			}
			return;
		}
		if (InputManager.GetButtonDown(107))
		{
			SelectNextCity();
			return;
		}
		if (InputManager.GetButtonDown(108))
		{
			SelectPreviousCity();
			return;
		}
		if (InputManager.GetButtonDown(141))
		{
			SelectStoryMapCity();
			return;
		}
		if (InputManager.GetButtonDown(142))
		{
			SelectDLCMapCity();
			return;
		}
		if (InputManager.IsLastControllerJoystick && InputManager.GetButtonDown(79))
		{
			foreach (WorldMapCity city in Cities)
			{
				if (city.WorldMapCityView.IsHovered)
				{
					SelectCity(city);
					break;
				}
			}
			return;
		}
		if (InputManager.GetButtonDown(144) && TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.CanFocusUsingJoystick)
		{
			TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.FocusUsingJoystick();
		}
	}

	public void DeserializeCities(int saveVersion, ISerializedData container = null)
	{
		Cities = new List<WorldMapCity>();
		SerializedCities serializedCities = container as SerializedCities;
		GenerateInitialCities(serializedCities, saveVersion);
		LastRunInfo = serializedCities?.LastRunInfo;
		if (container == null)
		{
			selectedCity = Cities.FirstOrDefault((WorldMapCity o) => MetaUpgradesManager.IsThisCityUnlockedByDefault(o.CityDefinition.Id));
			TPSingleton<WorldMapCityManager>.Instance.Log("No city were selected on deserialization, probably due to new AppSave creation, selecting " + selectedCity.CityDefinition.Id + " by default.", CLogLevel.DETAILED);
		}
		if (selectedCity == null)
		{
			selectedCity = Cities.FirstOrDefault();
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
	}

	public ISerializedData Serialize()
	{
		SerializedCities serializedCities = new SerializedCities
		{
			SelectedCityId = selectedCity?.CityDefinition.Id,
			LastRunInfo = LastRunInfo
		};
		foreach (WorldMapCity city in Cities)
		{
			serializedCities.Cities.Add(city.Serialize() as SerializedCity);
		}
		return serializedCities;
	}

	[DevConsoleCommand("ShowAllCities")]
	public static void ShowAllCities()
	{
		foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
		{
			city.WorldMapCityView.gameObject.SetActive(value: true);
		}
	}
}
