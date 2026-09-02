using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.Shop;

public class ShopSellRectView : MonoBehaviour
{
	[SerializeField]
	private RectTransform sellRectTransform;

	public bool IsInRect { get; private set; }

	private void Update()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping && InventoryManager.InventoryView.DraggableItem.Displayed && InventoryManager.InventoryView.DraggableItem.ItemSlot != null)
		{
			bool flag = RectTransformUtility.RectangleContainsScreenPoint(sellRectTransform, InputManager.MousePosition);
			if (!IsInRect && flag)
			{
				IsInRect = true;
			}
			else if (IsInRect && !flag)
			{
				IsInRect = false;
			}
		}
	}
}
