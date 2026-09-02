using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListProductionNotificationView : ToDoListNotificationView
{
	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn != Game.E_DayTurn.Undefined && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			return;
		}
		for (int i = 0; i < toDoTexts.Count; i++)
		{
			UnityEngine.Object.Destroy(toDoTexts[i].gameObject);
		}
		toDoTexts.Clear();
		if (TPSingleton<BuildingManager>.Instance.ProductionReport != null)
		{
			for (int j = 0; j < TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count; j++)
			{
				TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(toDoTextPrefab, notificationGroupRectTransform);
				textMeshProUGUI.text = "- " + TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects[j].Name;
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
