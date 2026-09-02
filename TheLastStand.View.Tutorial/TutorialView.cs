using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Model.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Tutorial;

public class TutorialView : TPSingleton<TutorialView>
{
	[SerializeField]
	private List<TutorialPopup> tutorialPopupList = new List<TutorialPopup>();

	[SerializeField]
	private GameObject raycastBlocker;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private readonly Queue<TheLastStand.Model.Tutorial.Tutorial> tutorialsToDisplay = new Queue<TheLastStand.Model.Tutorial.Tutorial>();

	private Coroutine displayCoroutine;

	public bool DisplayCoroutineRunning => displayCoroutine != null;

	public void AddTutorialsToDisplay(List<TheLastStand.Model.Tutorial.Tutorial> tutorials)
	{
		TPSingleton<TutorialManager>.Instance.Log("Triggering Tutorial(s) " + string.Join(", ", tutorials.Select((TheLastStand.Model.Tutorial.Tutorial o) => o.TutorialDefinition.Id)), CLogLevel.DETAILED, forcePrintInUnity: true);
		foreach (TheLastStand.Model.Tutorial.Tutorial tutorial in tutorials)
		{
			tutorialsToDisplay.Enqueue(tutorial);
		}
		if (displayCoroutine == null)
		{
			displayCoroutine = StartCoroutine(DisplayCoroutine());
		}
	}

	protected override void Awake()
	{
		base.Awake();
		ToggleRaycastBlockers(state: false);
	}

	private IEnumerator DisplayCoroutine()
	{
		OnPopupsDisplayBegan();
		TutorialPopup popup = null;
		while (tutorialsToDisplay.Count > 0)
		{
			yield return SharedYields.WaitForEndOfFrame;
			InputManager.OnTutorialPopupOpen();
			TheLastStand.Model.Tutorial.Tutorial tutorial = tutorialsToDisplay.Dequeue();
			popup = OpenPopup(tutorial);
			if (!(popup == null))
			{
				TutorialPopup popupToDisplay = popup;
				yield return new WaitUntil(() => !popupToDisplay.IsOpened);
				TPSingleton<TutorialManager>.Instance.OnTutorialRead(tutorial);
			}
		}
		yield return SharedYields.WaitForEndOfFrame;
		OnPopupsDisplayOver(popup);
		displayCoroutine = null;
	}

	private void OnPopupsDisplayBegan()
	{
		ToggleRaycastBlockers(state: true);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	private void OnPopupsDisplayOver(TutorialPopup lastPopup)
	{
		ToggleRaycastBlockers(state: false);
		InputManager.OnTutorialPopupClosed();
		if (lastPopup != null && InputManager.IsLastControllerJoystick)
		{
			if (lastPopup.JoystickTargetAfterClose != null)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode();
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(lastPopup.JoystickTargetAfterClose.GetSelectionInfo());
			}
			if (lastPopup.SelectableAfterClose != null)
			{
				StartCoroutine(RedirectSelectionEndOfFrame(lastPopup.SelectableAfterClose));
			}
		}
	}

	private TutorialPopup OpenPopup(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		TPSingleton<TutorialManager>.Instance.Log("Opening TutorialPopup " + tutorial.TutorialDefinition.Id + ".", this);
		TutorialPopup tutorialPopup = tutorialPopupList.FirstOrDefault((TutorialPopup popup) => popup.Id == tutorial.TutorialDefinition.Id);
		if (tutorialPopup == null)
		{
			TPSingleton<TutorialManager>.Instance.LogWarning("Missing TutorialPopup for Id " + tutorial.TutorialDefinition.Id + " -> skipping it.");
			return null;
		}
		tutorialPopup.Open(tutorial);
		tutorialPopup.transform.SetParent(base.transform);
		return tutorialPopup;
	}

	private IEnumerator RedirectSelectionEndOfFrame(Selectable selectable)
	{
		yield return SharedYields.WaitForEndOfFrame;
		EventSystem.current.SetSelectedGameObject(selectable.gameObject);
	}

	private void ToggleRaycastBlockers(bool state)
	{
		canvasGroup.blocksRaycasts = state;
		if (raycastBlocker != null)
		{
			raycastBlocker.SetActive(state);
		}
	}
}
