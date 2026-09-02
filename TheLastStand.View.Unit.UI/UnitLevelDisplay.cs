using System.Collections;
using TMPro;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class UnitLevelDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI levelText;

	[SerializeField]
	private Image levelGauge;

	[SerializeField]
	[Range(0.01f, 5f)]
	private float levelGaugeFillSpeed = 1f;

	[SerializeField]
	private Animator levelUpFXAnimator;

	[SerializeField]
	private Animator levelUpNotificationAnimator;

	[SerializeField]
	private Button levelUpNotifButton;

	[SerializeField]
	private AudioClip levelUpAudioClip;

	[SerializeField]
	private UnitExperienceTooltipDisplayer unitExperienceTooltipDisplayer;

	[SerializeField]
	private GamepadInputDisplay gamepadInputDisplay;

	private int currentLevel = 1;

	public PlayableUnit PlayableUnit { get; set; }

	public UnitExperienceTooltipDisplayer UnitExperienceTooltipDisplayer => unitExperienceTooltipDisplayer;

	public void Refresh(bool instant = true)
	{
		if (instant)
		{
			currentLevel = (int)PlayableUnit.Level;
			levelText.text = $"{currentLevel}";
			levelGauge.fillAmount = PlayableUnit.ExperienceInCurrentLevel / PlayableUnit.ExperienceNeededToNextLevel;
			DisplayLevelUpNotif(PlayableUnit.StatsPoints > 0);
		}
		else
		{
			StartCoroutine(RefreshExperience());
		}
	}

	public void RefreshButton(bool interactable)
	{
		if (levelUpNotifButton != null)
		{
			levelUpNotifButton.interactable = interactable;
		}
	}

	private void DisplayLevelUpNotif(bool show)
	{
		show = show && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
		levelUpNotificationAnimator.gameObject.SetActive(show);
		RefreshButton(show && !TPSingleton<UnitLevelUpView>.Instance.IsOpened);
		if (gamepadInputDisplay != null)
		{
			gamepadInputDisplay.gameObject.SetActive(show);
		}
	}

	private void OnDisable()
	{
		DisplayLevelUpNotif(show: false);
	}

	private IEnumerator RefreshExperience()
	{
		if ((double)currentLevel < PlayableUnit.Level || PlayableUnit.StatsPoints == 0)
		{
			DisplayLevelUpNotif(show: false);
		}
		yield return SharedYields.WaitForSeconds(RandomManager.GetRandomRange(this, 0f, 0.5f));
		while ((double)currentLevel < PlayableUnit.Level || levelGauge.fillAmount < PlayableUnit.ExperienceInCurrentLevel / PlayableUnit.ExperienceNeededToNextLevel)
		{
			levelGauge.fillAmount += Time.deltaTime * levelGaugeFillSpeed;
			if ((double)currentLevel < PlayableUnit.Level && levelGauge.fillAmount >= 1f)
			{
				currentLevel++;
				levelText.text = $"{currentLevel}";
				levelGauge.fillAmount = 0f;
				levelUpFXAnimator.SetTrigger("Appear");
				TPSingleton<UIManager>.Instance.PlayAudioClip(levelUpAudioClip);
				DisplayLevelUpNotif(show: true);
			}
			else if ((double)currentLevel == PlayableUnit.Level && levelGauge.fillAmount >= PlayableUnit.ExperienceInCurrentLevel / PlayableUnit.ExperienceNeededToNextLevel)
			{
				levelGauge.fillAmount = PlayableUnit.ExperienceInCurrentLevel / PlayableUnit.ExperienceNeededToNextLevel;
			}
			yield return null;
		}
	}
}
