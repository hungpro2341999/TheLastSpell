using TPLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dev;

public class TestPointerEvents : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		TPDebug.Log("The cursor ENTERED the selectable UI element.", this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TPDebug.Log("The cursor EXITED the selectable UI element.", this);
	}
}
