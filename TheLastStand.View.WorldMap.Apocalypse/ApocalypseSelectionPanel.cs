using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using TMPro;
using TPLib;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Database;
using TheLastStand.Definition;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Helpers;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tutorial;
using TheLastStand.View.WorldMap.Glyphs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseSelectionPanel : OraculumHub<ApocalypseSelectionPanel>
{
	public enum E_Sfx
	{
		None = -1,
		ModifierOff,
		ModifierOn,
		ModifierStepDecremented,
		ModifierStepIncremented
	}

	[SerializeField]
	private float scrollSensitivity = 0.1f;

	[SerializeField]
	private ApocalypseModifierDisplay apocalypseModifierDisplayPrefab;

	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private ApocalypseHeader apocalypseHeader;

	[SerializeField]
	private GridLayoutGroup gridLayoutGroup;

	[SerializeField]
	private ApocalypseModifierStepTooltip modifierStepTooltip;

	[SerializeField]
	private TutorialPopup tutorialPopup;

	[SerializeField]
	private RectTransform apocalypseModifiersDisplaysContainer;

	[SerializeField]
	private RectTransform apocalypseModifiersViewport;

	[SerializeField]
	private Scrollbar apocalypseModifiersScrollbar;

	[SerializeField]
	private ApocalypseCodeSharingView codeSharingView;

	[SerializeField]
	private TMP_Dropdown filtersDropDown;

	[SerializeField]
	private ApocalypseSelectionPanelSorter sorterByCompletion;

	[SerializeField]
	private Image newlyUnlockedModifiersNotification;

	[SerializeField]
	private Material newlyUnlockedNotificationMaterial;

	[SerializeField]
	private AudioClip flameActivationClip;

	[SerializeField]
	private AudioClip smallFlameActivationClip;

	[SerializeField]
	private AudioClip gaugeFullClip;

	[SerializeField]
	private AudioClip modifierOffClip;

	[SerializeField]
	private AudioClip modifierOnClip;

	[SerializeField]
	private AudioClip modifierStepDecrementedClip;

	[SerializeField]
	private AudioClip modifierStepIncrementedClip;

	[SerializeField]
	private AudioSource audioSourceTemplate;

	[SerializeField]
	[Min(1f)]
	private int audioSourcesCount = 3;

	[SerializeField]
	private LayoutNavigationInitializer gridNavigationInitializer;

	[SerializeField]
	private HUDJoystickDynamicTarget joystickDynamicTarget;

	[SerializeField]
	private HUDJoystickDynamicTarget joystickDynamicTargetGaugeDown;

	[SerializeField]
	private HUDJoystickSimpleTarget topPanelJoystickSimpleTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget scrollViewJoystickSimpleTarget;

	[SerializeField]
	private HUDJoystickSimpleTarget bottomPanelJoystickSimpleTarget;

	[SerializeField]
	private Selectable[] bottomPanelJoystickSelectables;

	private readonly List<ApocalypseModifierDisplay> apocalypseModifierDisplays = new List<ApocalypseModifierDisplay>();

	private AudioSource[] audioSources;

	private List<FilterDefinition> availableFilterDefinitions = new List<FilterDefinition>();

	private int selectedFilterIndex;

	private bool isSortingByCompletionEnabled;

	private ApocalypseModifierDisplay currentModifierDisplayForJoystick;

	private List<ApocalypseModifierDefinition> lockedModifiersDefinitions = new List<ApocalypseModifierDefinition>();

	private List<ApocalypseModifierDefinition> unlockedModifiersDefinitions = new List<ApocalypseModifierDefinition>();

	private Dictionary<string, List<ApocalypseModifierDefinition>> unlockedModifiersByTierId = new Dictionary<string, List<ApocalypseModifierDefinition>>();

	private Dictionary<string, int> highestCompletedStepIndexByModifierId = new Dictionary<string, int>();

	public ApocalypseModifierStepTooltip ApocalypseModifierStepTooltip => modifierStepTooltip;

	public ApocalypseCodeSharingView CodeSharingView => codeSharingView;

	public TMP_Dropdown FiltersDropDown => filtersDropDown;

	public bool IsFiltersDropDownOpen => filtersDropDown.IsExpanded;

	public bool IsFiltersDropDownSelected => EventSystem.current.currentSelectedGameObject == filtersDropDown.gameObject;

	public Material NewlyUnlockedNotificationMaterial => newlyUnlockedNotificationMaterial;

	public void ApplyCodeSharing(ApocalypseCodeGenerator.ApocalypseCodeDecodingData codeDecodingData)
	{
		if (codeDecodingData.Success)
		{
			ApocalypseManager.CurrentApocalypse.ApocalypseController.ClearAllSelectedModifierSteps(computeLevel: false);
			ApocalypseManager.CurrentApocalypse.ApocalypseController.SetSelectedModifierSteps(codeDecodingData.ModifierStepDefinitions, computeLevel: true, computeEffects: false);
			RefreshContent(refreshModifiersOrder: false);
		}
	}

	public void OnScrollbarTopButtonClick()
	{
		apocalypseModifiersScrollbar.value = Mathf.Clamp01(apocalypseModifiersScrollbar.value - scrollSensitivity);
	}

	public void OnScrollbarBottomButtonClick()
	{
		apocalypseModifiersScrollbar.value = Mathf.Clamp01(apocalypseModifiersScrollbar.value + scrollSensitivity);
	}

	public void PlayBigFlameSfx()
	{
		PlayAudioClip(flameActivationClip);
	}

	public void PlaySmallFlameSfx()
	{
		PlayAudioClip(smallFlameActivationClip);
	}

	public void PlayGaugeFullSfx()
	{
		PlayAudioClip(gaugeFullClip);
	}

	public void PlayModifierOffSfx()
	{
		PlayAudioClip(modifierOffClip);
	}

	public void PlayModifierOnSfx()
	{
		PlayAudioClip(modifierOnClip);
	}

	public void PlayModifierStepDecrementedSfx()
	{
		PlayAudioClip(modifierStepDecrementedClip);
	}

	public void PlayModifierStepIncrementedSfx()
	{
		PlayAudioClip(modifierStepIncrementedClip);
	}

	public AudioSource GetNextAudioSource()
	{
		AudioSource[] array = audioSources;
		foreach (AudioSource audioSource in array)
		{
			if (!audioSource.isPlaying)
			{
				return audioSource;
			}
		}
		return audioSources[^1];
	}

	public void OnBottomPanelDeselected()
	{
		if (IsFiltersDropDownOpen)
		{
			filtersDropDown.Hide();
		}
	}

	public void OnClearSelectedModifiersButtonClick()
	{
		List<ApocalypseModifierDisplay> allModifierDisplaysVisible = GetAllModifierDisplaysVisible();
		bool flag = false;
		foreach (ApocalypseModifierDisplay item in allModifierDisplaysVisible)
		{
			if (item.SelectedModifierStepDefinition != null)
			{
				flag = true;
				ApocalypseManager.CurrentApocalypse.ApocalypseController.RemoveSelectedModifierStep(item.SelectedModifierStepDefinition, computeLevel: false);
			}
		}
		if (flag)
		{
			ApocalypseManager.CurrentApocalypse.ApocalypseController.ComputeAllData(computeLevel: true, computeEffects: false);
			RefreshContent(refreshModifiersOrder: false);
		}
	}

	public void OnFilterChangedInDropdownList()
	{
		selectedFilterIndex = filtersDropDown.value;
		RefreshSelectedFilter();
		RefreshAllModifiers(refreshModifiersOrder: false);
	}

	public void OnGoToGlyphsPanelButtonClick()
	{
		OraculumHub<ApocalypseSelectionPanel>.Display(show: false, null, isShortDisplay: true, delegate
		{
			OraculumHub<GlyphSelectionPanel>.Display(show: true, null, isShortDisplay: true);
		});
	}

	public void OnSelectAllModifiersButtonClick()
	{
		bool flag = false;
		foreach (ApocalypseModifierDisplay item in GetAllModifierDisplaysVisible())
		{
			if (item.ApocalypseModifierDefinition != null)
			{
				ApocalypseModifierStepDefinition apocalypseModifierStepDefinition = item.ApocalypseModifierDefinition.StepDefinitions[^1];
				if (item.SelectedModifierStepDefinition != apocalypseModifierStepDefinition && ApocalypseManager.IsModifierUnlocked(item.ApocalypseModifierDefinition))
				{
					ApocalypseManager.CurrentApocalypse.ApocalypseController.AddSelectedModifierStep(apocalypseModifierStepDefinition, computeLevel: false);
					flag = true;
				}
			}
		}
		if (flag)
		{
			ApocalypseManager.CurrentApocalypse.ApocalypseController.ComputeAllData(computeLevel: true, computeEffects: false);
			RefreshContent(refreshModifiersOrder: false);
		}
	}

	public void OnSortByCompletionToggled(bool isToggled)
	{
		isSortingByCompletionEnabled = isToggled;
		RefreshModifiersSorting();
		RefreshJoystickNavigation();
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick && EventSystem.current.currentSelectedGameObject == currentModifierDisplayForJoystick.gameObject)
		{
			SelectDefaultJoystickSelectable();
		}
	}

	public void RefreshAllModifiers(bool refreshModifiersOrder = true, bool refreshJoystickNavigation = true)
	{
		if (!refreshModifiersOrder)
		{
			foreach (ApocalypseModifierDisplay apocalypseModifierDisplay in apocalypseModifierDisplays)
			{
				apocalypseModifierDisplay.gameObject.SetActive(IsApocalypseModifierDisplayVisible(apocalypseModifierDisplay));
				apocalypseModifierDisplay.RetrieveSelectedStep();
				apocalypseModifierDisplay.Refresh();
			}
		}
		else
		{
			RefreshModifiersSorting();
		}
		if (refreshJoystickNavigation)
		{
			RefreshJoystickNavigation();
		}
	}

	public void RefreshNewlyUnlockedModifiersNotification()
	{
		bool flag = false;
		foreach (ApocalypseModifierDefinition value in ApocalypseDatabase.ModifierDefinitions.Values)
		{
			if (ApocalypseManager.IsModifierUnlocked(value) && ApocalypseManager.ModifiersUnlockSeen.ContainsKey(value.Id) && !ApocalypseManager.ModifiersUnlockSeen[value.Id])
			{
				flag = true;
			}
		}
		newlyUnlockedModifiersNotification.enabled = flag;
		newlyUnlockedModifiersNotification.material = (flag ? newlyUnlockedNotificationMaterial : null);
	}

	public void SelectApocalypseModifierStep(ApocalypseModifierStepDefinition modifierStepDefinition, bool updateApocalypse = true, bool playFeedback = true, E_Sfx sfx = E_Sfx.None)
	{
		if (updateApocalypse)
		{
			ApocalypseManager.CurrentApocalypse.ApocalypseController.AddSelectedModifierStep(modifierStepDefinition);
		}
		RefreshHeader();
		if (playFeedback && sfx != E_Sfx.None)
		{
			PlayAudioClip(sfx);
		}
	}

	public void SelectDefaultJoystickSelectable()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickDynamicTarget.GetSelectionInfo());
	}

	public void SelectDefaultJoystickSelectableAfterAFrame()
	{
		StartCoroutine(SelectDefaultJoystickSelectableAfterAFrameCoroutine());
	}

	public void SelectBottomPanelAfterAFrame()
	{
		StartCoroutine(SelectBottomPanelAfterAFrameCoroutine());
	}

	public void SetCurrentModifierDisplayForJoystick(ApocalypseModifierDisplay modifierDisplay)
	{
		currentModifierDisplayForJoystick = modifierDisplay;
	}

	public void UnselectApocalypseModifierStep(ApocalypseModifierStepDefinition modifierStepDefinition, bool updateApocalypse = true, bool playFeedback = true)
	{
		if (updateApocalypse)
		{
			ApocalypseManager.CurrentApocalypse.ApocalypseController.RemoveSelectedModifierStep(modifierStepDefinition);
		}
		RefreshHeader();
		if (playFeedback)
		{
			PlayModifierOffSfx();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		sorterByCompletion.OnToggled += OnSortByCompletionToggled;
		InstantiateApocalypseModifiers();
		InitializeFilters();
		_ = tutorialPopup != null;
	}

	protected override AudioClip GetMusicClip()
	{
		return TPSingleton<SoundManager>.Instance.MetaShopsNoGoddessesMusic;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		sorterByCompletion.OnToggled -= OnSortByCompletionToggled;
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	protected override void OnFadeToBlackComplete()
	{
		base.OnFadeToBlackComplete();
		if (base.Displayed)
		{
			codeSharingView.RefreshVisibleState();
			InitModifiersData();
			ResetFilter();
			RefreshContent(refreshModifiersOrder: true, refreshJoystickNavigation: false, useTween: false);
			return;
		}
		apocalypseHeader.PauseAnimations();
		TPSingleton<GameConfigurationsView>.Instance.ApocalypseSelectionPreview.Refresh();
		TPSingleton<GameConfigurationsView>.Instance.GlyphSelectionPreview.Refresh();
		TPSingleton<GameConfigurationsView>.Instance.RefreshBoxSize();
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<GameConfigurationsView>.Instance.JoystickSelectPanel();
		}
	}

	protected override void OnHubEnter(Action onDisplayed = null)
	{
		base.OnHubEnter(onDisplayed);
		InitJoystickNavigation();
		RefreshJoystickNavigation();
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			SelectDefaultJoystickSelectable();
		}
	}

	protected override void OnFadeToBlackStarts()
	{
		base.OnFadeToBlackStarts();
		WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.APOCALYPSE_SELECTION);
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
			apocalypseHeader.ContinueAnimations();
			for (int j = 0; j < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; j++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[j].FadeOut();
			}
		}
	}

	protected override void OnHubExit()
	{
		base.OnHubExit();
		if (modifierStepTooltip != null)
		{
			modifierStepTooltip.Hide();
		}
		WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.FOCUSED);
	}

	protected override void Start()
	{
		leaveButton.onClick.AddListener(delegate
		{
			OraculumHub<ApocalypseSelectionPanel>.Display(show: false);
		});
		CanvasHelper.ScaleCanvasTowards720P(canvasScaler);
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		InitAudio();
		base.Start();
	}

	private List<ApocalypseModifierDisplay> GetAllModifierDisplaysVisible()
	{
		return apocalypseModifierDisplays.Where((ApocalypseModifierDisplay modifierDisplay) => modifierDisplay.gameObject.activeSelf).ToList();
	}

	private int GetModifierDisplayRowIndex(ApocalypseModifierDisplay modifierDisplay)
	{
		int constraintCount = gridLayoutGroup.constraintCount;
		int num = -1;
		foreach (ApocalypseModifierDisplay apocalypseModifierDisplay in apocalypseModifierDisplays)
		{
			if (apocalypseModifierDisplay.gameObject.activeSelf)
			{
				num++;
				if (apocalypseModifierDisplay == modifierDisplay)
				{
					break;
				}
			}
		}
		return num / constraintCount;
	}

	private void InitAudio()
	{
		audioSources = new AudioSource[audioSourcesCount];
		audioSources[0] = audioSourceTemplate;
		for (int i = 1; i < audioSources.Length; i++)
		{
			AudioSource audioSource = UnityEngine.Object.Instantiate(audioSourceTemplate, audioSourceTemplate.transform.parent);
			audioSources[i] = audioSource;
		}
	}

	private void InitJoystickNavigation()
	{
		topPanelJoystickSimpleTarget.ClearSelectables();
		if (apocalypseHeader.ApocalypseGaugeDisplay != null)
		{
			int rewardAtLevelTooltipDisplayersNb = apocalypseHeader.ApocalypseGaugeDisplay.RewardAtLevelTooltipDisplayersNb;
			if (rewardAtLevelTooltipDisplayersNb == 0)
			{
				return;
			}
			for (int i = 0; i < rewardAtLevelTooltipDisplayersNb; i++)
			{
				ApocalypseRewardAtLevelTooltipDisplayer rewardTooltipDisplayerAtIndex = apocalypseHeader.ApocalypseGaugeDisplay.GetRewardTooltipDisplayerAtIndex(i);
				topPanelJoystickSimpleTarget.AddSelectable(rewardTooltipDisplayerAtIndex.JoystickSelectable);
				rewardTooltipDisplayerAtIndex.JoystickSelectable.SetSelectOnUp(apocalypseHeader.ApocalypseLevelViewSelectable);
				if (i == 0)
				{
					apocalypseHeader.ApocalypseLevelViewSelectable.SetSelectOnDown(rewardTooltipDisplayerAtIndex.JoystickSelectable);
				}
			}
		}
		topPanelJoystickSimpleTarget.AddSelectable(apocalypseHeader.ApocalypseLevelViewSelectable);
	}

	private void InitModifiersData()
	{
		lockedModifiersDefinitions.Clear();
		unlockedModifiersDefinitions.Clear();
		unlockedModifiersByTierId.Clear();
		highestCompletedStepIndexByModifierId.Clear();
		foreach (ApocalypseModifierDefinition value in ApocalypseDatabase.ModifierDefinitions.Values)
		{
			if (!ApocalypseManager.IsModifierUnlocked(value))
			{
				lockedModifiersDefinitions.Add(value);
				continue;
			}
			unlockedModifiersDefinitions.Add(value);
			if (unlockedModifiersByTierId.ContainsKey(value.TierId))
			{
				unlockedModifiersByTierId[value.TierId].Add(value);
			}
			else
			{
				unlockedModifiersByTierId.Add(value.TierId, new List<ApocalypseModifierDefinition> { value });
			}
			if (!highestCompletedStepIndexByModifierId.ContainsKey(value.Id))
			{
				highestCompletedStepIndexByModifierId.Add(value.Id, value.GetHighestCompletedStepIndex());
			}
		}
		lockedModifiersDefinitions = lockedModifiersDefinitions.OrderBy((ApocalypseModifierDefinition modifier) => ApocalypseDatabase.OrderedTierDefinitions.IndexOf(ApocalypseDatabase.TierDefinitions[modifier.TierId])).ToList();
	}

	private void InstantiateApocalypseModifier(ApocalypseModifierDefinition apocalypseModifierDefinition, bool isLocked)
	{
		ApocalypseModifierDisplay apocalypseModifierDisplay = UnityEngine.Object.Instantiate(apocalypseModifierDisplayPrefab, apocalypseModifiersDisplaysContainer);
		apocalypseModifierDisplay.Init(apocalypseModifierDefinition, isLocked, -1);
		apocalypseModifierDisplay.Refresh();
		apocalypseModifierDisplay.GetComponent<JoystickSelectable>().AddListenerOnSelect(delegate
		{
			OnJoystickSelect(apocalypseModifierDisplay.transform as RectTransform);
		});
		apocalypseModifierDisplays.Add(apocalypseModifierDisplay);
	}

	private void InitializeFilters()
	{
		availableFilterDefinitions.Clear();
		foreach (FilterDefinition value2 in GenericDatabase.FilterDefinitions.Values)
		{
			if (value2.IsAll)
			{
				availableFilterDefinitions.Add(value2);
				break;
			}
		}
		List<FilterDefinition> list = new List<FilterDefinition>();
		foreach (string usedFilterId in ApocalypseDatabase.UsedFilterIds)
		{
			if (GenericDatabase.FilterDefinitions.TryGetValue(usedFilterId, out var value))
			{
				list.Add(value);
			}
		}
		list = list.OrderBy((FilterDefinition filterDefinition) => filterDefinition.Id).ToList();
		availableFilterDefinitions.AddRange(list);
		List<TMP_Dropdown.OptionData> list2 = new List<TMP_Dropdown.OptionData>();
		foreach (FilterDefinition availableFilterDefinition in availableFilterDefinitions)
		{
			string text = availableFilterDefinition.Name;
			list2.Add(new TMP_Dropdown.OptionData(text, availableFilterDefinition.FilterSprite));
		}
		filtersDropDown.ClearOptions();
		filtersDropDown.AddOptions(list2);
	}

	private void InstantiateApocalypseModifiers()
	{
		List<ApocalypseModifierDefinition> list = new List<ApocalypseModifierDefinition>();
		foreach (ApocalypseModifierDefinition value in ApocalypseDatabase.ModifierDefinitions.Values)
		{
			if (!ApocalypseManager.IsModifierUnlocked(value))
			{
				list.Add(value);
			}
			else
			{
				InstantiateApocalypseModifier(value, isLocked: false);
			}
		}
		foreach (ApocalypseModifierDefinition item in list)
		{
			InstantiateApocalypseModifier(item, isLocked: true);
		}
	}

	private bool IsApocalypseModifierDisplayVisible(ApocalypseModifierDisplay modifierDisplay)
	{
		string filterId = ((selectedFilterIndex < availableFilterDefinitions.Count) ? availableFilterDefinitions[selectedFilterIndex].Id : null);
		return modifierDisplay.ApocalypseModifierDefinition.IsMatchingFilter(filterId);
	}

	private void OnJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(source, apocalypseModifiersViewport, apocalypseModifiersScrollbar, 0.02f, 0f, 0.1f);
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		if (base.Displayed)
		{
			if (controllerType == ControllerType.Joystick && !codeSharingView.ApocalypseEditCodePopup.Displayed)
			{
				SelectDefaultJoystickSelectable();
			}
			else
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			}
		}
	}

	private void PlayAudioClip(AudioClip clip)
	{
		if (!(clip == null))
		{
			SoundManager.PlayAudioClip(GetNextAudioSource(), clip);
		}
	}

	private void PlayAudioClip(E_Sfx sfx)
	{
		switch (sfx)
		{
		case E_Sfx.ModifierOff:
			PlayModifierOffSfx();
			break;
		case E_Sfx.ModifierOn:
			PlayModifierOnSfx();
			break;
		case E_Sfx.ModifierStepDecremented:
			PlayModifierStepDecrementedSfx();
			break;
		case E_Sfx.ModifierStepIncremented:
			PlayModifierStepIncrementedSfx();
			break;
		}
	}

	private void RefreshContent(bool refreshModifiersOrder = true, bool refreshJoystickNavigation = true, bool useTween = true)
	{
		RefreshHeader(refreshCityName: true, refreshRewardsIcons: true, useTween);
		RefreshAllModifiers(refreshModifiersOrder, refreshJoystickNavigation);
		RefreshNewlyUnlockedModifiersNotification();
	}

	private void RefreshHeader(bool refreshCityName = false, bool refreshRewardsIcons = true, bool useTween = true)
	{
		if (refreshCityName)
		{
			apocalypseHeader.RefreshCityName();
		}
		if (refreshRewardsIcons && !useTween)
		{
			apocalypseHeader.RefreshRewardsFlames();
		}
		apocalypseHeader.RefreshApocalypseLevel(useTween);
		apocalypseHeader.ApocalypseEffectsTooltip.SetApocalypseModifierStepDefinitions(ApocalypseManager.CurrentApocalypseModifierStepDefinitions);
	}

	private void RefreshModifiersSorting()
	{
		int num = 0;
		if (isSortingByCompletionEnabled)
		{
			List<ApocalypseModifierDefinition> source = unlockedModifiersDefinitions.ToList();
			source = (from modifier in source
				orderby ApocalypseManager.ModifiersUnlockSeen[modifier.Id], highestCompletedStepIndexByModifierId[modifier.Id], ApocalypseDatabase.OrderedTierDefinitions.IndexOf(ApocalypseDatabase.TierDefinitions[modifier.TierId]) descending
				select modifier).ToList();
			for (int num2 = 0; num2 < source.Count; num2++)
			{
				ApocalypseModifierDisplay apocalypseModifierDisplay = apocalypseModifierDisplays[num];
				ApocalypseModifierDefinition apocalypseModifierDefinition = source[num2];
				SetApocalypseDisplayDefinition(apocalypseModifierDisplay, apocalypseModifierDefinition, isLocked: false, highestCompletedStepIndexByModifierId[apocalypseModifierDefinition.Id], refresh: true);
				apocalypseModifierDisplay.gameObject.SetActive(IsApocalypseModifierDisplayVisible(apocalypseModifierDisplay));
				num++;
			}
		}
		else
		{
			for (int num3 = ApocalypseDatabase.OrderedTierDefinitions.Count - 1; num3 >= 0; num3--)
			{
				ApocalypseTierDefinition apocalypseTierDefinition = ApocalypseDatabase.OrderedTierDefinitions[num3];
				if (unlockedModifiersByTierId.ContainsKey(apocalypseTierDefinition.Id))
				{
					int count = unlockedModifiersByTierId[apocalypseTierDefinition.Id].Count;
					for (int num4 = 0; num4 < count; num4++)
					{
						ApocalypseModifierDisplay apocalypseModifierDisplay = apocalypseModifierDisplays[num];
						ApocalypseModifierDefinition apocalypseModifierDefinition = unlockedModifiersByTierId[apocalypseTierDefinition.Id][num4];
						SetApocalypseDisplayDefinition(apocalypseModifierDisplay, apocalypseModifierDefinition, isLocked: false, highestCompletedStepIndexByModifierId[apocalypseModifierDefinition.Id], refresh: true);
						apocalypseModifierDisplay.gameObject.SetActive(IsApocalypseModifierDisplayVisible(apocalypseModifierDisplay));
						num++;
					}
				}
			}
		}
		foreach (ApocalypseModifierDefinition lockedModifiersDefinition in lockedModifiersDefinitions)
		{
			ApocalypseModifierDisplay apocalypseModifierDisplay = apocalypseModifierDisplays[num];
			SetApocalypseDisplayDefinition(apocalypseModifierDisplay, lockedModifiersDefinition, isLocked: true, -1, refresh: true);
			apocalypseModifierDisplay.gameObject.SetActive(IsApocalypseModifierDisplayVisible(apocalypseModifierDisplay));
			num++;
		}
	}

	private void RefreshSelectedFilter()
	{
		filtersDropDown.SetValueWithoutNotify(selectedFilterIndex);
	}

	private void ResetFilter()
	{
		selectedFilterIndex = 0;
		RefreshSelectedFilter();
	}

	private void SetApocalypseDisplayDefinition(ApocalypseModifierDisplay modifierDisplay, ApocalypseModifierDefinition newModifierDefinition, bool isLocked, int highestCompletedStepIndex, bool refresh)
	{
		modifierDisplay.Init(newModifierDefinition, isLocked, highestCompletedStepIndex);
		if (refresh)
		{
			modifierDisplay.Refresh();
		}
	}

	private void RefreshJoystickNavigation()
	{
		scrollViewJoystickSimpleTarget.ClearSelectables();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)gridNavigationInitializer.transform);
		gridNavigationInitializer.InitNavigation(reset: true);
		int count = apocalypseModifierDisplays.Count;
		ApocalypseModifierDisplay apocalypseModifierDisplay = null;
		List<ApocalypseModifierDisplay> list = new List<ApocalypseModifierDisplay>();
		for (int i = 0; i < count; i++)
		{
			ApocalypseModifierDisplay apocalypseModifierDisplay2 = apocalypseModifierDisplays[i];
			if (apocalypseModifierDisplay2.gameObject.activeSelf)
			{
				if (GetModifierDisplayRowIndex(apocalypseModifierDisplay2) == 0)
				{
					apocalypseModifierDisplay2.JoystickSelectable.SetSelectOnUp(topPanelJoystickSimpleTarget.GetSelectionInfo().Selectable);
				}
				scrollViewJoystickSimpleTarget.AddSelectable(apocalypseModifierDisplay2.JoystickSelectable);
				apocalypseModifierDisplay = apocalypseModifierDisplay2;
				list.Add(apocalypseModifierDisplay2);
			}
		}
		if (apocalypseModifierDisplay != null)
		{
			int modifierDisplayRowIndex = GetModifierDisplayRowIndex(apocalypseModifierDisplay);
			apocalypseModifierDisplay.JoystickSelectable.SetSelectOnDown(bottomPanelJoystickSimpleTarget.GetSelectionInfo().Selectable);
			Selectable[] array = bottomPanelJoystickSelectables;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetSelectOnUp(apocalypseModifierDisplay.JoystickSelectable);
			}
			if (list.Count >= 2)
			{
				ApocalypseModifierDisplay apocalypseModifierDisplay3 = list[^2];
				int modifierDisplayRowIndex2 = GetModifierDisplayRowIndex(apocalypseModifierDisplay3);
				if (modifierDisplayRowIndex == modifierDisplayRowIndex2)
				{
					apocalypseModifierDisplay3.JoystickSelectable.SetSelectOnDown(bottomPanelJoystickSimpleTarget.GetSelectionInfo().Selectable);
				}
			}
			int num = apocalypseModifierDisplays.IndexOf(apocalypseModifierDisplay);
			num--;
			if (num >= 0 && num < apocalypseModifierDisplays.Count && !apocalypseModifierDisplays[num].gameObject.activeSelf)
			{
			}
		}
		else
		{
			Selectable[] array = bottomPanelJoystickSelectables;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetSelectOnUp(topPanelJoystickSimpleTarget.GetSelectionInfo().Selectable);
			}
		}
		if (!(apocalypseHeader.ApocalypseGaugeDisplay != null))
		{
			return;
		}
		int rewardAtLevelTooltipDisplayersNb = apocalypseHeader.ApocalypseGaugeDisplay.RewardAtLevelTooltipDisplayersNb;
		if (rewardAtLevelTooltipDisplayersNb != 0)
		{
			for (int num2 = rewardAtLevelTooltipDisplayersNb - 1; num2 >= 0; num2--)
			{
				apocalypseHeader.ApocalypseGaugeDisplay.GetRewardTooltipDisplayerAtIndex(num2).JoystickSelectable.SetSelectOnDown(joystickDynamicTargetGaugeDown.GetSelectionInfo().Selectable);
			}
		}
	}

	private IEnumerator SelectDefaultJoystickSelectableAfterAFrameCoroutine()
	{
		yield return new WaitForEndOfFrame();
		SelectDefaultJoystickSelectable();
	}

	private IEnumerator SelectBottomPanelAfterAFrameCoroutine()
	{
		yield return new WaitForEndOfFrame();
		TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(bottomPanelJoystickSimpleTarget.GetSelectionInfo());
	}

	private void Update()
	{
		if (!base.Displayed || base.OpeningOrClosing || !TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			return;
		}
		if (currentModifierDisplayForJoystick != null)
		{
			if (TheLastStand.Manager.InputManager.GetButtonDown(145))
			{
				currentModifierDisplayForJoystick.OnPreviousStepButtonClick();
			}
			else if (TheLastStand.Manager.InputManager.GetButtonDown(146))
			{
				currentModifierDisplayForJoystick.OnNextStepButtonClick();
			}
		}
		if (TheLastStand.Manager.InputManager.GetButtonDown(147))
		{
			if (codeSharingView.ApocalypseEditCodePopup.Displayed)
			{
				codeSharingView.ApocalypseEditCodePopup.OnValidateButtonClicked();
			}
			else
			{
				sorterByCompletion.OnClick();
			}
		}
		if (TheLastStand.Manager.InputManager.GetButtonDown(148))
		{
			OnGoToGlyphsPanelButtonClick();
		}
	}
}
