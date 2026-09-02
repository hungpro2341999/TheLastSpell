using System;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class TutorialsPanel : SettingsPageView
{
	[SerializeField]
	private ToggleGroup categoriesToggleGroup;

	[SerializeField]
	private RectTransform categoryTogglesContainer;

	[SerializeField]
	private RectTransform textBoxesContainer;

	[SerializeField]
	private TutorialCategoryToggle categoryTogglePrefab;

	[SerializeField]
	private TutorialTextBox textBoxPrefab;

	[SerializeField]
	private RectTransform categoryTogglesViewport;

	[SerializeField]
	private Scrollbar categoryTogglesScrollbar;

	[SerializeField]
	private Scrollbar textBoxesScrollbar;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private Selectable[] additionalSelectables;

	[SerializeField]
	private LayoutNavigationInitializer categoriesLayoutNavigationInitializer;

	[SerializeField]
	private Selectable selectableBelowCategories;

	[SerializeField]
	private Dictionary<TutorialCategoryToggle, E_TutorialCategory> categoryToggles = new Dictionary<TutorialCategoryToggle, E_TutorialCategory>();

	private readonly List<TutorialTextBox> textBoxes = new List<TutorialTextBox>();

	public override void Open()
	{
		base.Open();
		if (categoryToggles.Count == 0)
		{
			TPSingleton<TutorialManager>.Instance.LogError("Missing category toggles! Those should be set in editor, not at runtime!");
			InitCategoryToggles();
			joystickTarget.AddSelectables(additionalSelectables);
		}
		foreach (KeyValuePair<TutorialCategoryToggle, E_TutorialCategory> categoryToggle in categoryToggles)
		{
			categoryToggle.Key.gameObject.SetActive(TPSingleton<TutorialManager>.Instance.AnyMatchingTutorial((TheLastStand.Model.Tutorial.Tutorial t) => t.TutorialDefinition.Category == categoryToggle.Value && t.CanBeDisplayedInSettings()));
			categoryToggle.Key.Init(categoryToggle.Value, this);
		}
		categoriesLayoutNavigationInitializer.InitNavigation();
		categoryToggles.Last((KeyValuePair<TutorialCategoryToggle, E_TutorialCategory> o) => o.Key.gameObject.activeSelf).Key.Toggle.SetSelectOnDown(selectableBelowCategories);
		foreach (KeyValuePair<TutorialCategoryToggle, E_TutorialCategory> categoryToggle2 in categoryToggles)
		{
			categoryToggle2.Key.DynamicNavigationMode.RefreshNavigationMode(InputManager.IsLastControllerJoystick);
		}
		if (categoryToggles.Count > 0)
		{
			TutorialCategoryToggle key = categoryToggles.FirstOrDefault().Key;
			if (!key.Toggle.isOn)
			{
				key.Toggle.isOn = true;
			}
			else
			{
				OnCategoryToggleValueChanged(key);
			}
		}
		categoryTogglesScrollbar.value = 1f;
	}

	public void OnCategoryToggleValueChanged(TutorialCategoryToggle categoryToggle)
	{
		if (categoryToggle.Toggle.isOn)
		{
			List<TheLastStand.Model.Tutorial.Tutorial> tutorialsOfCategory = TPSingleton<TutorialManager>.Instance.GetTutorialsOfCategory(categoryToggle.Category, (TheLastStand.Model.Tutorial.Tutorial t) => t.CanBeDisplayedInSettings());
			int num;
			for (num = 0; num < tutorialsOfCategory.Count; num++)
			{
				((num < textBoxes.Count) ? textBoxes[num] : AddTextBox()).Refresh(tutorialsOfCategory[num]);
				textBoxes[num].Show();
			}
			for (; num < textBoxes.Count; num++)
			{
				textBoxes[num].Hide();
			}
			textBoxesScrollbar.value = 1f;
		}
	}

	public void OnCategoryToggleJoystickSelected(TutorialCategoryToggle categoryToggle)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem((RectTransform)categoryToggle.transform, categoryTogglesViewport, categoryTogglesScrollbar, 0.01f, 0.01f, 0.5f);
	}

	private TutorialTextBox AddTextBox()
	{
		TutorialTextBox tutorialTextBox = UnityEngine.Object.Instantiate(textBoxPrefab, textBoxesContainer);
		textBoxes.Add(tutorialTextBox);
		return tutorialTextBox;
	}

	[ContextMenu("Init Category Toggles")]
	private void InitCategoryToggles()
	{
		E_TutorialCategory[] array = (E_TutorialCategory[])Enum.GetValues(typeof(E_TutorialCategory));
		for (int i = 0; i < array.Length; i++)
		{
			TutorialCategoryToggle tutorialCategoryToggle = null;
			tutorialCategoryToggle = UnityEngine.Object.Instantiate(categoryTogglePrefab, categoryTogglesContainer);
			categoryToggles.Add(tutorialCategoryToggle, array[i]);
			categoriesToggleGroup.RegisterToggle(tutorialCategoryToggle.Toggle);
			tutorialCategoryToggle.Toggle.group = categoriesToggleGroup;
			joystickTarget.AddSelectable(tutorialCategoryToggle.Toggle);
		}
	}
}
