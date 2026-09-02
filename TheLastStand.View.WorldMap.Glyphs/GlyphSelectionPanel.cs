using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Database.Meta;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Helpers;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tutorial;
using TheLastStand.View.WorldMap.Apocalypse;
using TheLastStand.View.WorldMap.Glyphs.Feedback;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphSelectionPanel : OraculumHub<GlyphSelectionPanel>
{
	[SerializeField]
	private float scrollSensitivity = 0.1f;

	[SerializeField]
	private Material glyphAvailableMaterial;

	[SerializeField]
	private Material glyphSelectedMaterial;

	[SerializeField]
	private GlyphDisplay glyphDisplayPrefab;

	[SerializeField]
	private SelectedGlyphDisplay selectedGlyphDisplayPrefab;

	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private GlyphsHeader glyphsHeader;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private TutorialPopup tutorialPopup;

	[SerializeField]
	private BetterToggle customModeToggle;

	[SerializeField]
	private RectTransform selectedGlyphDisplaysContainer;

	[SerializeField]
	private ScrollRect selectedGlyphsScrollRect;

	[SerializeField]
	private GameObject selectedGlyphsLeftButton;

	[SerializeField]
	private GameObject selectedGlyphsRightButton;

	[SerializeField]
	private RectTransform glyphDisplaysContainer;

	[SerializeField]
	private RectTransform glyphsViewport;

	[SerializeField]
	private Scrollbar glyphsScrollbar;

	[SerializeField]
	private GameObject apocalypsePanelButton;

	[SerializeField]
	private List<AudioClip> activateClips;

	[SerializeField]
	private List<AudioClip> deactivateClips;

	[SerializeField]
	private List<AudioClip> errorClips;

	[SerializeField]
	private AudioSource audioSourceTemplate;

	[SerializeField]
	[Min(1f)]
	private int audioSourcesCount = 3;

	[SerializeField]
	private LayoutNavigationInitializer glyphsGridNavigationInitializer;

	[SerializeField]
	private float waitBetweenUnlockFeedback = 0.1f;

	private readonly List<GlyphDisplay> glyphDisplays = new List<GlyphDisplay>();

	private readonly List<SelectedGlyphDisplay> selectedGlyphDisplays = new List<SelectedGlyphDisplay>();

	private AudioSource[] audioSources;

	private int nextAudioSourceIndex;

	public bool AnySelectedGlyphDestroying { get; private set; }

	public Material GlyphAvailableMaterial => glyphAvailableMaterial;

	public Material GlyphSelectedMaterial => glyphSelectedMaterial;

	public void OnSelectedGlyphsLeftButtonClick()
	{
		selectedGlyphsScrollRect.OnScroll(new PointerEventData(null)
		{
			scrollDelta = new Vector2(-1f, 0f)
		});
	}

	public void OnSelectedGlyphsRightButtonClick()
	{
		selectedGlyphsScrollRect.OnScroll(new PointerEventData(null)
		{
			scrollDelta = new Vector2(1f, 0f)
		});
	}

	public void OnGlyphsTopButtonClick()
	{
		glyphsScrollbar.value -= scrollSensitivity;
	}

	public void OnGlyphsBotButtonClick()
	{
		glyphsScrollbar.value += scrollSensitivity;
	}

	public AudioSource GetNextAudioSource()
	{
		return audioSources[nextAudioSourceIndex++ % audioSources.Length];
	}

	public void RefreshAllGlyphs()
	{
		string[] source = (GlyphManager.Debug_GlyphsForceUnlock ? Array.Empty<string>() : TPSingleton<MetaUpgradesManager>.Instance.GetLockedGlyphIds());
		List<GlyphDefinition> list = new List<GlyphDefinition>();
		List<GlyphDefinition> list2 = new List<GlyphDefinition>();
		HashSet<UnlockedGlyphFeedback> hashSet = new HashSet<UnlockedGlyphFeedback>();
		int num = 0;
		bool flag = TPSingleton<WorldMapCityManager>.Instance.IsCurrentSelectedCityLastPlayedCity();
		foreach (KeyValuePair<string, GlyphDefinition> glyphDefinition in GlyphDatabase.GlyphDefinitions)
		{
			if (source.Contains(glyphDefinition.Key))
			{
				list2.Add(glyphDefinition.Value);
				continue;
			}
			if (!glyphDefinition.Value.IsCustom)
			{
				list.Add(glyphDefinition.Value);
				continue;
			}
			bool flag2 = TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Contains(glyphDefinition.Value.Id);
			bool flag3 = flag && TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Contains(glyphDefinition.Value.Id);
			Dictionary<string, int> value;
			bool isCompleted = TPSingleton<GlyphManager>.Instance.MaxApoPassedByCityByGlyph.TryGetValue(glyphDefinition.Value.Id, out value) && value.ContainsKey(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id);
			glyphDisplays[num].Init(glyphDefinition.Value, locked: false, isCompleted, TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled && flag3, flag2);
			glyphDisplays[num].SetSelection(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(glyphDefinition.Value));
			if (flag2)
			{
				hashSet.Add(glyphDisplays[num].UnlockFeedback);
			}
			num++;
		}
		foreach (GlyphDefinition item in list)
		{
			bool flag4 = TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Contains(item.Id);
			bool triggerCompletionAnimation = flag && TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Contains(item.Id);
			Dictionary<string, int> value2;
			bool isCompleted2 = TPSingleton<GlyphManager>.Instance.MaxApoPassedByCityByGlyph.TryGetValue(item.Id, out value2) && value2.ContainsKey(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id);
			glyphDisplays[num].Init(item, locked: false, isCompleted2, triggerCompletionAnimation, flag4);
			glyphDisplays[num].SetSelection(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(item));
			if (flag4)
			{
				hashSet.Add(glyphDisplays[num].UnlockFeedback);
			}
			num++;
		}
		foreach (GlyphDefinition item2 in list2)
		{
			glyphDisplays[num].Init(item2, locked: true, isCompleted: false, triggerCompletionAnimation: false, newlyUnlocked: false);
			glyphDisplays[num].SetSelection(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(item2));
			num++;
		}
		if (TPSingleton<WorldMapCityManager>.Instance.LastRunInfo != null)
		{
			TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Clear();
		}
		TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Clear();
		StartCoroutine(TriggerUnlockAnimations(hashSet));
	}

	public void OnGoToApocalypsePanelButtonClick()
	{
		OraculumHub<GlyphSelectionPanel>.Display(show: false, null, isShortDisplay: true, delegate
		{
			OraculumHub<ApocalypseSelectionPanel>.Display(show: true, null, isShortDisplay: true);
		});
	}

	public void OnSelectedGlyphJoystickSubmit(SelectedGlyphDisplay selectedGlyphDisplay)
	{
		if (selectedGlyphDisplays.Count == 1)
		{
			GlyphDisplay glyphDisplay = glyphDisplays.FirstOrDefault((GlyphDisplay o) => o.gameObject.activeSelf);
			EventSystem.current.SetSelectedGameObject((glyphDisplay != null) ? glyphDisplay.gameObject : customModeToggle.gameObject);
			RefreshJoystickNavigation();
			return;
		}
		for (int num = 0; num < selectedGlyphDisplays.Count; num++)
		{
			if (selectedGlyphDisplays[num] == selectedGlyphDisplay)
			{
				SelectedGlyphDisplay selectedGlyphDisplay2 = (((InputManager.JoystickConfig.HUDNavigation.SelectNextOmenOnUnselect && num < selectedGlyphDisplays.Count - 1) || num == 0) ? selectedGlyphDisplays[num + 1] : selectedGlyphDisplays[num - 1]);
				EventSystem.current.SetSelectedGameObject(selectedGlyphDisplay2.gameObject);
				break;
			}
		}
	}

	public void OnSelectedGlyphJoystickSelect(SelectedGlyphDisplay selectedGlyphDisplay)
	{
		Rect worldRect = ((RectTransform)selectedGlyphDisplay.transform).GetWorldRect();
		Rect worldRect2 = selectedGlyphsScrollRect.viewport.GetWorldRect();
		bool flag = false;
		while (worldRect.center.x > worldRect2.max.x)
		{
			OnSelectedGlyphsRightButtonClick();
			worldRect = ((RectTransform)selectedGlyphDisplay.transform).GetWorldRect();
			flag = true;
		}
		while (worldRect.center.x < worldRect2.min.x)
		{
			OnSelectedGlyphsLeftButtonClick();
			worldRect = ((RectTransform)selectedGlyphDisplay.transform).GetWorldRect();
			flag = true;
		}
		if (flag)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
		}
	}

	public void PlayErrorClip()
	{
		SoundManager.PlayAudioClip(GetNextAudioSource(), errorClips.RandomElement());
	}

	public bool SelectedGlyphDisplayExists(GlyphDefinition glyphDefinition)
	{
		return selectedGlyphDisplays.Any((SelectedGlyphDisplay x) => x.GlyphDefinition == glyphDefinition);
	}

	public void SelectGlyph(GlyphDisplay glyphDisplay, bool updateCity = true, bool playFeedback = true)
	{
		if (updateCity)
		{
			TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityController.AddGlyph(glyphDisplay.GlyphDefinition);
			glyphsHeader.RefreshCityPoints();
		}
		glyphDisplay.SetSelection(selected: true);
		InstantiateSelectedGlyph(glyphDisplay.GlyphDefinition);
		RefreshSelectedGlyphsButtons();
		if (playFeedback)
		{
			glyphDisplay.TriggerSelectionFeedback();
			PlayGlyphClip(selected: true);
		}
	}

	public void UnselectGlyph(GlyphDisplay glyphDisplay, bool updateCity = true, bool playFeedback = true)
	{
		SelectedGlyphDisplay selectedGlyphDisplay = selectedGlyphDisplays.Find((SelectedGlyphDisplay o) => o.GlyphDefinition == glyphDisplay.GlyphDefinition);
		UnselectGlyph(selectedGlyphDisplay, glyphDisplay, updateCity, playFeedback);
	}

	public void UnselectGlyph(SelectedGlyphDisplay selectedGlyphDisplay, GlyphDisplay glyphDisplay = null, bool updateCity = true, bool playFeedback = true)
	{
		if (updateCity)
		{
			TPSingleton<WorldMapCityManager>.Instance.SelectedCity.WorldMapCityController.RemoveGlyph(selectedGlyphDisplay.GlyphDefinition);
			glyphsHeader.RefreshCityPoints();
		}
		if ((object)glyphDisplay == null)
		{
			glyphDisplay = glyphDisplays.Find((GlyphDisplay o) => o.GlyphDefinition == selectedGlyphDisplay.GlyphDefinition);
		}
		glyphDisplay.SetSelection(selected: false);
		DestroySelectedGlyph(selectedGlyphDisplay, !playFeedback);
		if (playFeedback)
		{
			glyphDisplay.TriggerSelectionFeedback();
			PlayGlyphClip(selected: false);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InstantiateGlyphs();
		if (tutorialPopup != null)
		{
			tutorialPopup.SetSelectableAfterClose(glyphDisplays.FirstOrDefault((GlyphDisplay o) => !o.GlyphDefinition.IsCustom)?.JoystickSelectable);
		}
		customModeToggle.onValueChanged.AddListener(ToggleCustomMode);
	}

	protected override void OnFadeToBlackComplete()
	{
		base.OnFadeToBlackComplete();
		if (base.Displayed)
		{
			RefreshContent();
			glyphsGridNavigationInitializer.InitNavigation(reset: true);
			if (TPSingleton<WorldMapCityManager>.Instance.IsCurrentSelectedCityLastPlayedCity())
			{
				TPSingleton<WorldMapCityManager>.Instance.LastRunInfo.NewlyCompletedGlyphsId.Clear();
				TPSingleton<GlyphManager>.Instance.NewlyUnlockedGlyphIds.Clear();
			}
			RefreshJoystickNavigation();
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnGlyphsPanelOpen);
		}
		else
		{
			DeactivateCustomModeIfNeeded();
			TPSingleton<GameConfigurationsView>.Instance.ApocalypseSelectionPreview.Refresh();
			TPSingleton<GameConfigurationsView>.Instance.GlyphSelectionPreview.Refresh();
			TPSingleton<GameConfigurationsView>.Instance.RefreshBoxSize();
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<GameConfigurationsView>.Instance.JoystickSelectPanel();
			}
		}
	}

	protected override void OnHubEnter(Action onDisplayed = null)
	{
		base.OnHubEnter(onDisplayed);
		if (InputManager.IsLastControllerJoystick)
		{
			GlyphDisplay glyphDisplay = glyphDisplays.FirstOrDefault((GlyphDisplay o) => o.gameObject.activeSelf);
			if (glyphDisplay != null)
			{
				EventSystem.current.SetSelectedGameObject(glyphDisplay.gameObject);
			}
		}
	}

	protected override void OnFadeToBlackStarts()
	{
		base.OnFadeToBlackStarts();
		WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.GLYPHSELECTION);
		if (!base.Displayed)
		{
			TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<SoundManager>.Instance.WorldMapMusic);
			for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeIn();
			}
		}
		else
		{
			for (int j = 0; j < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; j++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[j].FadeOut();
			}
		}
	}

	protected override void OnHubExit()
	{
		base.OnHubExit();
		TPSingleton<GlyphManager>.Instance.GlyphTooltip.Hide();
		WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.FOCUSED);
	}

	protected override void Start()
	{
		leaveButton.onClick.AddListener(delegate
		{
			OraculumHub<GlyphSelectionPanel>.Display(show: false);
		});
		CanvasHelper.ScaleCanvasTowards720P(canvasScaler);
		InitAudio();
		base.Start();
	}

	private void DeactivateCustomModeIfNeeded()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints() == 0 && !TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Any((GlyphDefinition glyph) => glyph.IsCustom))
		{
			TPSingleton<GlyphManager>.Instance.ToggleCustomMode(toggle: false);
			glyphsGridNavigationInitializer.InitNavigation(reset: true);
			RefreshJoystickNavigation();
		}
	}

	private void DestroySelectedGlyph(SelectedGlyphDisplay selectedGlyphDisplay, bool instantly = false)
	{
		AnySelectedGlyphDestroying = true;
		selectedGlyphDisplays.Remove(selectedGlyphDisplay);
		RefreshSelectedGlyphsNavigation();
		StartCoroutine(selectedGlyphDisplay.DestroySelf(RefreshSelectedGlyphsButtonsAfterAFrame, instantly));
	}

	private void InitAudio()
	{
		ListExtensions.Shuffle(activateClips);
		ListExtensions.Shuffle(deactivateClips);
		audioSources = new AudioSource[audioSourcesCount];
		audioSources[0] = audioSourceTemplate;
		for (int i = 1; i < audioSources.Length; i++)
		{
			AudioSource audioSource = UnityEngine.Object.Instantiate(audioSourceTemplate, audioSourceTemplate.transform.parent);
			audioSources[i] = audioSource;
		}
	}

	private void InstantiateGlyph(GlyphDefinition glyphDefinition, bool isLocked)
	{
		bool num = TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Contains(glyphDefinition);
		GlyphDisplay glyphDisplay = UnityEngine.Object.Instantiate(glyphDisplayPrefab, glyphDisplaysContainer);
		glyphDisplay.Init(glyphDefinition, isLocked, isCompleted: false, triggerCompletionAnimation: false, newlyUnlocked: false);
		glyphDisplay.GetComponent<JoystickSelectable>().AddListenerOnSelect(delegate
		{
			OnJoystickSelect(glyphDisplay.transform as RectTransform);
		});
		glyphDisplays.Add(glyphDisplay);
		if (num)
		{
			SelectGlyph(glyphDisplay, updateCity: false, playFeedback: false);
		}
	}

	private void InstantiateGlyphs()
	{
		string[] lockedGlyphIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedGlyphIds();
		List<GlyphDefinition> list = new List<GlyphDefinition>();
		List<GlyphDefinition> list2 = new List<GlyphDefinition>();
		foreach (KeyValuePair<string, GlyphDefinition> glyphDefinition in GlyphDatabase.GlyphDefinitions)
		{
			if (lockedGlyphIds.Contains(glyphDefinition.Key))
			{
				list2.Add(glyphDefinition.Value);
			}
			else if (!glyphDefinition.Value.IsCustom)
			{
				list.Add(glyphDefinition.Value);
			}
			else
			{
				InstantiateGlyph(glyphDefinition.Value, isLocked: false);
			}
		}
		foreach (GlyphDefinition item in list)
		{
			InstantiateGlyph(item, isLocked: false);
		}
		foreach (GlyphDefinition item2 in list2)
		{
			InstantiateGlyph(item2, isLocked: true);
		}
	}

	private void InstantiateSelectedGlyph(GlyphDefinition glyphDefinition)
	{
		SelectedGlyphDisplay selectedGlyphDisplay = UnityEngine.Object.Instantiate(selectedGlyphDisplayPrefab, selectedGlyphDisplaysContainer);
		selectedGlyphDisplay.Init(glyphDefinition);
		selectedGlyphDisplays.Add(selectedGlyphDisplay);
		RefreshSelectedGlyphsNavigation();
	}

	private void OnJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(source, glyphsViewport, glyphsScrollbar, 0.02f, 0f, 0.1f);
	}

	private void PlayGlyphClip(bool selected)
	{
		AudioClip audioClip = GetNextClip(selected ? activateClips : deactivateClips);
		if (audioClip == null)
		{
			CLoggerManager.Log("Could not find a valid glyph audio clip!");
		}
		else
		{
			SoundManager.PlayAudioClip(GetNextAudioSource(), audioClip);
		}
		static AudioClip GetNextClip(List<AudioClip> list)
		{
			AudioClip audioClip2 = list[0];
			int index = UnityEngine.Random.Range(2, list.Count);
			list.RemoveAt(0);
			list.Insert(index, audioClip2);
			return audioClip2;
		}
	}

	private void RefreshContent()
	{
		WorldMapCity selectedCity = TPSingleton<WorldMapCityManager>.Instance.SelectedCity;
		glyphsHeader.RefreshCityName();
		for (int num = selectedGlyphDisplays.Count - 1; num >= 0; num--)
		{
			UnselectGlyph(selectedGlyphDisplays[num], null, updateCity: false, playFeedback: false);
		}
		List<GlyphDefinition> selectedGlyphs = selectedCity.GlyphsConfig.SelectedGlyphs;
		int i = 0;
		while (i < selectedGlyphs.Count)
		{
			GlyphDisplay glyphDisplay = glyphDisplays.Find((GlyphDisplay o) => o.GlyphDefinition == selectedGlyphs[i]);
			SelectGlyph(glyphDisplay, updateCity: false, playFeedback: false);
			int num2 = i + 1;
			i = num2;
		}
		customModeToggle.isOn = selectedCity.GlyphsConfig.CustomModeEnabled;
		glyphsHeader.InitCityPointImages();
		glyphsHeader.RefreshCityPoints();
		RefreshCustomMode();
		RefreshSelectedGlyphsButtons();
		RefreshAllGlyphs();
		RefreshApocalypseButton();
	}

	private void RefreshCustomMode()
	{
		bool customModeEnabled = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled;
		foreach (GlyphDisplay glyphDisplay in glyphDisplays)
		{
			if (!glyphDisplay.GlyphDefinition.IsCustom)
			{
				break;
			}
			glyphDisplay.gameObject.SetActive(customModeEnabled);
		}
	}

	private void RefreshSelectedGlyphsButtons()
	{
		StartCoroutine(RefreshSelectedGlyphsButtonsAfterAFrame());
	}

	private IEnumerator RefreshSelectedGlyphsButtonsAfterAFrame()
	{
		yield return null;
		AnySelectedGlyphDestroying = false;
		bool flag = selectedGlyphsScrollRect.viewport.rect.width < selectedGlyphsScrollRect.content.rect.width;
		selectedGlyphsLeftButton.SetActive(flag);
		selectedGlyphsRightButton.SetActive(flag);
		if (flag)
		{
			OnSelectedGlyphsRightButtonClick();
		}
		if (!base.OpeningOrClosing)
		{
			RefreshJoystickNavigation();
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ForcePositionUpdate();
		}
	}

	private void RefreshApocalypseButton()
	{
		apocalypsePanelButton.SetActive(ApocalypseManager.IsApocalypseUnlocked);
	}

	private void ToggleCustomMode(bool toggle)
	{
		if (!toggle)
		{
			UnselectCustomGlyphs();
		}
		TPSingleton<GlyphManager>.Instance.ToggleCustomMode(toggle);
		RefreshContent();
		glyphsGridNavigationInitializer.InitNavigation(reset: true);
		RefreshJoystickNavigation();
	}

	private IEnumerator TriggerUnlockAnimations(HashSet<UnlockedGlyphFeedback> feedback)
	{
		foreach (UnlockedGlyphFeedback unlockedGlyphFeedback in feedback)
		{
			yield return SharedYields.WaitForSeconds(waitBetweenUnlockFeedback);
			unlockedGlyphFeedback.TriggerUnlockAnimation();
		}
	}

	private void UnselectCustomGlyphs()
	{
		for (int num = selectedGlyphDisplays.Count - 1; num >= 0; num--)
		{
			if (selectedGlyphDisplays[num].GlyphDefinition.IsCustom)
			{
				UnselectGlyph(selectedGlyphDisplays[num], null, updateCity: true, playFeedback: false);
			}
		}
	}

	private void RefreshSelectedGlyphsNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < selectedGlyphDisplays.Count; i++)
		{
			SelectedGlyphDisplay selectedGlyphDisplay = selectedGlyphDisplays[i];
			if (!selectedGlyphDisplay.DestroyingSelf && !(selectedGlyphDisplay.JoystickSelectable == null))
			{
				list.Add(selectedGlyphDisplay.JoystickSelectable);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Selectable selectable = list[j];
			selectable.SetMode(Navigation.Mode.Explicit);
			selectable.SetSelectOnLeft(null);
			selectable.SetSelectOnRight(null);
			if (j > 0)
			{
				selectable.SetSelectOnLeft(list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				selectable.SetSelectOnRight(list[j + 1]);
			}
		}
	}

	private void RefreshJoystickNavigation()
	{
		customModeToggle.SetMode(Navigation.Mode.Explicit);
		customModeToggle.ClearNavigation();
		Selectable selectable = glyphDisplays.FirstOrDefault((GlyphDisplay o) => o.gameObject.activeSelf)?.JoystickSelectable;
		Selectable selectable2 = selectedGlyphDisplays.FirstOrDefault((SelectedGlyphDisplay o) => o.gameObject.activeSelf && !o.DestroyingSelf)?.JoystickSelectable;
		for (int num = 0; num < selectedGlyphDisplays.Count; num++)
		{
			selectedGlyphDisplays[num].JoystickSelectable.SetSelectOnDown(selectable);
			selectedGlyphDisplays[num].JoystickSelectable.SetSelectOnUp(customModeToggle);
		}
		for (int num2 = 0; num2 < glyphDisplays.Count; num2++)
		{
			Selectable joystickSelectable = glyphDisplays[num2].JoystickSelectable;
			if (!(joystickSelectable.navigation.selectOnUp != null) || !(joystickSelectable.navigation.selectOnUp != customModeToggle))
			{
				joystickSelectable.SetSelectOnUp((selectable2 != null) ? selectable2 : customModeToggle);
			}
		}
		customModeToggle.SetSelectOnDown((selectable2 != null) ? selectable2 : selectable);
		customModeToggle.SetSelectOnRight(closeButton);
	}

	private void Update()
	{
		if (base.Displayed && !base.OpeningOrClosing && InputManager.IsLastControllerJoystick && InputManager.GetButtonDown(148) && apocalypsePanelButton.activeSelf)
		{
			OnGoToApocalypsePanelButtonClick();
		}
	}
}
