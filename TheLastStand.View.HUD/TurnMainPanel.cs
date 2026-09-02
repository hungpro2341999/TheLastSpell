using System;
using System.Collections;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.HUD;

public class TurnMainPanel : MonoBehaviour
{
	private static class AnimatorParameters
	{
		public static string ProductionPhase = "ProductionPhase";

		public static string DeploymentPhase = "DeploymentPhase";

		public static string NightTurnEnemies = "NightTurnEnemies";

		public static string NightTurnHeroes = "NightTurnHeroes";
	}

	[SerializeField]
	private Animator backgroundAnimator;

	[SerializeField]
	private GameObject dayTextBoxGameObject;

	[SerializeField]
	private TextMeshProUGUI dayCycleText;

	[SerializeField]
	private TextMeshProUGUI dayCycleNoNumberText;

	[SerializeField]
	private TextMeshProUGUI dayCycleNumberText;

	[SerializeField]
	private TextMeshProUGUI dayPhaseNameText;

	[SerializeField]
	private GameObject nightTextBoxGameObject;

	[SerializeField]
	private TextMeshProUGUI nightCycleText;

	[SerializeField]
	private TextMeshProUGUI nightCycleNumberText;

	[SerializeField]
	private TextMeshProUGUI nightTurnText;

	[SerializeField]
	private TextMeshProUGUI nightTurnNumberText;

	[SerializeField]
	private TextMeshProUGUI nightPhaseNameText;

	[SerializeField]
	private BetterButton nightEndTurnButton;

	[SerializeField]
	private BetterButton dayEndTurnButton;

	public BetterButton NightEndTurnButton => nightEndTurnButton;

	public void OnEndTurnButtonClick()
	{
		GameManager.HandleEndTurnInput();
	}

	public void Refresh()
	{
		dayEndTurnButton.gameObject.SetActive(TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day);
		nightEndTurnButton.gameObject.SetActive(TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night);
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			nightTextBoxGameObject.SetActive(value: false);
			dayTextBoxGameObject.SetActive(value: true);
			if (TPSingleton<GameManager>.Instance.Game.DayNumber > 0)
			{
				dayCycleText.gameObject.SetActive(value: true);
				dayCycleNoNumberText.gameObject.SetActive(value: false);
				dayCycleText.text = Localizer.Get("Cycle_Day") ?? "";
			}
			else
			{
				dayCycleText.gameObject.SetActive(value: false);
				dayCycleNoNumberText.gameObject.SetActive(value: true);
				dayCycleNoNumberText.text = Localizer.Get("Cycle_Day") ?? "";
			}
			dayCycleNumberText.text = ((TPSingleton<GameManager>.Instance.Game.DayNumber > 0) ? TPSingleton<GameManager>.Instance.Game.DayNumber.ToString() : string.Empty);
			switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
			{
			case Game.E_DayTurn.Production:
				dayPhaseNameText.text = Localizer.Get("TurnInfoPanelTitle_ProductionPhase");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, value: true);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, value: false);
				break;
			case Game.E_DayTurn.Deployment:
				dayPhaseNameText.text = Localizer.Get("TurnInfoPanelTitle_DeploymentPhase");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, value: true);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, value: false);
				break;
			}
			break;
		case Game.E_Cycle.Night:
			dayTextBoxGameObject.SetActive(value: false);
			nightTextBoxGameObject.SetActive(value: true);
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
			{
				StopCoroutine(WaitForDyingEnemiesThenUnlockNightEndTurnButton());
				StartCoroutine(WaitForDyingEnemiesThenUnlockNightEndTurnButton());
			}
			else
			{
				nightEndTurnButton.Interactable = GameController.CanEndPlayerTurn();
			}
			nightCycleText.text = Localizer.Get("Cycle_Night");
			nightCycleNumberText.text = TPSingleton<GameManager>.Instance.Game.DayNumber.ToString();
			nightTurnText.text = Localizer.Get("Phase_Turn");
			nightTurnNumberText.text = TPSingleton<GameManager>.Instance.Game.CurrentNightHour.ToString();
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.PlayableUnits:
				nightPhaseNameText.text = Localizer.Get("TurnInfoPanelTitle_PlayerTurn");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, value: true);
				break;
			case Game.E_NightTurn.EnemyUnits:
				nightPhaseNameText.text = Localizer.Get("TurnInfoPanelTitle_EnemyTurn");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, value: false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, value: true);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, value: false);
				break;
			}
			break;
		}
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshLocalizedTexts));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(RefreshLocalizedTexts));
	}

	private void RefreshLocalizedTexts()
	{
		dayCycleText.text = Localizer.Get("Cycle_Day") ?? "";
		dayCycleNoNumberText.text = Localizer.Get("Cycle_Day") ?? "";
		TextMeshProUGUI textMeshProUGUI = dayPhaseNameText;
		textMeshProUGUI.text = TPSingleton<GameManager>.Instance.Game.DayTurn switch
		{
			Game.E_DayTurn.Production => Localizer.Get("TurnInfoPanelTitle_ProductionPhase"), 
			Game.E_DayTurn.Deployment => Localizer.Get("TurnInfoPanelTitle_DeploymentPhase"), 
			_ => dayPhaseNameText.text, 
		};
	}

	private IEnumerator WaitForDyingEnemiesThenUnlockNightEndTurnButton()
	{
		bool canUnlockButton = false;
		yield return new WaitUntil(delegate
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
			{
				return true;
			}
			if (!EnemyUnitManager.IsThereAnyEnemyDying())
			{
				canUnlockButton = true;
				return true;
			}
			return false;
		});
		if (canUnlockButton)
		{
			nightEndTurnButton.Interactable = GameController.CanEndPlayerTurn();
		}
	}
}
