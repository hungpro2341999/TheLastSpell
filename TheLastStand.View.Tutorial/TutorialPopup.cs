using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tutorial;

public class TutorialPopup : MonoBehaviour
{
	public enum ConfirmButtonType
	{
		Okay,
		Next
	}

	[Serializable]
	private struct ImagesRow
	{
		public GameObject container;

		public List<Image> images;
	}

	public static class Constants
	{
		public const string PopupConfirmationOkay = "TutorialPopupConfirmation_OK";

		public const string PopupConfirmationNext = "TutorialPopupConfirmation_Next";
	}

	[SerializeField]
	private string id = string.Empty;

	[SerializeField]
	private float baseVerticalSize = 72f;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform backgroundTransform;

	[SerializeField]
	private VerticalLayoutGroup verticalLayoutGroup;

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private RectTransform middleBackgroundTransform;

	[SerializeField]
	private List<GameObject> middleBackgroundList;

	[SerializeField]
	private List<ImagesRow> imagesRows;

	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	[SerializeField]
	private ConfirmButtonType confirmButtonType;

	[SerializeField]
	private HUDJoystickTarget joystickTargetAfterClose;

	[SerializeField]
	private Selectable selectableAfterClose;

	[SerializeField]
	private AudioClip openAudioClip;

	private List<LocalizedFont> fontList = new List<LocalizedFont>();

	private List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();

	private TheLastStand.Model.Tutorial.Tutorial tutorial;

	public string Id => id;

	public bool IsOpened => base.gameObject.activeSelf;

	public HUDJoystickTarget JoystickTargetAfterClose => joystickTargetAfterClose;

	public Selectable SelectableAfterClose => selectableAfterClose;

	public void Close()
	{
		DisableContents();
		base.gameObject.SetActive(value: false);
	}

	public void Open(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		SoundManager.PlayAudioClip(openAudioClip);
		this.tutorial = tutorial;
		base.gameObject.SetActive(value: true);
		Refresh();
	}

	public void SetSelectableAfterClose(Selectable selectable)
	{
		selectableAfterClose = selectable;
	}

	private void AutoResize()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
		verticalLayoutGroup.childAlignment = ((rectTransform.pivot.y == 0f) ? TextAnchor.LowerCenter : ((rectTransform.pivot.y == 1f) ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter));
		int num;
		for (num = middleBackgroundList.Count((GameObject background) => background.activeSelf); (contentTransform.sizeDelta.y - baseVerticalSize) / middleBackgroundTransform.sizeDelta.y > (float)num && num < middleBackgroundList.Count - 1; num++)
		{
			middleBackgroundList.First((GameObject background) => !background.activeSelf).SetActive(value: true);
		}
		while ((contentTransform.sizeDelta.y - baseVerticalSize) / middleBackgroundTransform.sizeDelta.y <= (float)(num - 1) && num > 0)
		{
			middleBackgroundList.First((GameObject background) => background.activeSelf).SetActive(value: false);
			num--;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundTransform);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, backgroundTransform.rect.height);
		rectTransform.ForceUpdateRectTransforms();
	}

	private void Awake()
	{
		fontList.AddRange(base.transform.GetComponentsInChildren<LocalizedFont>(includeInactive: true));
		textList.AddRange(contentTransform.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true));
		if (string.IsNullOrEmpty(Id))
		{
			TPSingleton<TutorialManager>.Instance.LogError("Missing Id for a TutorialPopup instance! (Click on log to select popup)", base.gameObject);
		}
	}

	private void DisableContents()
	{
		foreach (TextMeshProUGUI text in textList)
		{
			text.gameObject.SetActive(value: false);
		}
		foreach (ImagesRow imagesRow in imagesRows)
		{
			imagesRow.images.ForEach(delegate(Image image)
			{
				image.gameObject.SetActive(value: false);
			});
			imagesRow.container.SetActive(value: false);
		}
	}

	private void DisplayImages()
	{
		if (!InputManager.IsLastControllerJoystick)
		{
			return;
		}
		for (int i = 0; i < imagesRows.Count; i++)
		{
			if (!TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table.ContainsKey(Id))
			{
				break;
			}
			List<Sprite> sprites = TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table[Id].GetSprites(i);
			if (sprites == null)
			{
				break;
			}
			foreach (Sprite item in sprites)
			{
				Image image = imagesRows[i].images.First((Image image2) => !image2.gameObject.activeSelf);
				image.gameObject.SetActive(value: true);
				image.sprite = item;
				image.SetNativeSize();
			}
			imagesRows[i].container.SetActive(value: true);
		}
	}

	private void DisplayText(string textString)
	{
		TextMeshProUGUI textMeshProUGUI = textList.First((TextMeshProUGUI text) => !text.gameObject.activeSelf);
		textMeshProUGUI.gameObject.SetActive(value: true);
		textMeshProUGUI.text = textString;
	}

	private void Refresh()
	{
		fontList.ForEach(delegate(LocalizedFont font)
		{
			font.RefreshFont();
		});
		RefreshLocalizedTexts();
		AutoResize();
	}

	private void RefreshLocalizedTexts()
	{
		List<string> list = tutorial.LocalizeTexts();
		if (list.Count <= 0)
		{
			TPSingleton<TutorialManager>.Instance.LogError("Localization for TutorialPopup with id : " + Id + " is not found", CLogLevel.MAJOR);
		}
		else
		{
			foreach (string item in list)
			{
				DisplayText(item);
			}
			DisplayImages();
		}
		TextMeshProUGUI textMeshProUGUI = confirmButtonText;
		textMeshProUGUI.text = Localizer.Get(confirmButtonType switch
		{
			ConfirmButtonType.Okay => "TutorialPopupConfirmation_OK", 
			ConfirmButtonType.Next => "TutorialPopupConfirmation_Next", 
			_ => "TutorialPopupConfirmation_OK", 
		});
	}

	private void Update()
	{
		if (InputManager.GetButtonDown(111))
		{
			Close();
		}
	}
}
