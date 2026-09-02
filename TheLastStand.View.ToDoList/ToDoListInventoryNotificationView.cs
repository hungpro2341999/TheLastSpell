using System;
using System.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using TheLastStand.Model.Item;

namespace TheLastStand.View.ToDoList;

public class ToDoListInventoryNotificationView : ToDoListNotificationView
{
	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn == Game.E_DayTurn.Undefined || TPSingleton<GameManager>.Instance.Game.DayTurn == base.OnlyShowDuringDayTurn)
		{
			Display(TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots.Where((InventorySlot o) => o.IsNewItem).Count() > 0);
		}
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
