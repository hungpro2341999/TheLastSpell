using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingAction;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListWorkersNotificationView : ToDoListNotificationView
{
	[SerializeField]
	private GameObject workersSubNotification;

	[SerializeField]
	private GameObject freeActionsSubNotification;

	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn == Game.E_DayTurn.Undefined || TPSingleton<GameManager>.Instance.Game.DayTurn == base.OnlyShowDuringDayTurn)
		{
			CheckDisplay();
		}
	}

	protected override void CheckDisplay()
	{
		List<TheLastStand.Model.Building.Building> availableBuildingsWithActions = TPSingleton<BuildingManager>.Instance.GetAvailableBuildingsWithActions(FilterWorkersActionsBuildings);
		List<TheLastStand.Model.Building.Building> availableBuildingsWithActions2 = TPSingleton<BuildingManager>.Instance.GetAvailableBuildingsWithActions(FilterFreeActionsBuildings);
		workersSubNotification.SetActive(availableBuildingsWithActions.Count > 0);
		freeActionsSubNotification.SetActive(availableBuildingsWithActions2.Count > 0);
		Display(workersSubNotification.activeSelf || freeActionsSubNotification.activeSelf);
		LayoutRebuilder.ForceRebuildLayoutImmediate(notificationGroupRectTransform);
		notificationRectTransform.sizeDelta = new Vector2(notificationRectTransform.sizeDelta.x, notificationGroupRectTransform.sizeDelta.y);
		static bool FilterFreeActionsBuildings(BuildingAction action)
		{
			if (ResourceManager.GetModifiedWorkersCost(action.BuildingActionDefinition) == 0)
			{
				return action.BuildingActionController.CanExecuteAction();
			}
			return false;
		}
		static bool FilterWorkersActionsBuildings(BuildingAction action)
		{
			int modifiedWorkersCost = ResourceManager.GetModifiedWorkersCost(action.BuildingActionDefinition);
			if (modifiedWorkersCost > 0 && modifiedWorkersCost <= TPSingleton<ResourceManager>.Instance.Workers)
			{
				return action.BuildingActionController.CanExecuteAction();
			}
			return false;
		}
	}
}
