using System;
using System.Collections;
using DG.Tweening;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.HUD;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public abstract class OraculumHub<T> : TPSingleton<T> where T : OraculumHub<T>
{
	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected RectTransform globalRect;

	[SerializeField]
	protected GraphicRaycaster hubRaycaster;

	[SerializeField]
	protected BetterButton leaveButton;

	[SerializeField]
	protected JoystickSelectableCanvasScaler joystickCanvasScaler;

	[SerializeField]
	protected RectTransform[] dynamicWidthRectTransforms;

	[SerializeField]
	protected RectTransform[] dynamicHeightRectTransforms;

	[SerializeField]
	protected RectTransform[] globalRectContent;

	[SerializeField]
	[Tooltip("Canvas group used to fade from game view to meta shops view.")]
	private CanvasGroup gameTransitionCanvasGroup;

	[Tooltip("Activate block raycast or not on gameTransitionCanvasGroup during transitions")]
	[SerializeField]
	private bool blockRaycastInTransition;

	[SerializeField]
	protected Canvas gameTransitionCanvas;

	[SerializeField]
	private float blackScreenInDuration = 0.5f;

	[SerializeField]
	private Ease blackScreenInEase = Ease.OutSine;

	[SerializeField]
	private float blackScreenOutDuration = 0.25f;

	[SerializeField]
	private Ease blackScreenOutEase = Ease.InSine;

	private Sequence displaySequence;

	private Action shortDisplayCallback;

	private bool isShortDisplay;

	public bool Displayed { get; protected set; }

	public bool OpeningOrClosing { get; protected set; }

	public bool HideGoddesses
	{
		get
		{
			if (ApplicationManager.Application.State.GetName() == "Game" || ApplicationManager.Application.State.GetName() == "MetaShops")
			{
				return TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.HideGoddesses;
			}
			return false;
		}
	}

	public static void Display(bool show, Action onDisplayed = null, bool isShortDisplay = false, Action shortDisplayAction = null)
	{
		TPSingleton<T>.Instance.Displayed = show;
		TPSingleton<T>.Instance.isShortDisplay = isShortDisplay;
		TPSingleton<T>.Instance.shortDisplayCallback = shortDisplayAction;
		if (TPSingleton<T>.Instance.Displayed)
		{
			TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<T>.Instance.GetMusicClip());
		}
		if (TPSingleton<T>.Instance.gameTransitionCanvasGroup == null)
		{
			TPSingleton<LightningSDKManager>.Instance.HandleMetaShopTransition(LightningSDKManager.SDKEvent.HUB_SHOP);
			TPSingleton<T>.Instance.RefreshShopsScene();
			return;
		}
		TPSingleton<T>.Instance.ActivateBlockRaycastInTransition(mustBlock: true);
		TPSingleton<T>.Instance.SetActiveLeaveHubButton(TPSingleton<T>.Instance.Displayed);
		TPSingleton<T>.Instance.OnFadeToBlackStarts();
		float value = TPSingleton<T>.Instance.blackScreenInDuration + TPSingleton<T>.Instance.blackScreenOutDuration;
		if (TPSingleton<T>.Instance.isShortDisplay)
		{
			value = TPSingleton<T>.Instance.blackScreenInDuration;
		}
		if (TPSingleton<T>.Instance.Displayed)
		{
			if (!TPSingleton<T>.Instance.isShortDisplay)
			{
				TPSingleton<LightningSDKManager>.Instance.HandleMetaShopTransition(LightningSDKManager.SDKEvent.HUB_SHOP, value);
				TPSingleton<T>.Instance.displaySequence.Append(TPSingleton<T>.Instance.gameTransitionCanvasGroup.DOFade(1f, TPSingleton<T>.Instance.blackScreenInDuration).SetEase(TPSingleton<T>.Instance.blackScreenInEase).OnComplete(TPSingleton<T>.Instance.OnFadeToBlackComplete));
				TPSingleton<T>.Instance.displaySequence.Append(TPSingleton<T>.Instance.gameTransitionCanvasGroup.DOFade(0f, TPSingleton<T>.Instance.blackScreenOutDuration).SetEase(TPSingleton<T>.Instance.blackScreenOutEase).OnStart(delegate
				{
					TPSingleton<T>.Instance.gameTransitionCanvas.sortingOrder--;
				})
					.OnComplete(delegate
					{
						TPSingleton<T>.Instance.ActivateBlockRaycastInTransition(mustBlock: false);
						TPSingleton<T>.Instance.OnHubEnter(onDisplayed);
					}));
			}
			else
			{
				TPSingleton<T>.Instance.gameTransitionCanvasGroup.alpha = 1f;
				TPSingleton<T>.Instance.OnFadeToBlackComplete();
				TPSingleton<T>.Instance.displaySequence.Append(TPSingleton<T>.Instance.gameTransitionCanvasGroup.DOFade(0f, TPSingleton<T>.Instance.blackScreenOutDuration).SetEase(TPSingleton<T>.Instance.blackScreenOutEase).OnStart(delegate
				{
					TPSingleton<T>.Instance.gameTransitionCanvas.sortingOrder--;
				})
					.OnComplete(delegate
					{
						TPSingleton<T>.Instance.ActivateBlockRaycastInTransition(mustBlock: false);
						TPSingleton<T>.Instance.OnHubEnter(onDisplayed);
						TPSingleton<T>.Instance.shortDisplayCallback?.Invoke();
					}));
			}
		}
		else
		{
			if (ApplicationManager.Application.State.GetName() == "Game")
			{
				TPSingleton<LightningSDKManager>.Instance.HandleGameCycleColor(value);
			}
			else if (!TPSingleton<T>.Instance.isShortDisplay)
			{
				TPSingleton<LightningSDKManager>.Instance.HandleApplicationStateColor(ApplicationManager.Application.State, value);
			}
			if (!TPSingleton<T>.Instance.isShortDisplay)
			{
				TPSingleton<T>.Instance.displaySequence.Append(TPSingleton<T>.Instance.gameTransitionCanvasGroup.DOFade(1f, TPSingleton<T>.Instance.blackScreenInDuration).SetEase(TPSingleton<T>.Instance.blackScreenInEase).OnComplete(TPSingleton<T>.Instance.OnFadeToBlackComplete));
				TPSingleton<T>.Instance.displaySequence.Append(TPSingleton<T>.Instance.gameTransitionCanvasGroup.DOFade(0f, TPSingleton<T>.Instance.blackScreenOutDuration).SetEase(TPSingleton<T>.Instance.blackScreenOutEase).OnComplete(delegate
				{
					TPSingleton<T>.Instance.ActivateBlockRaycastInTransition(mustBlock: false);
					TPSingleton<T>.Instance.OnHubExit();
				}));
			}
			else
			{
				TPSingleton<T>.Instance.displaySequence.Append(TPSingleton<T>.Instance.gameTransitionCanvasGroup.DOFade(1f, TPSingleton<T>.Instance.blackScreenInDuration).SetEase(TPSingleton<T>.Instance.blackScreenInEase).OnComplete(delegate
				{
					TPSingleton<T>.Instance.gameTransitionCanvasGroup.alpha = 0f;
					TPSingleton<T>.Instance.OnFadeToBlackComplete();
					TPSingleton<T>.Instance.ActivateBlockRaycastInTransition(mustBlock: false);
					TPSingleton<T>.Instance.OnHubExit();
					TPSingleton<T>.Instance.shortDisplayCallback?.Invoke();
				}));
			}
		}
		EventSystem.current.SetSelectedGameObject(null);
	}

	public void SetActiveLeaveHubButton(bool isActive)
	{
		leaveButton.Interactable = isActive;
	}

	protected virtual void EnableRaycasters(bool state)
	{
		canvasGroup.interactable = state;
		canvasGroup.blocksRaycasts = state;
		hubRaycaster.enabled = state;
	}

	protected virtual AudioClip GetMusicClip()
	{
		if (!HideGoddesses)
		{
			return TPSingleton<SoundManager>.Instance.MetaShopsMusic;
		}
		return TPSingleton<SoundManager>.Instance.MetaShopsNoGoddessesMusic;
	}

	protected virtual IEnumerator InitCoroutine()
	{
		yield break;
	}

	protected virtual void OnDestroy()
	{
		leaveButton.onClick.RemoveAllListeners();
	}

	protected virtual void OnFadeToBlackComplete()
	{
		if (Displayed)
		{
			base.gameObject.SetActive(value: true);
			canvasGroup.alpha = 1f;
			gameTransitionCanvas.sortingOrder++;
			if (ApplicationManager.Application.State.GetName() == "Game")
			{
				TPSingleton<TileMapView>.Instance.Hide();
				TPSingleton<GameView>.Instance.HideHud();
			}
		}
		else
		{
			TPSingleton<T>.Instance.canvasGroup.alpha = 0f;
			SaveManager.SaveApp();
			if (ApplicationManager.Application.State.GetName() == "Game")
			{
				TPSingleton<TileMapView>.Instance.Display();
				TPSingleton<GameView>.Instance.DisplayHud();
				GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
			}
		}
	}

	protected virtual void OnFadeToBlackStarts()
	{
		OpeningOrClosing = true;
		displaySequence = DOTween.Sequence();
		if (TPSingleton<T>.Instance.Displayed)
		{
			gameTransitionCanvasGroup.enabled = true;
		}
		else if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		}
	}

	protected virtual void OnHubEnter(Action onDisplayed = null)
	{
		EnableRaycasters(state: true);
		OpeningOrClosing = false;
		onDisplayed?.Invoke();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	protected virtual void OnHubExit()
	{
		EnableRaycasters(state: false);
		Vector3 position = TPSingleton<T>.Instance.transform.position;
		position.x = (float)Screen.width * 0.5f;
		base.transform.position = position;
		TPSingleton<T>.Instance.OpeningOrClosing = false;
		TPSingleton<T>.Instance.gameObject.SetActive(value: false);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	protected virtual void RefreshShopsScene()
	{
	}

	protected virtual void Start()
	{
		StartCoroutine(InitCoroutine());
		if (ApplicationManager.Application.State.GetName() == "MetaShops")
		{
			Display(show: true);
			EnableRaycasters(state: true);
		}
		else if (ApplicationManager.Application.State.GetName() != "Credits")
		{
			canvasGroup.alpha = 0f;
			base.gameObject.SetActive(value: false);
			EnableRaycasters(state: false);
		}
	}

	protected void ActivateBlockRaycastInTransition(bool mustBlock)
	{
		if (blockRaycastInTransition)
		{
			gameTransitionCanvasGroup.blocksRaycasts = mustBlock;
		}
	}
}
