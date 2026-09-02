using TheLastStand.Framework;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

[RequireComponent(typeof(RectTransform))]
public class HandlerDropdownItem : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private Scrollbar scrollbar;

	public void OnSelect(BaseEventData eventData)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			GUIHelpers.AdjustScrollViewToFocusedItem((RectTransform)base.transform, scrollViewport, scrollbar, 0.01f, 0.01f);
		}
	}
}
