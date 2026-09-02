using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListLevelUpNotificationView : ToDoListNotificationView
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
			if (playableUnit.StatsPoints > 0)
			{
				TextMeshProUGUI textMeshProUGUI = Object.Instantiate(toDoTextPrefab, notificationGroupRectTransform);
				textMeshProUGUI.text = "- " + playableUnit.Name + ((playableUnit.LevelPoints > 1) ? string.Format(" <style=\"Outline\">({0} {1})</style>", playableUnit.LevelPoints, Localizer.Get("ToDoList_LevelUpText")) : string.Empty);
				toDoTexts.Add(textMeshProUGUI);
			}
		}
		CheckDisplay();
	}
}
