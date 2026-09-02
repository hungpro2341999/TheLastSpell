using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;

namespace TheLastStand.View.ToDoList;

public class ToDoListMaterialsNotificationView : ToDoListNotificationView
{
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
		base.CheckDisplay();
		if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment && TPSingleton<GameManager>.Instance.Game.DayNumber == 0)
		{
			Display(show: false);
		}
	}
}
