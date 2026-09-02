using System;
using System.Collections;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.NightReport;
using TheLastStand.View.PlayableUnitCustomisation;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Recruitment;
using TheLastStand.View.SaveSlots;
using TheLastStand.View.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericConsent : MonoBehaviour, IOverlayUser
{
	public class Constants
	{
		public const string SmallContentLocalizedFontChilds = "SmallContent";

		public const string ContentLocalizedFontChilds = "Content";
	}

	private static GenericConsent instance;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image overlay;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private GameObject popupContainer;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private BetterButton cancelButton;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI coreText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject smallPopupContainer;

	[SerializeField]
	private Button smallCloseButton;

	[SerializeField]
	private BetterButton smallCancelButton;

	[SerializeField]
	private BetterButton smallConfirmButton;

	[SerializeField]
	private TextMeshProUGUI smallCoreText;

	[SerializeField]
	private TextMeshProUGUI smallTitleText;

	private Action onCancel;

	private Action onConfirm;

	private bool isSmallVersion;

	private bool openThisFrame;

	private bool isOpen;

	public Canvas Canvas => canvas;

	public Localizer.ParameterizedLocalizationLine CancelButtonContent { get; private set; }

	public Localizer.ParameterizedLocalizationLine ConfirmButtonContent { get; private set; }

	public string LocalizedText { get; private set; }

	public int OverlaySortingOrder => Canvas.sortingOrder - 1;

	public Localizer.ParameterizedLocalizationLine Text { get; private set; }

	public Localizer.ParameterizedLocalizationLine Title { get; private set; }

	public static bool IsOpen => instance.isOpen;

	public static bool IsWaitingForInput()
	{
		if (instance != null && instance.Canvas.enabled)
		{
			return !instance.openThisFrame;
		}
		return false;
	}

	public static GenericConsent Open(string textLocKey, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		return Open(new Localizer.ParameterizedLocalizationLine("GenericConsent_Title"), new Localizer.ParameterizedLocalizationLine(textLocKey), onConfirm, onCancel, null, null, smallVersion);
	}

	public static GenericConsent Open(string titleLocKey, string textLocKey, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		return Open(new Localizer.ParameterizedLocalizationLine(titleLocKey), new Localizer.ParameterizedLocalizationLine(textLocKey), onConfirm, onCancel, null, null, smallVersion);
	}

	public static GenericConsent Open(Localizer.ParameterizedLocalizationLine titleLocKey, string localizedText, Action onConfirm, Action onCancel, Localizer.ParameterizedLocalizationLine? confirmButtonLocKey = null, Localizer.ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		if (instance == null)
		{
			Debug.LogError("Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = titleLocKey;
		instance.LocalizedText = localizedText;
		instance.ConfirmButtonContent = confirmButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericPopup_Confirm");
		instance.CancelButtonContent = cancelButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericConsent_Cancel");
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, onCancel);
	}

	public static GenericConsent Open(Localizer.ParameterizedLocalizationLine titleLocKey, Localizer.ParameterizedLocalizationLine textLocKey, Action onConfirm, Action onCancel, Localizer.ParameterizedLocalizationLine? confirmButtonLocKey = null, Localizer.ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		if (instance == null)
		{
			Debug.LogError("Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = titleLocKey;
		instance.Text = textLocKey;
		instance.LocalizedText = string.Empty;
		instance.ConfirmButtonContent = confirmButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericPopup_Confirm");
		instance.CancelButtonContent = cancelButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericConsent_Cancel");
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, onCancel);
	}

	public static GenericConsent Open(Localizer.ParameterizedLocalizationLine textLocKey, Action onConfirm, Localizer.ParameterizedLocalizationLine? confirmButtonLocKey = null, Localizer.ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		if (instance == null)
		{
			Debug.LogError("Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = new Localizer.ParameterizedLocalizationLine("GenericConsent_Title");
		instance.Text = textLocKey;
		instance.LocalizedText = string.Empty;
		instance.ConfirmButtonContent = confirmButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericPopup_Confirm");
		instance.CancelButtonContent = cancelButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericConsent_Cancel");
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, null);
	}

	public static GenericConsent Open(Localizer.ParameterizedLocalizationLine textLocKey, Action onConfirm, Action onCancel, Localizer.ParameterizedLocalizationLine? confirmButtonLocKey = null, Localizer.ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		if (instance == null)
		{
			Debug.LogError("Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = new Localizer.ParameterizedLocalizationLine("GenericConsent_Title");
		instance.Text = textLocKey;
		instance.LocalizedText = string.Empty;
		instance.ConfirmButtonContent = confirmButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericPopup_Confirm");
		instance.CancelButtonContent = cancelButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericConsent_Cancel");
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, onCancel);
	}

	public static GenericConsent OpenLocalized(string localizedText, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		return Open(new Localizer.ParameterizedLocalizationLine("GenericConsent_Title"), localizedText, onConfirm, onCancel, null, null, smallVersion);
	}

	public static GenericConsent OpenLocalized(string titleLocKey, string localizedText, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		return Open(new Localizer.ParameterizedLocalizationLine(titleLocKey), localizedText, onConfirm, onCancel, null, null, smallVersion);
	}

	public void RefreshContent()
	{
		overlay.enabled = !TPSingleton<ACameraView>.Exist() || !(TPSingleton<ACameraView>.Instance is CameraView);
		if ((Text.key != null || !string.IsNullOrEmpty(LocalizedText)) && Title.key != null)
		{
			smallPopupContainer.SetActive(isSmallVersion && Canvas.enabled);
			popupContainer.SetActive(!isSmallVersion && Canvas.enabled);
			if (isSmallVersion)
			{
				smallCoreText.text = (string.IsNullOrEmpty(LocalizedText) ? Localizer.Get(Text) : LocalizedText);
				smallTitleText.text = Localizer.Get(Title);
				smallCancelButton.ChangeText(Localizer.Get(CancelButtonContent));
				smallConfirmButton.ChangeText(Localizer.Get(ConfirmButtonContent));
			}
			else
			{
				coreText.text = (string.IsNullOrEmpty(LocalizedText) ? Localizer.Get(Text) : LocalizedText);
				titleText.text = Localizer.Get(Title);
				cancelButton.ChangeText(Localizer.Get(CancelButtonContent));
				confirmButton.ChangeText(Localizer.Get(ConfirmButtonContent));
			}
			complexFontLocalizedParent.TargetKey = (isSmallVersion ? "SmallContent" : "Content");
			complexFontLocalizedParent.RefreshChildren();
		}
	}

	private static GenericConsent Open(Action onConfirm, Action onCancel)
	{
		instance.Canvas.enabled = true;
		instance.onConfirm = onConfirm;
		instance.onCancel = onCancel;
		instance.isOpen = true;
		instance.RefreshContent();
		if (TPSingleton<ACameraView>.Exist() && TPSingleton<ACameraView>.Instance is CameraView)
		{
			CameraView.AttenuateWorldForPopupFocus(instance);
		}
		if (TPSingleton<GameManager>.Exist())
		{
			GameController.SetState(Game.E_State.ConsentPopup);
		}
		InputManager.OnGenericConsentViewToggled(state: true);
		instance.StartCoroutine(instance.OpenThisFrameCoroutine());
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
		return instance;
	}

	private void Awake()
	{
		instance = this;
		closeButton.onClick.AddListener(delegate
		{
			StartCloseCoroutine();
		});
		confirmButton.onClick.AddListener(Confirm);
		cancelButton.onClick.AddListener(delegate
		{
			StartCloseCoroutine();
		});
		smallCloseButton.onClick.AddListener(delegate
		{
			StartCloseCoroutine();
		});
		smallConfirmButton.onClick.AddListener(Confirm);
		smallCancelButton.onClick.AddListener(delegate
		{
			StartCloseCoroutine();
		});
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void Close()
	{
		if (TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game.PreviousState != Game.E_State.ConsentPopup)
		{
			GameController.SetState(TPSingleton<GameManager>.Instance.Game.PreviousState);
		}
		if (TPSingleton<ACameraView>.Exist() && TPSingleton<ACameraView>.Instance is CameraView)
		{
			if (TPSingleton<GameManager>.Exist())
			{
				switch (TPSingleton<GameManager>.Instance.Game.State)
				{
				case Game.E_State.CharacterSheet:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<CharacterSheetPanel>.Instance);
					break;
				case Game.E_State.Recruitment:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<RecruitmentView>.Instance);
					break;
				case Game.E_State.Shopping:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<ShopView>.Instance);
					break;
				case Game.E_State.NightReport:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<NightReportPanel>.Instance);
					break;
				case Game.E_State.ProductionReport:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<ProductionReportPanel>.Instance);
					break;
				case Game.E_State.Settings:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<SettingsManager>.Instance.SettingsPanel);
					break;
				case Game.E_State.GameOver:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<GameOverPanel>.Instance);
					break;
				case Game.E_State.UnitCustomisation:
					CameraView.AttenuateWorldForPopupFocus(TPSingleton<PlayableUnitCustomisationPanel>.Instance);
					break;
				default:
					CameraView.AttenuateWorldForPopupFocus(null);
					break;
				}
			}
			else
			{
				CameraView.AttenuateWorldForPopupFocus(ApplicationManager.Application.State.GetName() switch
				{
					"Settings" => TPSingleton<SettingsManager>.Instance.SettingsPanel, 
					"SaveSlots" => TPSingleton<SaveSlotsPanel>.Instance, 
					_ => null, 
				});
			}
		}
		InputManager.OnGenericConsentViewToggled(state: false);
		instance.Canvas.enabled = false;
		ACameraView.AllowUserPan = true;
		instance.isOpen = false;
	}

	private IEnumerator CloseAtEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		Close();
	}

	private void Confirm()
	{
		StartCloseCoroutine(mustCallOnCancel: false);
		onConfirm?.Invoke();
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		instance = null;
	}

	private void OnEnable()
	{
		RefreshContent();
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshContent();
		}
	}

	private IEnumerator OpenThisFrameCoroutine()
	{
		openThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		openThisFrame = false;
	}

	private void StartCloseCoroutine(bool mustCallOnCancel = true)
	{
		if (mustCallOnCancel)
		{
			onCancel?.Invoke();
		}
		StartCoroutine(CloseAtEndOfFrame());
	}

	private void Update()
	{
		if (IsWaitingForInput())
		{
			if (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80))
			{
				StartCloseCoroutine();
			}
			if (InputManager.GetButtonDown(7) || InputManager.GetButtonDown(66))
			{
				Confirm();
			}
		}
	}
}
