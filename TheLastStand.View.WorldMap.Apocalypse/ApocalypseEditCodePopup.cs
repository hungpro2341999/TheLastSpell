using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Controller.Apocalypse;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Apocalypse;

public class ApocalypseEditCodePopup : MonoBehaviour
{
	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected TextMeshProUGUI errorText;

	[SerializeField]
	protected TMP_InputField inputField;

	[SerializeField]
	protected SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	protected BetterButton validateButton;

	[SerializeField]
	protected BetterButton closeButton;

	[SerializeField]
	private Selectable joystickTargetOnClosed;

	private Tween fadeTween;

	private ApocalypseCodeGenerator.ApocalypseCodeDecodingData decodingData;

	public bool Displayed { get; private set; }

	public void Close()
	{
		if (!Displayed)
		{
			return;
		}
		Displayed = false;
		if (TPSingleton<ApocalypseSelectionPanel>.Instance != null && TPSingleton<ApocalypseSelectionPanel>.Instance.Displayed)
		{
			TPSingleton<ApocalypseSelectionPanel>.Instance.CodeSharingView.IsEditingCode = false;
		}
		fadeTween?.Kill();
		fadeTween = canvasGroup.DOFade(0f, 0.25f).OnComplete(delegate
		{
			canvas.enabled = false;
			if (InputManager.IsLastControllerJoystick)
			{
				if (joystickTargetOnClosed != null)
				{
					EventSystem.current.SetSelectedGameObject(joystickTargetOnClosed.gameObject);
				}
				else if (TPSingleton<ApocalypseSelectionPanel>.Instance != null && TPSingleton<ApocalypseSelectionPanel>.Instance.Displayed)
				{
					TPSingleton<ApocalypseSelectionPanel>.Instance.SelectDefaultJoystickSelectableAfterAFrame();
				}
			}
		});
	}

	public void OnCloseButtonClicked()
	{
		Close();
	}

	public void OnValidateButtonClicked()
	{
		if (decodingData != null && decodingData.Success)
		{
			if (TPSingleton<ApocalypseSelectionPanel>.Instance != null && TPSingleton<ApocalypseSelectionPanel>.Instance.Displayed)
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.ApplyCodeSharing(decodingData);
			}
			Close();
		}
	}

	public void OnValueChanged(string apocalypseCode)
	{
		if (TryRemoveLineBreaks(ref apocalypseCode))
		{
			inputField.SetTextWithoutNotify(apocalypseCode);
		}
		decodingData = ApocalypseCodeGenerator.TryDecodeCode(apocalypseCode);
		if (decodingData.Success)
		{
			SetValidateButtonInteractable(isInteractable: true);
			errorText.text = string.Empty;
		}
		else
		{
			errorText.text = decodingData.GetFailureMessage();
			SetValidateButtonInteractable(isInteractable: false);
		}
	}

	public void Open()
	{
		if (!Displayed)
		{
			Displayed = true;
			fadeTween?.Kill();
			canvas.enabled = true;
			fadeTween = canvasGroup.DOFade(1f, 0.25f);
			if (fontLocalizedParent != null)
			{
				fontLocalizedParent.RefreshChildren();
			}
			errorText.text = string.Empty;
			inputField.text = string.Empty;
			inputField.Select();
			SetValidateButtonInteractable(isInteractable: false);
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			}
		}
	}

	private void SetValidateButtonInteractable(bool isInteractable)
	{
		validateButton.interactable = isInteractable;
		validateButton.image.color = new Color(1f, 1f, 1f, isInteractable ? 1f : 0.35f);
	}

	private void Start()
	{
		validateButton.onClick.AddListener(OnValidateButtonClicked);
		closeButton.onClick.AddListener(OnCloseButtonClicked);
		inputField.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnDestroy()
	{
		validateButton.onClick.RemoveListener(OnValidateButtonClicked);
		closeButton.onClick.RemoveListener(OnCloseButtonClicked);
		inputField.onValueChanged.RemoveListener(OnValueChanged);
	}

	private bool TryRemoveLineBreaks(ref string apocalypseCode)
	{
		bool result = false;
		string[] array = new string[3] { "\r", "\n", "\r\n" };
		foreach (string text in array)
		{
			if (apocalypseCode.Contains(text))
			{
				result = true;
				apocalypseCode = apocalypseCode.Replace(text, string.Empty);
			}
		}
		return result;
	}
}
