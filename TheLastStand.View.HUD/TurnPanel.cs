using TMPro;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class TurnPanel : MonoBehaviour
{
	[SerializeField]
	private Canvas turnPanelCanvas;

	[SerializeField]
	private TurnMainPanel turnMainPanel;

	[SerializeField]
	private PhasePanel phasePanel;

	[SerializeField]
	private GameObject apocalypseObject;

	[SerializeField]
	private Animator apocalypseFlameAnimator;

	[SerializeField]
	private TextMeshProUGUI apocalypseLevelText;

	[SerializeField]
	private ApocalypseEffectsTooltip apocalypseTooltip;

	[SerializeField]
	private GameObject glyphsObject;

	[SerializeField]
	private TextMeshProUGUI glyphsCustomModeText;

	[SerializeField]
	private GameObject weaponsRestrictionsObject;

	[SerializeField]
	private Selectable nightEndTurnButton;

	[SerializeField]
	private Selectable dayEndTurnButton;

	[SerializeField]
	private Selectable damnedSoulsSelectable;

	[SerializeField]
	private Selectable goldSelectable;

	[SerializeField]
	private Selectable materialsSelectable;

	[SerializeField]
	private Selectable workersSelectable;

	[SerializeField]
	private Selectable enemiesLeftSelectable;

	[SerializeField]
	private Selectable apocalypseSelectable;

	[SerializeField]
	private Selectable glyphsSelectable;

	[SerializeField]
	private Selectable weaponsRestrictionsSelectable;

	public Canvas TurnPanelCanvas => turnPanelCanvas;

	public PhasePanel PhasePanel => phasePanel;

	public void Display(bool show)
	{
		turnPanelCanvas.enabled = show && UIManager.DebugToggleUI != false;
	}

	public void OnApocalypseMoreInfoButtonClick()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night || turnMainPanel.NightEndTurnButton.Interactable)
		{
			TPSingleton<ApocalypseMoreInfoPanel>.Instance.Open();
		}
	}

	public void Refresh()
	{
		RefreshTurnMainPanel();
		phasePanel.Refresh();
		RefreshJoystickNavigation();
	}

	public void RefreshTurnMainPanel()
	{
		turnMainPanel.Refresh();
	}

	private void RefreshJoystickNavigation()
	{
		Selectable selectable = (dayEndTurnButton.gameObject.activeSelf ? dayEndTurnButton : nightEndTurnButton);
		goldSelectable.SetSelectOnUp(selectable);
		materialsSelectable.SetSelectOnUp(selectable);
		workersSelectable.SetSelectOnUp(goldSelectable);
		goldSelectable.SetSelectOnDown(workersSelectable);
		materialsSelectable.SetSelectOnDown(workersSelectable);
		goldSelectable.SetSelectOnRight(materialsSelectable);
		materialsSelectable.SetSelectOnLeft(goldSelectable);
		if (damnedSoulsSelectable.gameObject.activeSelf)
		{
			selectable.SetSelectOnDown(damnedSoulsSelectable);
			damnedSoulsSelectable.SetSelectOnUp(selectable);
			damnedSoulsSelectable.SetSelectOnLeft(materialsSelectable);
			materialsSelectable.SetSelectOnRight(damnedSoulsSelectable);
			damnedSoulsSelectable.SetSelectOnDown(workersSelectable);
		}
		else
		{
			selectable.SetSelectOnDown(materialsSelectable);
		}
		if (PhasePanel.RemainingEnemiesTextEnabled)
		{
			if (damnedSoulsSelectable.gameObject.activeSelf)
			{
				enemiesLeftSelectable.SetSelectOnUp(damnedSoulsSelectable);
				damnedSoulsSelectable.SetSelectOnDown(enemiesLeftSelectable);
			}
			else
			{
				enemiesLeftSelectable.SetSelectOnUp(materialsSelectable);
				enemiesLeftSelectable.SetSelectOnLeft(workersSelectable);
				workersSelectable.SetSelectOnRight(enemiesLeftSelectable);
			}
		}
		else
		{
			workersSelectable.SetSelectOnRight(null);
		}
		apocalypseSelectable.SetMode(Navigation.Mode.Explicit);
		glyphsSelectable.SetMode(Navigation.Mode.Explicit);
		weaponsRestrictionsSelectable.SetMode(Navigation.Mode.Explicit);
		selectable.SetSelectOnRight(GetEndTurnSelectableRight());
		enemiesLeftSelectable.SetSelectOnRight(GetEndTurnSelectableRight());
		glyphsSelectable.SetSelectOnLeft(selectable);
		glyphsSelectable.SetSelectOnRight(GetGlyphSelectableRight());
		apocalypseSelectable.SetSelectOnLeft(glyphsSelectable.gameObject.activeSelf ? glyphsSelectable : selectable);
		apocalypseSelectable.SetSelectOnRight(weaponsRestrictionsSelectable.gameObject.activeSelf ? weaponsRestrictionsSelectable : null);
		weaponsRestrictionsSelectable.SetSelectOnLeft(GetWeaponsRestrictionsSelectableLeft(selectable));
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			Selectable foldButton = TPSingleton<ToDoListView>.Instance.GetFoldButton();
			workersSelectable.SetSelectOnDown(foldButton);
			foldButton.SetSelectOnUp(workersSelectable);
		}
		else
		{
			workersSelectable.SetSelectOnDown(null);
		}
	}

	private Selectable GetEndTurnSelectableRight()
	{
		if (glyphsSelectable.gameObject.activeSelf)
		{
			return glyphsSelectable;
		}
		if (apocalypseSelectable.gameObject.activeSelf)
		{
			return apocalypseSelectable;
		}
		if (!weaponsRestrictionsSelectable.gameObject.activeSelf)
		{
			return null;
		}
		return weaponsRestrictionsSelectable;
	}

	private Selectable GetGlyphSelectableRight()
	{
		if (apocalypseSelectable.gameObject.activeSelf)
		{
			return apocalypseSelectable;
		}
		if (!weaponsRestrictionsSelectable.gameObject.activeSelf)
		{
			return null;
		}
		return weaponsRestrictionsSelectable;
	}

	private Selectable GetWeaponsRestrictionsSelectableLeft(Selectable endTurnSelectable)
	{
		if (apocalypseSelectable.gameObject.activeSelf)
		{
			return apocalypseSelectable;
		}
		if (!glyphsSelectable.gameObject.activeSelf)
		{
			return endTurnSelectable;
		}
		return glyphsSelectable;
	}

	private void Start()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Count > 0)
		{
			glyphsObject.SetActive(value: true);
			if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
			{
				glyphsCustomModeText.enabled = true;
				glyphsCustomModeText.text = $"+{TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints()}";
			}
		}
		if (ApocalypseManager.CurrentApocalypseLevel > 0)
		{
			apocalypseObject.SetActive(value: true);
			apocalypseLevelText.text = $"<style=Bad>{ApocalypseManager.CurrentApocalypseLevel}</style>";
			apocalypseFlameAnimator.Play("WorldMapFlamesIdle");
			apocalypseTooltip.SetApocalypseModifierStepDefinitions(ApocalypseManager.CurrentApocalypseModifierStepDefinitions);
		}
		if (!TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.AreAllUnlockedFamiliesSelected())
		{
			weaponsRestrictionsObject.SetActive(value: true);
		}
	}
}
