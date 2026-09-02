using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListActionPointsNotificationView : ToDoListNotificationView
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
			Object.Destroy(toDoTexts[i].gameObject);
		}
		toDoTexts.Clear();
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPoints) > 0f)
			{
				TextMeshProUGUI textMeshProUGUI = Object.Instantiate(toDoTextPrefab, notificationGroupRectTransform);
				textMeshProUGUI.text = string.Format("- {0} <style=\"Outline\">({1} {2})</style>", playableUnit.Name, playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPoints), Localizer.Get("ToDoList_ActionPointsText"));
				toDoTexts.Add(textMeshProUGUI);
			}
		}
		CheckDisplay();
	}
}
