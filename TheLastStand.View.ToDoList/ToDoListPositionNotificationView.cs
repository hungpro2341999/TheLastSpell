using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListPositionNotificationView : ToDoListNotificationView
{
	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn != Game.E_DayTurn.Undefined && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			return;
		}
		for (int num = toDoTexts.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(toDoTexts[num].gameObject);
		}
		toDoTexts.Clear();
		int i = 0;
		for (int count = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i < count; i++)
		{
			if (!TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].MovedThisDay || TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].OriginTile.HasFog)
			{
				TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(toDoTextPrefab, notificationGroupRectTransform);
				textMeshProUGUI.text = "- " + TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitName;
				toDoTexts.Add(textMeshProUGUI);
			}
		}
		CheckDisplay();
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
