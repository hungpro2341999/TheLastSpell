using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Generic;

public class AllowingCursorUI : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private bool hasCursor;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!hasCursor)
		{
			InputManager.IsPointerOverAllowingCursorUI = true;
			hasCursor = true;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (hasCursor)
		{
			InputManager.IsPointerOverAllowingCursorUI = false;
			hasCursor = false;
		}
	}

	private void OnDisable()
	{
		if (hasCursor)
		{
			InputManager.IsPointerOverAllowingCursorUI = false;
			hasCursor = false;
		}
	}
}
