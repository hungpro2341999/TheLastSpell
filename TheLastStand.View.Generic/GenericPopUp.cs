using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericPopUp : MonoBehaviour, IOverlayUser
{
	private static GenericPopUp instance;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI coreText;

	[SerializeField]
	private HyperlinkListener hyperlinkListener;

	[SerializeField]
	private Image overlay;

	[SerializeField]
	private Image titleIcon;

	[SerializeField]
	private TextMeshProUGUI titleText;

	private Action onCancel;

	public static bool IsOpen => instance.Canvas.enabled;

	public Localizer.ParameterizedLocalizationLine ButtonContent { get; private set; }

	public Canvas Canvas => canvas;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public Queue<Localizer.ParameterizedLocalizationLine> Texts { get; private set; } = new Queue<Localizer.ParameterizedLocalizationLine>();

	public Queue<Localizer.ParameterizedLocalizationLine> Titles { get; private set; } = new Queue<Localizer.ParameterizedLocalizationLine>();

	public static GenericPopUp Open(string titleLocKey, string textLocKey, Sprite titleIcon = null, string okButtonLocKey = null, Action onCancel = null)
	{
		return Open(new List<Localizer.ParameterizedLocalizationLine>
		{
			new Localizer.ParameterizedLocalizationLine(titleLocKey)
		}, new List<Localizer.ParameterizedLocalizationLine>
		{
			new Localizer.ParameterizedLocalizationLine(textLocKey)
		}, titleIcon, (okButtonLocKey == null) ? ((Localizer.ParameterizedLocalizationLine?)null) : new Localizer.ParameterizedLocalizationLine?(new Localizer.ParameterizedLocalizationLine(okButtonLocKey)), onCancel);
	}

	public static GenericPopUp Open(Localizer.ParameterizedLocalizationLine titleLocKey, Localizer.ParameterizedLocalizationLine textLocKey, Sprite titleIcon = null, string okButtonLocKey = null, Action onCancel = null)
	{
		return Open(new List<Localizer.ParameterizedLocalizationLine> { titleLocKey }, new List<Localizer.ParameterizedLocalizationLine> { textLocKey }, titleIcon, (okButtonLocKey == null) ? ((Localizer.ParameterizedLocalizationLine?)null) : new Localizer.ParameterizedLocalizationLine?(new Localizer.ParameterizedLocalizationLine(okButtonLocKey)), onCancel);
	}

	public static GenericPopUp Open(List<string> titleLocKeys, List<string> textLocKeys, List<string[]> formatsParameters, Sprite titleIcon = null, string okButtonLocKey = null, Action onCancel = null)
	{
		List<Localizer.ParameterizedLocalizationLine> titleLocKeys2 = titleLocKeys.ConvertAll((string o) => new Localizer.ParameterizedLocalizationLine(o));
		List<Localizer.ParameterizedLocalizationLine> list = new List<Localizer.ParameterizedLocalizationLine>();
		for (int num = 0; num < textLocKeys.Count; num++)
		{
			list.Add(new Localizer.ParameterizedLocalizationLine
			{
				key = textLocKeys[num],
				parameters = formatsParameters[num]
			});
		}
		return Open(titleLocKeys2, list, titleIcon, (okButtonLocKey == null) ? ((Localizer.ParameterizedLocalizationLine?)null) : new Localizer.ParameterizedLocalizationLine?(new Localizer.ParameterizedLocalizationLine(okButtonLocKey)), onCancel);
	}

	public static GenericPopUp Open(List<Localizer.ParameterizedLocalizationLine> titleLocKeys, List<Localizer.ParameterizedLocalizationLine> textLocKeys, Sprite titleIcon = null, string okButtonLocKey = null, Action onCancel = null)
	{
		return Open(titleLocKeys, textLocKeys, titleIcon, (okButtonLocKey == null) ? ((Localizer.ParameterizedLocalizationLine?)null) : new Localizer.ParameterizedLocalizationLine?(new Localizer.ParameterizedLocalizationLine(okButtonLocKey)), onCancel);
	}

	private static GenericPopUp Open(List<Localizer.ParameterizedLocalizationLine> titleLocKeys, List<Localizer.ParameterizedLocalizationLine> textLocKeys, Sprite titleIcon = null, Localizer.ParameterizedLocalizationLine? okButtonLocKey = null, Action onCancel = null)
	{
		if (instance == null)
		{
			TPSingleton<UIManager>.Instance.LogError("Someone tried to open a generic popup, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Canvas.enabled = true;
		instance.onCancel = onCancel;
		instance.Titles.Clear();
		instance.Texts.Clear();
		titleLocKeys.ForEach(delegate(Localizer.ParameterizedLocalizationLine o)
		{
			instance.Titles.Enqueue(o);
		});
		textLocKeys.ForEach(delegate(Localizer.ParameterizedLocalizationLine o)
		{
			instance.Texts.Enqueue(o);
		});
		instance.titleIcon.sprite = titleIcon;
		instance.ButtonContent = okButtonLocKey ?? new Localizer.ParameterizedLocalizationLine("GenericPopup_Confirm");
		instance.DisplayNextContent();
		CameraView.AttenuateWorldForPopupFocus(instance);
		return instance;
	}

	private void Awake()
	{
		instance = this;
		confirmButton.onClick.AddListener(CloseOrNext);
	}

	private void Close()
	{
		CameraView.AttenuateWorldForPopupFocus(null);
		instance.onCancel?.Invoke();
		instance.Canvas.enabled = false;
	}

	private IEnumerator CloseAtEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		Close();
	}

	private void CloseOrNext()
	{
		if (Titles.Count == 0)
		{
			StartCoroutine(CloseAtEndOfFrame());
		}
		else
		{
			DisplayNextContent();
		}
	}

	private void DisplayNextContent()
	{
		overlay.enabled = !TPSingleton<ACameraView>.Exist();
		if (Texts.Count != 0)
		{
			coreText.text = Localizer.Get(Texts.Dequeue());
			hyperlinkListener?.ForceRefresh();
			titleText.text = Localizer.Get(Titles.Dequeue());
			confirmButton.ChangeText(Localizer.Get(ButtonContent));
			titleIcon.gameObject.SetActive(titleIcon.sprite != null);
			simpleFontLocalizedParent?.RefreshChildren();
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void Update()
	{
		if (instance.Canvas.enabled && (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(7) || InputManager.GetButtonDown(66) || InputManager.GetButtonDown(80)))
		{
			CloseOrNext();
		}
	}
}
