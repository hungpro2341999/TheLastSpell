using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class WeaponRestrictionsPanel : TPSingleton<WeaponRestrictionsPanel>
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private BetterButton closeButton;

	[SerializeField]
	private TextMeshProUGUI weaponCategoriesWarningText;

	[SerializeField]
	private BetterToggle customModeToggle;

	[SerializeField]
	private TutorialPopup tutorialPopup;

	[SerializeField]
	protected Selectable selectableAfterTutorialClosed;

	[SerializeField]
	private WeaponFamiliesCountDisplay weaponFamiliesCountDisplay;

	[SerializeField]
	private List<WeaponRestrictionsCategoryPanel> weaponCategoryPanels;

	[SerializeField]
	private WeaponFamilyTooltip weaponFamilyTooltip;

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

	private AudioSource[] audioSources;

	private int nextAudioSourceIndex;

	public bool AreCategoriesCorrectlyConfigured { get; private set; }

	public bool CanClosePanel => AreCategoriesCorrectlyConfigured;

	public ItemRestrictionCategoriesCollection CurrentRestrictionCategoriesCollection { get; private set; }

	public bool Displayed { get; protected set; }

	public bool OpeningOrClosing { get; private set; }

	public WeaponFamilyTooltip WeaponFamilyTooltip => weaponFamilyTooltip;

	public static event Action OnPanelClosed;

	public void Open()
	{
		if (!Displayed)
		{
			CheckIfMustCreateNewWeaponFamilyDisplay();
			Display(mustDisplay: true);
		}
	}

	public void Close()
	{
		if (Displayed && CanClosePanel)
		{
			DeactivateCustomModeIfNeeded();
			Display(mustDisplay: false);
			WeaponRestrictionsPanel.OnPanelClosed();
		}
	}

	public AudioSource GetNextAudioSource()
	{
		return audioSources[nextAudioSourceIndex++ % audioSources.Length];
	}

	public void PlayErrorClip()
	{
		SoundManager.PlayAudioClip(GetNextAudioSource(), errorClips.RandomElement());
	}

	public void OnWeaponFamilyDisplaySelectChanged(ItemRestrictionFamily itemRestrictionFamily)
	{
		RefreshWeaponCategoryPanel(itemRestrictionFamily.ItemFamilyDefinition.ItemCategory);
		RefreshContentRelatedToFamiliesSelection();
		PlayWeaponFamilySelectClip(itemRestrictionFamily.IsActive);
		if (weaponFamilyTooltip.Displayed)
		{
			weaponFamilyTooltip.Refresh();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		CurrentRestrictionCategoriesCollection = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories;
		customModeToggle.onValueChanged.AddListener(ToggleCustomMode);
	}

	protected void Start()
	{
		closeButton.onClick.AddListener(Close);
		Init();
		InitAudio();
	}

	protected void OnDestroy()
	{
		closeButton.onClick.RemoveListener(Close);
		customModeToggle.onValueChanged.RemoveListener(ToggleCustomMode);
	}

	private void CheckIfMustCreateNewWeaponFamilyDisplay()
	{
		for (int i = 0; i < weaponCategoryPanels.Count; i++)
		{
			weaponCategoryPanels[i].TryAddNewWeaponFamilyDisplays();
		}
	}

	private void DeactivateCustomModeIfNeeded()
	{
		if (!TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeRequired() && TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive)
		{
			TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive = false;
		}
	}

	private void Display(bool mustDisplay)
	{
		canvas.enabled = mustDisplay;
		Displayed = mustDisplay;
		if (Displayed)
		{
			RefreshContent();
			RefreshJoystickNavigation();
			if (InputManager.IsLastControllerJoystick)
			{
				JoystickSelectPanel();
			}
			if (tutorialPopup != null)
			{
				tutorialPopup.SetSelectableAfterClose(selectableAfterTutorialClosed);
			}
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnWeaponRestrictionsPanelOpen);
		}
		else
		{
			weaponFamilyTooltip.Hide();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			if (InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject(null);
				TPSingleton<GameConfigurationsView>.Instance.JoystickSelectPanel();
			}
		}
		WorldMapCameraView.CheckIfCanMoveCameraInExplorationState();
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

	private void Init()
	{
		weaponFamiliesCountDisplay.Init();
		List<ItemDefinition.E_Category> list = new List<ItemDefinition.E_Category>
		{
			ItemDefinition.E_Category.MeleeWeapon,
			ItemDefinition.E_Category.RangeWeapon,
			ItemDefinition.E_Category.MagicWeapon
		};
		for (int i = 0; i < weaponCategoryPanels.Count; i++)
		{
			if (i < list.Count)
			{
				weaponCategoryPanels[i].Init(list[i]);
			}
		}
	}

	private void PlayWeaponFamilySelectClip(bool selected)
	{
		AudioClip audioClip = GetNextClip(selected ? activateClips : deactivateClips);
		if (audioClip == null)
		{
			CLoggerManager.Log("Could not find a valid weapon family audio clip!");
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
		customModeToggle.isOn = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive;
		RefreshContentRelatedToFamiliesSelection();
		foreach (WeaponRestrictionsCategoryPanel weaponCategoryPanel in weaponCategoryPanels)
		{
			weaponCategoryPanel.Refresh();
		}
	}

	private void RefreshContentRelatedToFamiliesSelection()
	{
		AreCategoriesCorrectlyConfigured = CurrentRestrictionCategoriesCollection.AreAllCategoriesCorrectlyConfigured();
		RefreshWeaponFamiliesCountDisplay();
		closeButton.interactable = AreCategoriesCorrectlyConfigured;
		weaponCategoriesWarningText.enabled = !AreCategoriesCorrectlyConfigured;
		if (!AreCategoriesCorrectlyConfigured)
		{
			RefreshWeaponCategoriesWarningText();
		}
	}

	private void RefreshWeaponFamiliesCountDisplay()
	{
		weaponFamiliesCountDisplay.Refresh();
	}

	private void RefreshWeaponCategoryPanel(ItemDefinition.E_Category category)
	{
		foreach (WeaponRestrictionsCategoryPanel weaponCategoryPanel in weaponCategoryPanels)
		{
			if (category == ItemDefinition.E_Category.All || category == weaponCategoryPanel.CurrentItemCategory)
			{
				weaponCategoryPanel.Refresh();
			}
		}
	}

	private void RefreshWeaponCategoriesWarningText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in CurrentRestrictionCategoriesCollection.ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
		{
			if (!CurrentRestrictionCategoriesCollection.IsCategoryCorrectlyConfigured(itemCategoryDefinition.ItemCategory))
			{
				int requiredSelectedFamiliesNb = CurrentRestrictionCategoriesCollection.GetRequiredSelectedFamiliesNb(itemCategoryDefinition.ItemCategory, itemCategoryDefinition);
				stringBuilder.Append(Localizer.Format("WeaponRestrictionsPanel_CategoryWarning_" + itemCategoryDefinition.ItemCategory, requiredSelectedFamiliesNb)).AppendLine();
			}
		}
		weaponCategoriesWarningText.text = stringBuilder.ToString();
	}

	private void ToggleCustomMode(bool toggle)
	{
		TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive = toggle;
		RefreshContent();
	}

	private void RefreshJoystickNavigation()
	{
		customModeToggle.SetMode(Navigation.Mode.Explicit);
		customModeToggle.ClearNavigation();
		customModeToggle.SetSelectOnDown(GetFirstWeaponFamilyDisplay().JoystickSelectable);
		RefreshWeaponFamilyDisplaysJoystickNavigation();
	}

	private void RefreshWeaponFamilyDisplaysJoystickNavigation()
	{
		int count = weaponCategoryPanels.Count;
		int num = 0;
		for (num = 0; num < count; num++)
		{
			List<WeaponFamilyDisplay> weaponFamilyDisplays = weaponCategoryPanels[num].WeaponFamilyDisplays;
			int count2 = weaponFamilyDisplays.Count;
			for (int i = 0; i < count2; i++)
			{
				int rowIndex = i / weaponCategoryPanels[num].GridLayoutGroup.constraintCount;
				if (weaponFamilyDisplays[i].JoystickSelectable.navigation.selectOnUp == null)
				{
					weaponFamilyDisplays[i].JoystickSelectable.SetSelectOnUp(customModeToggle);
				}
				if (weaponFamilyDisplays[i].JoystickSelectable.navigation.selectOnLeft == null && num - 1 >= 0)
				{
					weaponFamilyDisplays[i].JoystickSelectable.SetSelectOnLeft(weaponCategoryPanels[num - 1].GetClosestWeaponFamilyDisplayFromRowIndex(rowIndex, getClosestFromLeft: false)?.JoystickSelectable);
				}
				if (weaponFamilyDisplays[i].JoystickSelectable.navigation.selectOnRight == null && num + 1 < count)
				{
					weaponFamilyDisplays[i].JoystickSelectable.SetSelectOnRight(weaponCategoryPanels[num + 1].GetClosestWeaponFamilyDisplayFromRowIndex(rowIndex, getClosestFromLeft: true)?.JoystickSelectable);
				}
			}
		}
	}

	private void JoystickSelectPanel()
	{
		WeaponFamilyDisplay firstWeaponFamilyDisplay = GetFirstWeaponFamilyDisplay();
		GameObject gameObject = null;
		if (firstWeaponFamilyDisplay != null)
		{
			gameObject = firstWeaponFamilyDisplay.gameObject;
			selectableAfterTutorialClosed = firstWeaponFamilyDisplay.JoystickSelectable;
		}
		else
		{
			gameObject = customModeToggle.gameObject;
			selectableAfterTutorialClosed = customModeToggle;
		}
		EventSystem.current.SetSelectedGameObject(gameObject);
	}

	private WeaponFamilyDisplay GetFirstWeaponFamilyDisplay()
	{
		foreach (WeaponRestrictionsCategoryPanel weaponCategoryPanel in weaponCategoryPanels)
		{
			WeaponFamilyDisplay weaponFamilyDisplay = weaponCategoryPanel.WeaponFamilyDisplays.FirstOrDefault((WeaponFamilyDisplay o) => o.gameObject.activeSelf);
			if (weaponFamilyDisplay != null)
			{
				return weaponFamilyDisplay;
			}
		}
		return null;
	}

	static WeaponRestrictionsPanel()
	{
		WeaponRestrictionsPanel.OnPanelClosed = delegate
		{
		};
	}
}
