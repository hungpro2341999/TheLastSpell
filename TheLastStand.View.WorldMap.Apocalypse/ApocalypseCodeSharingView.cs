using System.Collections;
using DG.Tweening;
using Rewired;
using TPLib.Yield;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseCodeSharingView : MonoBehaviour
{
	[SerializeField]
	private BetterButton editCodeButton;

	[SerializeField]
	private BetterButton copyCodeButton;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private ApocalypseEditCodePopup editCodePopup;

	[SerializeField]
	private CanvasGroup copyFeedbackCanvasGroup;

	private Tween copyFeedbackApparitionTween;

	private Tween copyFeedbackDisappearTween;

	private string currentApocalypseCode;

	public ApocalypseEditCodePopup ApocalypseEditCodePopup => editCodePopup;

	public bool IsEditingCode { get; set; }

	public void RefreshApocalypseCode()
	{
		currentApocalypseCode = string.Empty;
		if (ApocalypseManager.CurrentApocalypse != null && ApocalypseManager.CurrentApocalypse.ModifierStepDefinitions.Count > 0)
		{
			currentApocalypseCode = ApocalypseCodeGenerator.GenerateCode(ApocalypseManager.CurrentApocalypse);
		}
	}

	public void RefreshVisibleState()
	{
		Display(!TheLastStand.Manager.InputManager.IsLastControllerJoystick);
	}

	private void Display(bool mustDisplay)
	{
		if (canvasGroup != null)
		{
			canvasGroup.alpha = (mustDisplay ? 1f : 0f);
		}
	}

	private void OnApocalypseLevelChanged()
	{
		if (ApocalypseManager.CurrentApocalypse != null)
		{
			SetCopyButtonInteractable(ApocalypseManager.CurrentApocalypse.CurrentLevel > 0);
			RefreshApocalypseCode();
		}
	}

	private void OnCopyCodeButtonClick()
	{
		GUIUtility.systemCopyBuffer = currentApocalypseCode;
		if (copyFeedbackApparitionTween != null)
		{
			copyFeedbackApparitionTween.Kill();
		}
		if (copyFeedbackDisappearTween != null)
		{
			copyFeedbackDisappearTween.Kill();
		}
		copyFeedbackApparitionTween = copyFeedbackCanvasGroup.DOFade(1f, 0.25f).OnComplete(delegate
		{
			StartCoroutine(WaitThenPlayDisappear());
		});
	}

	private void OnEditCodeButtonClick()
	{
		IsEditingCode = true;
		editCodePopup.Open();
	}

	private void SetCopyButtonInteractable(bool isInteractable)
	{
		if (!(copyCodeButton == null))
		{
			copyCodeButton.Interactable = isInteractable;
		}
	}

	private void Start()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		if (ApocalypseManager.CurrentApocalypse != null)
		{
			ApocalypseManager.CurrentApocalypse.ApocalypseController.OnApocalypseLevelComputed += OnApocalypseLevelChanged;
		}
		OnApocalypseLevelChanged();
		if (editCodeButton != null)
		{
			editCodeButton.onClick.AddListener(OnEditCodeButtonClick);
		}
		if (copyCodeButton != null)
		{
			copyCodeButton.onClick.AddListener(OnCopyCodeButtonClick);
		}
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		if (ApocalypseManager.CurrentApocalypse != null)
		{
			ApocalypseManager.CurrentApocalypse.ApocalypseController.OnApocalypseLevelComputed -= OnApocalypseLevelChanged;
		}
		if (editCodeButton != null)
		{
			editCodeButton.onClick.RemoveListener(OnEditCodeButtonClick);
		}
		if (copyCodeButton != null)
		{
			copyCodeButton.onClick.RemoveListener(OnCopyCodeButtonClick);
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		bool mustDisplay = controllerType != ControllerType.Joystick;
		Display(mustDisplay);
	}

	private IEnumerator WaitThenPlayDisappear()
	{
		yield return SharedYields.WaitForSeconds(1f);
		copyFeedbackDisappearTween = copyFeedbackCanvasGroup.DOFade(0f, 0.25f);
	}
}
