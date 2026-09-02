using System.Collections;
using TPLib;
using TheLastStand.Controller.Unit;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Recruitment;

public class RecruitmentButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private FollowElement.FollowDatas joystickTooltipFollowDatas;

	public BetterButton Button => button;

	public void OnPointerEnter(PointerEventData eventData)
	{
		string localizationKey = string.Empty;
		switch (TPSingleton<RecruitmentView>.Instance.ComputeImpossibleRecruitmentReason())
		{
		case RecruitmentController.E_ImpossibleRecruitmentReason.None:
			RecruitmentView.WarningTooltip.Hide();
			return;
		case RecruitmentController.E_ImpossibleRecruitmentReason.NotEnoughGold:
			localizationKey = "Recruitment_NotEnoughGold";
			break;
		case RecruitmentController.E_ImpossibleRecruitmentReason.MaxAmountReach:
			localizationKey = (RecruitmentView.HasUnitSelected ? "Recruitment_MaxHeroAmountReached" : "Recruitment_MaxMageAmountReached");
			break;
		}
		if (InputManager.IsLastControllerJoystick)
		{
			RecruitmentView.WarningTooltip.FollowElement.FollowElementDatas.FollowTarget = joystickTooltipFollowDatas.FollowTarget;
			RecruitmentView.WarningTooltip.FollowElement.FollowElementDatas.Offset = joystickTooltipFollowDatas.Offset;
			if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
			{
				RecruitmentView.WarningTooltip.SetContent(localizationKey);
				RecruitmentView.WarningTooltip.Display();
			}
		}
		else
		{
			RecruitmentView.WarningTooltip.FollowElement.FollowElementDatas.FollowTarget = null;
			RecruitmentView.WarningTooltip.FollowElement.RestoreFollowDatasOffset();
			RecruitmentView.WarningTooltip.SetContent(localizationKey);
			RecruitmentView.WarningTooltip.Display();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StartCoroutine(RefreshWarningTooltipEndOfFrame());
	}

	public void OnSelect(BaseEventData eventData)
	{
		OnPointerEnter(null);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnPointerExit(null);
	}

	public void DisableWarningTooltip()
	{
		RecruitmentView.WarningTooltip.Hide();
	}

	private void OnEnable()
	{
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDisable()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnTooltipsToggled(bool state)
	{
		if (!(EventSystem.current.currentSelectedGameObject != null) || !(EventSystem.current.currentSelectedGameObject != base.gameObject))
		{
			if (state)
			{
				OnPointerEnter(null);
			}
			else
			{
				OnPointerExit(null);
			}
		}
	}

	private IEnumerator RefreshWarningTooltipEndOfFrame()
	{
		yield return null;
		if (EventSystem.current.currentSelectedGameObject != base.gameObject)
		{
			DisableWarningTooltip();
		}
	}
}
