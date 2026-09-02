using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class JoystickSelectableDynamic : Selectable
{
	[SerializeField]
	private Selectable[] selectables;

	[SerializeField]
	private bool checkSelectablesParentCanvas;

	public Selectable[] Selectables => selectables;

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		Selectable selectable = null;
		for (int i = 0; i < selectables.Length; i++)
		{
			Selectable selectable2 = selectables[i];
			if (selectable2 == null)
			{
				continue;
			}
			if (checkSelectablesParentCanvas)
			{
				Canvas componentInParent = selectable2.GetComponentInParent<Canvas>();
				if (componentInParent != null && !componentInParent.enabled)
				{
					continue;
				}
			}
			if (selectable2.IsInteractable() && selectable2.gameObject.activeInHierarchy)
			{
				selectable = selectable2;
				break;
			}
		}
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
