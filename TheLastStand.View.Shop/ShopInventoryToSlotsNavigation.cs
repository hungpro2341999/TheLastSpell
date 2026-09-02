using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Shop;

public class ShopInventoryToSlotsNavigation : Selectable
{
	public readonly List<Selectable> ShelvesSlots = new List<Selectable>();

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		Vector3 highlightPosition = TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.transform.position;
		Selectable selectable = ShelvesSlots.OrderBy((Selectable o) => (o.transform.position - highlightPosition).sqrMagnitude).FirstOrDefault();
		if (selectable == null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.LogWarning("No valid selectable target has been found when selecting " + base.transform.name + ".");
		}
		else
		{
			StartCoroutine(RedirectSelection(selectable.gameObject));
		}
	}

	private IEnumerator RedirectSelection(GameObject selected)
	{
		EventSystem currentEventSystem = EventSystem.current;
		if (currentEventSystem.alreadySelecting)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		currentEventSystem.SetSelectedGameObject(selected);
	}
}
