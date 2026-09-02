using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View;

public class RetryTutorialPanel : TPSingleton<RetryTutorialPanel>, IOverlayUser
{
	private static class Constants
	{
		public const string TitleTextKey = "TutorialGameOver_Title";

		public const string ContentTextKey = "TutorialGameOver_Content";

		public const string RetryButtonTextKey = "TutorialGameOver_Retry";

		public const string SkipButtonTextKey = "TutorialGameOver_Skip";
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI contentText;

	[SerializeField]
	private TextMeshProUGUI retryButtonText;

	[SerializeField]
	private TextMeshProUGUI skipButtonText;

	[SerializeField]
	private Button retryButton;

	[SerializeField]
	private Button skipButton;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	public Canvas Canvas => canvas;

	public int OverlaySortingOrder => TPSingleton<RetryTutorialPanel>.Instance.canvas.sortingOrder - 2;

	public void Open()
	{
		CLoggerManager.Log("Opening RetryTutorialPanel.", this, LogType.Log, CLogLevel.DETAILED);
		complexFontLocalizedParent?.RefreshChildren();
		RefreshLocalizedText();
		Canvas.enabled = true;
		canvasGroup.blocksRaycasts = true;
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		retryButton.onClick.AddListener(OnRetryButtonClicked);
		skipButton.onClick.AddListener(OnSkipButtonClicked);
		Canvas.enabled = false;
	}

	private void OnRetryButtonClicked()
	{
		canvas.enabled = false;
		GameController.RestartLevel();
	}

	private void OnSkipButtonClicked()
	{
		canvas.enabled = false;
		ApplicationManager.Application.TutorialDone = true;
		TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		GameController.GoToMetaShops();
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		retryButton.onClick.RemoveListener(OnRetryButtonClicked);
		skipButton.onClick.RemoveListener(OnSkipButtonClicked);
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshLocalizedText();
		}
	}

	private void RefreshLocalizedText()
	{
		titleText.text = Localizer.Get("TutorialGameOver_Title");
		contentText.text = Localizer.Get("TutorialGameOver_Content");
		retryButtonText.text = Localizer.Get("TutorialGameOver_Retry");
		skipButtonText.text = Localizer.Get("TutorialGameOver_Skip");
	}

	private void Update()
	{
		if (Canvas.enabled)
		{
			if (InputManager.GetButtonDown(80))
			{
				OnSkipButtonClicked();
			}
			else if (InputManager.GetButtonDown(7) || InputManager.GetButtonDown(66))
			{
				OnRetryButtonClicked();
			}
		}
	}
}
