using System;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListMetaShopsNotificationView : ToDoListNotificationView
{
	[SerializeField]
	private GameObject darkShopSubNotification;

	[SerializeField]
	private GameObject lightShopSubNotification;

	public override void Refresh()
	{
		if (base.OnlyShowDuringDayTurn != Game.E_DayTurn.Undefined && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			Display(show: false);
			return;
		}
		darkShopSubNotification.SetActive(TPSingleton<DarkShopManager>.Instance.IsAnyUpgradeAffordable());
		lightShopSubNotification.SetActive(TPSingleton<LightShopManager>.Instance.IsAnyUpgradeAffordable());
		Display(MetaShopsManager.CanOpenShops() && (darkShopSubNotification.activeSelf || lightShopSubNotification.activeSelf));
		LayoutRebuilder.ForceRebuildLayoutImmediate(notificationGroupRectTransform);
		notificationRectTransform.sizeDelta = new Vector2(notificationRectTransform.sizeDelta.x, notificationGroupRectTransform.sizeDelta.y);
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		Refresh();
	}
}
