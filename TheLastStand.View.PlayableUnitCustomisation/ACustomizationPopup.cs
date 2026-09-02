using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public abstract class ACustomizationPopup : MonoBehaviour, IOverlayUser
{
	public enum E_ErrorCause
	{
		None,
		MinSize,
		MaxSize,
		WrongCharacter,
		InvalidLayerValue
	}

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

	protected string previousValue;

	public virtual int OverlaySortingOrder => canvas.sortingOrder - 2;

	public virtual void Close()
	{
		CameraView.AttenuateWorldForPopupFocus(TPSingleton<PlayableUnitCustomisationPanel>.Instance);
		fadeTween?.Kill();
		fadeTween = canvasGroup.DOFade(0f, 0.25f).OnComplete(delegate
		{
			canvas.enabled = false;
			if (joystickTargetOnClosed != null && InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject(joystickTargetOnClosed.gameObject);
			}
		});
	}

	public abstract void OnCloseButtonClicked();

	public abstract void OnValidateButtonClicked();

	public virtual void OnValueChanged(string value)
	{
		if (CheckValidity(value) != E_ErrorCause.None)
		{
			validateButton.interactable = false;
			validateButton.image.color = new Color(1f, 1f, 1f, 0.35f);
		}
		else
		{
			validateButton.interactable = true;
			validateButton.image.color = new Color(1f, 1f, 1f, 1f);
			errorText.text = string.Empty;
		}
	}

	public virtual void Open()
	{
		CameraView.AttenuateWorldForPopupFocus(this);
		fadeTween?.Kill();
		canvas.enabled = true;
		fadeTween = canvasGroup.DOFade(1f, 0.25f);
		if (fontLocalizedParent != null)
		{
			fontLocalizedParent.RefreshChildren();
		}
		errorText.text = string.Empty;
		inputField.Select();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
	}

	protected virtual E_ErrorCause CheckValidity(string value)
	{
		return E_ErrorCause.None;
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
}
