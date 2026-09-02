using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib.Localization;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings.KeyRemapping;

public class KeyRemappingView : TabbedPageView
{
	[SerializeField]
	private KeyRemappingCategoryView categoryViewPrefab;

	[SerializeField]
	private RectTransform categoriesViewsContainer;

	[SerializeField]
	private RectTransform layoutToRebuild;

	[SerializeField]
	private GameObject warningPanelOverlay;

	[SerializeField]
	private BetterButton[] disabledDuringMapping;

	[SerializeField]
	private BetterToggle[] tabs;

	[SerializeField]
	private CanvasGroup scrollBarCanvasGroup;

	[SerializeField]
	[Range(0f, 1f)]
	private float nonInteractableScrollBarAlpha = 0.2f;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	private float scrollBarButtonsSensitivity = 10f;

	[SerializeField]
	private RectTransform scrollContent;

	[SerializeField]
	private TextMeshProUGUI mainColumnTitle;

	[SerializeField]
	private TextMeshProUGUI secondaryColumnTitle;

	[SerializeField]
	private TextMeshProUGUI resetDefaultText;

	[SerializeField]
	private GameObject resetAllPanel;

	[SerializeField]
	private GameObject resetAllButtonContainer;

	[SerializeField]
	private TextMeshProUGUI resetAllWarningText;

	[SerializeField]
	private BetterButton resetAllButton;

	[SerializeField]
	private BetterButton resetAllConfirmButton;

	[SerializeField]
	private BetterButton resetAllCancelButton;

	[SerializeField]
	private GameObject conflictPanel;

	[SerializeField]
	private TextMeshProUGUI conflictWarningText;

	[SerializeField]
	private TextMeshProUGUI conflictActionText;

	[SerializeField]
	private BetterButton conflictConfirmButton;

	[SerializeField]
	private BetterButton conflictCancelButton;

	private List<KeyRemappingCategoryView> categoryViews = new List<KeyRemappingCategoryView>();

	public BetterButton ConflictCancelButton => conflictCancelButton;

	public BetterButton ConflictConfirmButton => conflictConfirmButton;

	public bool Initialized { get; set; }

	public BetterButton ResetAllButton => resetAllButton;

	public BetterButton ResetAllCancelButton => resetAllCancelButton;

	public BetterButton ResetAllConfirmButton => resetAllConfirmButton;

	public void AllowPanelNavigation(bool state)
	{
		for (int num = disabledDuringMapping.Length - 1; num >= 0; num--)
		{
			disabledDuringMapping[num].Interactable = state;
		}
		for (int num2 = tabs.Length - 1; num2 >= 0; num2--)
		{
			tabs[num2].interactable = state;
		}
		scrollBarCanvasGroup.alpha = (state ? 1f : nonInteractableScrollBarAlpha);
		scrollBarCanvasGroup.interactable = state;
		scrollRect.enabled = state;
	}

	public override void Close()
	{
		base.Close();
		scrollRect.enabled = false;
	}

	public void DisplayConflictSolver(bool show, string conflictedActionName = null, KeyCode? keyCode = null)
	{
		conflictPanel.SetActive(show);
		warningPanelOverlay.SetActive(show);
		DisplayResetAllOption(!show);
		if (show)
		{
			conflictWarningText.text = string.Format(Localizer.Get("KeyRemapping_ConflictWarning"), Keyboard.GetKeyName(keyCode.Value));
			conflictActionText.text = Localizer.Get("KeyRemapping_ActionName_" + conflictedActionName);
			ConflictConfirmButton.ChangeText(Localizer.Get("KeyRemapping_ConflictConfirm"));
			ConflictCancelButton.ChangeText(Localizer.Get("KeyRemapping_ConflictCancel"));
		}
	}

	public void DisplayResetAllOption(bool show)
	{
		resetAllButtonContainer.SetActive(show);
	}

	public void DisplayResetAllPanel(bool show)
	{
		resetAllPanel.SetActive(show);
		warningPanelOverlay.SetActive(show);
		DisplayResetAllOption(!show);
		AllowPanelNavigation(!show);
		if (show)
		{
			resetAllWarningText.text = Localizer.Get("KeyRemapping_ResetAllWarning");
			ResetAllConfirmButton.ChangeText(Localizer.Get("KeyRemapping_ResetAllConfirm"));
			ResetAllCancelButton.ChangeText(Localizer.Get("KeyRemapping_ResetAllCancel"));
		}
	}

	public KeyRemappingCategoryView InstantiateInputCategory(InputCategory category)
	{
		KeyRemappingCategoryView keyRemappingCategoryView = Object.Instantiate(categoryViewPrefab, categoriesViewsContainer);
		keyRemappingCategoryView.Initialize(category);
		categoryViews.Add(keyRemappingCategoryView);
		return keyRemappingCategoryView;
	}

	public void OnBotButtonClick()
	{
		scrollBar.value = Mathf.Clamp01(scrollBar.value + scrollBarButtonsSensitivity / scrollContent.sizeDelta.y);
	}

	public void OnTopButtonClick()
	{
		scrollBar.value = Mathf.Clamp01(scrollBar.value - scrollBarButtonsSensitivity / scrollContent.sizeDelta.y);
	}

	public override void Open()
	{
		base.Open();
		KeyRemappingManager.Initialize();
		scrollRect.enabled = true;
		scrollBar.value = 1f;
	}

	public void RebuildLayout()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(layoutToRebuild);
	}

	public void RefreshBindingViews()
	{
		foreach (KeyRemappingCategoryView categoryView in categoryViews)
		{
			categoryView.Refresh();
		}
	}

	public void RefreshTexts()
	{
		mainColumnTitle.text = Localizer.Get("KeyRemapping_MainColumn");
		secondaryColumnTitle.text = Localizer.Get("KeyRemapping_SecondaryColumn");
		resetDefaultText.text = Localizer.Get("KeyRemapping_ResetAll");
		foreach (KeyRemappingCategoryView categoryView in categoryViews)
		{
			categoryView.RefreshTexts();
		}
	}
}
