using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseModifierDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
	private static class Constants
	{
		public const string ApocalypseModifierIconDefault = "GuessAgain";

		public const string DropDownRedColor = "CA483F";
	}

	[SerializeField]
	private FollowElement.FollowDatas followDatas;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI titleLocked;

	[SerializeField]
	private GameObject modifierLevelContainer;

	[SerializeField]
	private Image modifierLevelFlamesIcon;

	[SerializeField]
	private TextMeshProUGUI modifierLevelGainText;

	[SerializeField]
	private GameObject stepControlsContainer;

	[SerializeField]
	private BetterButton previousStepButton;

	[SerializeField]
	private BetterButton nextStepButton;

	[SerializeField]
	private TMP_Dropdown stepsDropDown;

	[SerializeField]
	private Image boxBackground;

	[SerializeField]
	private Image boxHoveredFeedback;

	[SerializeField]
	private Image apocalypseModifierIcon;

	[SerializeField]
	private Image newlyUnlockedIcon;

	[SerializeField]
	private Image allStepsCompletedIcon;

	[SerializeField]
	private GameObject[] joystickNavigationIconContainers;

	[SerializeField]
	private Color lockedModifierIconColor = Color.white;

	[SerializeField]
	private Material grayScaleMaterial;

	[SerializeField]
	private List<ApocalypseModifierStepPointDisplay> modifierStepPointDisplays;

	[SerializeField]
	private GameObject modifierStepPointsContainer;

	[SerializeField]
	private Sprite completedStepSprite;

	[SerializeField]
	private Sprite defaultIconSprite;

	[SerializeField]
	private Sprite lockedBoxSprite;

	[SerializeField]
	private Sprite selectedBoxSprite;

	[SerializeField]
	private Sprite notSelectedBoxSprite;

	[SerializeField]
	private Sprite selectedFlamesSprite;

	[SerializeField]
	private Sprite notSelectedFlamesSprite;

	[SerializeField]
	protected JoystickSelectable joystickSelectable;

	[SerializeField]
	protected AudioClip hoverClip;

	private ItemRestrictionCategoriesCollection categoriesCollection;

	private bool areAllStepsCompleted;

	private List<bool> stepsCompletedState = new List<bool>();

	private bool previousArrowHovered;

	private bool nextArrowHovered;

	private Coroutine checkDropDownClosedOnPointerEnter;

	private int highestCompletedStepIndex;

	private ApocalypseModifierStepDefinition overridenTooltipStepModifierDefinition;

	public ApocalypseModifierDefinition ApocalypseModifierDefinition { get; private set; }

	public bool IsDropdownExpanded => stepsDropDown.IsExpanded;

	public bool IsHovered { get; private set; }

	public bool IsLocked { get; private set; }

	public bool IsSelected => SelectedModifierStepDefinition != null;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public bool JoystickSelected { get; private set; }

	public ApocalypseModifierStepDefinition SelectedModifierStepDefinition { get; private set; }

	public int SelectedModifierStepIndex
	{
		get
		{
			if (ApocalypseModifierDefinition == null)
			{
				return -1;
			}
			return ApocalypseModifierDefinition.StepDefinitions.IndexOf(SelectedModifierStepDefinition);
		}
	}

	public bool UnlockSeen
	{
		get
		{
			if (ApocalypseModifierDefinition != null)
			{
				return ApocalypseManager.ModifiersUnlockSeen[ApocalypseModifierDefinition.Id];
			}
			return false;
		}
	}

	public void Init(ApocalypseModifierDefinition apocalypseModifierDefinition, bool isLocked, int highestStepIndexCompleted)
	{
		ApocalypseModifierDefinition = apocalypseModifierDefinition;
		IsLocked = isLocked;
		highestCompletedStepIndex = highestStepIndexCompleted;
		InitDropdownSteps();
		RetrieveSelectedStep();
	}

	public void ChangeSelectedStep(int stepIndex, bool refreshDropdownList = true)
	{
		bool flag = false;
		if (stepIndex == -1)
		{
			TPSingleton<ApocalypseSelectionPanel>.Instance.UnselectApocalypseModifierStep(SelectedModifierStepDefinition);
			SelectedModifierStepDefinition = null;
			flag = true;
		}
		else if (stepIndex >= 0 && stepIndex < ApocalypseModifierDefinition.StepDefinitions.Count)
		{
			ApocalypseSelectionPanel.E_Sfx sfx = ApocalypseSelectionPanel.E_Sfx.None;
			if (SelectedModifierStepIndex == -1)
			{
				sfx = ApocalypseSelectionPanel.E_Sfx.ModifierOn;
			}
			else if (SelectedModifierStepIndex < stepIndex)
			{
				sfx = ApocalypseSelectionPanel.E_Sfx.ModifierStepIncremented;
			}
			else if (SelectedModifierStepIndex > stepIndex)
			{
				sfx = ApocalypseSelectionPanel.E_Sfx.ModifierStepDecremented;
			}
			flag = true;
			SelectedModifierStepDefinition = ApocalypseModifierDefinition.StepDefinitions[stepIndex];
			TPSingleton<ApocalypseSelectionPanel>.Instance.SelectApocalypseModifierStep(SelectedModifierStepDefinition, updateApocalypse: true, playFeedback: true, sfx);
		}
		if (flag)
		{
			RefreshSelectedStep(refreshDropdownList);
		}
		if (TPSingleton<ApocalypseSelectionPanel>.Instance.ApocalypseModifierStepTooltip.Displayed)
		{
			RefreshModifierStepTooltip();
		}
	}

	public void ChangeModifierTooltip(int stepIndex)
	{
		overridenTooltipStepModifierDefinition = null;
		if (stepIndex <= -1)
		{
			ShowTooltip(refresh: true);
			return;
		}
		if (stepIndex < ApocalypseModifierDefinition.StepDefinitions.Count)
		{
			overridenTooltipStepModifierDefinition = ApocalypseModifierDefinition.StepDefinitions[stepIndex];
		}
		ShowTooltip(refresh: true);
	}

	public void OnNextStepButtonClick()
	{
		if (!IsLocked && ApocalypseModifierDefinition != null)
		{
			int selectedModifierStepIndex = SelectedModifierStepIndex;
			if (selectedModifierStepIndex + 1 < ApocalypseModifierDefinition.StepDefinitions.Count)
			{
				ChangeSelectedStep(selectedModifierStepIndex + 1);
				ShowTooltipIfModifierHovered();
			}
		}
	}

	public void OnPreviousStepButtonClick()
	{
		if (!IsLocked && ApocalypseModifierDefinition != null)
		{
			int selectedModifierStepIndex = SelectedModifierStepIndex;
			if (selectedModifierStepIndex - 1 >= -1)
			{
				ChangeSelectedStep(selectedModifierStepIndex - 1);
				ShowTooltipIfModifierHovered();
			}
		}
	}

	public void OnNextStepButtonHovered(bool isHovered)
	{
		if (IsLocked || ApocalypseModifierDefinition == null)
		{
			nextArrowHovered = false;
			return;
		}
		nextArrowHovered = isHovered;
		ShowTooltipIfModifierHovered();
	}

	public void OnPreviousStepButtonHovered(bool isHovered)
	{
		if (IsLocked || ApocalypseModifierDefinition == null)
		{
			previousArrowHovered = false;
			return;
		}
		previousArrowHovered = isHovered;
		ShowTooltipIfModifierHovered();
	}

	public void OnStepChangedInDropdownList()
	{
		int value = stepsDropDown.value;
		ChangeSelectedStep(value - 1, refreshDropdownList: false);
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			OnPointerEnter(null);
			JoystickSelected = true;
			if (TPSingleton<ApocalypseSelectionPanel>.Instance != null)
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.SetCurrentModifierDisplayForJoystick(this);
			}
		}
		RefreshJoystickIconsVisibleState();
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			OnPointerExit(null);
		}
		JoystickSelected = false;
		RefreshJoystickIconsVisibleState();
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	public void Refresh()
	{
		if (ApocalypseModifierDefinition != null)
		{
			RefreshJoystickIconsVisibleState();
			allStepsCompletedIcon.gameObject.SetActive(areAllStepsCompleted);
			RefreshSelectedStep();
			RefreshUnlockNotification();
			RefreshTitle();
		}
	}

	public void RefreshSelectedStep(bool refreshDropDownList = true)
	{
		RefreshBoxSprite();
		RefreshIconSprite();
		stepControlsContainer.SetActive(!IsLocked);
		if (IsLocked)
		{
			modifierLevelFlamesIcon.sprite = notSelectedFlamesSprite;
			modifierLevelContainer.SetActive(value: false);
			modifierStepPointsContainer.SetActive(value: false);
			return;
		}
		modifierLevelContainer.SetActive(value: true);
		modifierStepPointsContainer.SetActive(value: true);
		int selectedModifierStepIndex = SelectedModifierStepIndex;
		bool flag = selectedModifierStepIndex - 1 >= -1;
		bool flag2 = selectedModifierStepIndex + 1 < ApocalypseModifierDefinition.StepDefinitions.Count;
		previousStepButton.Interactable = flag;
		nextStepButton.Interactable = flag2;
		if (!flag)
		{
			previousArrowHovered = false;
		}
		if (!flag2)
		{
			nextArrowHovered = false;
		}
		if (refreshDropDownList)
		{
			stepsDropDown.SetValueWithoutNotify(selectedModifierStepIndex + 1);
		}
		int count = modifierStepPointDisplays.Count;
		for (int i = 0; i < ApocalypseModifierDefinition.StepDefinitions.Count; i++)
		{
			if (i < count)
			{
				modifierStepPointDisplays[i].ChangeDisplay(selectedModifierStepIndex >= i);
				modifierStepPointDisplays[i].SetCompletionFeedbackVisible(IsStepIndexCompleted(i));
			}
		}
		modifierLevelFlamesIcon.sprite = (IsSelected ? selectedFlamesSprite : notSelectedFlamesSprite);
		modifierLevelGainText.enabled = IsSelected;
		if (IsSelected)
		{
			modifierLevelGainText.text = $"<style=Bad>+{SelectedModifierStepDefinition.ApocalypseLevel}</style>";
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (IsDropdownExpanded)
		{
			if (checkDropDownClosedOnPointerEnter == null)
			{
				checkDropDownClosedOnPointerEnter = StartCoroutine(CheckDropDownClosedOnPointerEnter());
			}
			return;
		}
		if (!IsLocked)
		{
			IsHovered = true;
			if (!UnlockSeen)
			{
				ApocalypseManager.SetModifierUnlockSeen(ApocalypseModifierDefinition.Id, isSeen: true);
				RefreshUnlockNotification(true);
				TPSingleton<ApocalypseSelectionPanel>.Instance.RefreshNewlyUnlockedModifiersNotification();
			}
			SoundManager.PlayAudioClip(TPSingleton<ApocalypseSelectionPanel>.Instance.GetNextAudioSource(), hoverClip);
		}
		if ((!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips) && !IsLocked)
		{
			ShowTooltip();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		boxHoveredFeedback.enabled = false;
		IsHovered = false;
		TPSingleton<ApocalypseSelectionPanel>.Instance.ApocalypseModifierStepTooltip.Hide();
	}

	public void RetrieveSelectedStep()
	{
		if (ApocalypseModifierDefinition == null || ApocalypseManager.CurrentApocalypse == null)
		{
			return;
		}
		SelectedModifierStepDefinition = null;
		foreach (ApocalypseModifierStepDefinition modifierStepDefinition in ApocalypseManager.CurrentApocalypse.ModifierStepDefinitions)
		{
			int num = ApocalypseModifierDefinition.StepDefinitions.IndexOf(modifierStepDefinition);
			if (num != -1)
			{
				SelectedModifierStepDefinition = ApocalypseModifierDefinition.StepDefinitions[num];
			}
		}
	}

	private IEnumerator CheckDropDownClosedOnPointerEnter()
	{
		yield return new WaitForEndOfFrame();
		OnPointerEnter(null);
		checkDropDownClosedOnPointerEnter = null;
	}

	private Sprite GetApocalypseModifierIcon(ApocalypseModifierDefinition apocalypseModifierDefinition, bool isSelected)
	{
		if (apocalypseModifierDefinition == null)
		{
			return null;
		}
		if (apocalypseModifierDefinition.FilterIds.Count > 0)
		{
			Sprite result = null;
			if (GenericDatabase.FilterDefinitions.TryGetValue(apocalypseModifierDefinition.FilterIds.ToList()[0], out var value))
			{
				result = (isSelected ? value.ApocalypseOnFilterSprite : value.ApocalypseOffFilterSprite);
			}
			return result;
		}
		return null;
	}

	private void InitDropdownSteps()
	{
		if (ApocalypseModifierDefinition == null)
		{
			return;
		}
		areAllStepsCompleted = false;
		int count = ApocalypseModifierDefinition.StepDefinitions.Count;
		stepsCompletedState.Clear();
		for (int i = 0; i < count; i++)
		{
			stepsCompletedState.Add(item: false);
		}
		int num = 0;
		if (highestCompletedStepIndex == count - 1)
		{
			areAllStepsCompleted = true;
		}
		for (num = 0; num < stepsCompletedState.Count; num++)
		{
			if (num <= highestCompletedStepIndex)
			{
				stepsCompletedState[num] = true;
			}
		}
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		list.Add(new TMP_Dropdown.OptionData(Localizer.Get("ApocalypseModifier_DropdownStep_Off")));
		num = 0;
		bool flag = ApocalypseModifierDefinition.StepDefinitions.Count > 1;
		foreach (ApocalypseModifierStepDefinition stepDefinition in ApocalypseModifierDefinition.StepDefinitions)
		{
			string text = (string.IsNullOrEmpty(stepDefinition.ViewDropdownValue) ? ((flag ? (num + 1).ToRoman() : Localizer.Get("ApocalypseModifier_DropdownStep_On")) ?? "") : stepDefinition.ViewDropdownValue);
			text = "<color=#CA483F>" + text + "</color>";
			list.Add(new TMP_Dropdown.OptionData(text, (num < stepsCompletedState.Count && stepsCompletedState[num]) ? completedStepSprite : null));
			num++;
		}
		stepsDropDown.ClearOptions();
		stepsDropDown.AddOptions(list);
		int num2 = 0;
		foreach (ApocalypseModifierStepPointDisplay modifierStepPointDisplay in modifierStepPointDisplays)
		{
			modifierStepPointDisplay.gameObject.SetActive(num2 < count);
			if (modifierStepPointDisplay.gameObject.activeSelf)
			{
				modifierStepPointDisplay.Init(num2);
			}
			num2++;
		}
	}

	private bool IsStepIndexCompleted(int stepIndex)
	{
		if (stepIndex == -1)
		{
			return false;
		}
		if (stepIndex < stepsCompletedState.Count)
		{
			return stepsCompletedState[stepIndex];
		}
		return false;
	}

	private void RefreshIconSprite()
	{
		Sprite sprite = GetApocalypseModifierIcon(ApocalypseModifierDefinition, IsSelected);
		if (sprite != null)
		{
			apocalypseModifierIcon.sprite = sprite;
		}
		else
		{
			apocalypseModifierIcon.sprite = defaultIconSprite;
		}
		apocalypseModifierIcon.material = null;
		apocalypseModifierIcon.color = Color.white;
		if (IsLocked)
		{
			apocalypseModifierIcon.material = grayScaleMaterial;
			apocalypseModifierIcon.color = lockedModifierIconColor;
		}
	}

	private void RefreshBoxSprite()
	{
		if (IsSelected)
		{
			boxBackground.sprite = selectedBoxSprite;
		}
		else
		{
			boxBackground.sprite = (IsLocked ? lockedBoxSprite : notSelectedBoxSprite);
		}
	}

	private void RefreshJoystickIconsVisibleState()
	{
		bool active = JoystickSelected && InputManager.IsLastControllerJoystick;
		GameObject[] array = joystickNavigationIconContainers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(active);
		}
	}

	private void RefreshTitle()
	{
		int num = 0;
		if (IsLocked && ApocalypseDatabase.TierDefinitions.TryGetValue(ApocalypseModifierDefinition?.TierId, out var value))
		{
			num = value.ApocalypseLevelCompletedToUnlock;
		}
		title.enabled = !IsLocked;
		titleLocked.enabled = IsLocked;
		if (IsLocked)
		{
			titleLocked.text = Localizer.Format("ApocalypseModifier_Title_Locked", num);
		}
		else
		{
			title.text = ApocalypseModifierDefinition?.GetLocalizedTitle();
		}
	}

	private void RefreshUnlockNotification(bool? forcedSeenState = null)
	{
		if (!IsLocked)
		{
			bool flag = forcedSeenState ?? UnlockSeen;
			newlyUnlockedIcon.enabled = !flag;
			newlyUnlockedIcon.material = (flag ? null : TPSingleton<ApocalypseSelectionPanel>.Instance.NewlyUnlockedNotificationMaterial);
		}
		else
		{
			newlyUnlockedIcon.enabled = false;
			newlyUnlockedIcon.material = null;
		}
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (showTooltips && JoystickSelected)
		{
			ShowTooltip();
		}
		else
		{
			TPSingleton<ApocalypseSelectionPanel>.Instance.ApocalypseModifierStepTooltip.Hide();
		}
	}

	private void RefreshModifierStepTooltip()
	{
		ApocalypseModifierStepTooltip apocalypseModifierStepTooltip = TPSingleton<ApocalypseSelectionPanel>.Instance.ApocalypseModifierStepTooltip;
		apocalypseModifierStepTooltip.Init(SelectedModifierStepDefinition ?? ApocalypseModifierDefinition.StepDefinitions[0]);
		if (apocalypseModifierStepTooltip.Displayed)
		{
			apocalypseModifierStepTooltip.Refresh();
		}
	}

	private void ShowTooltip(bool refresh = false)
	{
		ApocalypseModifierStepTooltip apocalypseModifierStepTooltip = TPSingleton<ApocalypseSelectionPanel>.Instance.ApocalypseModifierStepTooltip;
		if (!previousArrowHovered && !nextArrowHovered)
		{
			if (overridenTooltipStepModifierDefinition == null)
			{
				apocalypseModifierStepTooltip.Init(SelectedModifierStepDefinition ?? ApocalypseModifierDefinition.StepDefinitions[0]);
			}
			else
			{
				apocalypseModifierStepTooltip.Init(overridenTooltipStepModifierDefinition);
			}
		}
		else
		{
			ApocalypseModifierStepDefinition apocalypseModifierStepDefinition = null;
			if (previousArrowHovered && SelectedModifierStepIndex >= 1)
			{
				apocalypseModifierStepDefinition = ApocalypseModifierDefinition.StepDefinitions[SelectedModifierStepIndex - 1];
			}
			if (nextArrowHovered)
			{
				apocalypseModifierStepDefinition = ApocalypseModifierDefinition.StepDefinitions[SelectedModifierStepIndex + 1];
			}
			if (apocalypseModifierStepDefinition == null)
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.ApocalypseModifierStepTooltip.Hide();
				return;
			}
			apocalypseModifierStepTooltip.Init(apocalypseModifierStepDefinition);
		}
		apocalypseModifierStepTooltip.FollowElement.ChangeFollowDatas(followDatas);
		apocalypseModifierStepTooltip.Display();
		if (refresh)
		{
			apocalypseModifierStepTooltip.Refresh();
		}
	}

	private void ShowTooltipIfModifierHovered()
	{
		if (IsHovered)
		{
			ShowTooltip(refresh: true);
		}
	}
}
