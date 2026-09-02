using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Manager;
using TheLastStand.Model.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class TutorialTextBox : MonoBehaviour
{
	[Serializable]
	private struct ImagesRow
	{
		public List<Image> images;
	}

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private List<ImagesRow> imagesRows;

	private List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();

	private List<LocalizedFont> fontList = new List<LocalizedFont>();

	public void Refresh(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		HideContents();
		List<string> list = tutorial.LocalizeTexts();
		if (list.Count == 0)
		{
			TPSingleton<TutorialManager>.Instance.LogError("Localization for TutorialPopup with id : " + tutorial.TutorialDefinition.Id + " is not found", CLogLevel.MAJOR);
		}
		else
		{
			foreach (string item in list)
			{
				DisplayText(item);
			}
		}
		DisplayImages(tutorial);
	}

	public void Show()
	{
		Display(show: true);
	}

	public void Hide()
	{
		Display(show: false);
	}

	private void Awake()
	{
		fontList.AddRange(base.transform.GetComponentsInChildren<LocalizedFont>(includeInactive: true));
		textList.AddRange(contentTransform.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true));
	}

	private void Display(bool show)
	{
		base.gameObject.SetActive(show);
	}

	private void DisplayImages(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		if (!InputManager.IsLastControllerJoystick)
		{
			return;
		}
		for (int i = 0; i < imagesRows.Count; i++)
		{
			if (!TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table.ContainsKey(tutorial.TutorialDefinition.Id))
			{
				break;
			}
			List<Sprite> sprites = TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table[tutorial.TutorialDefinition.Id].GetSprites(i);
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
		}
	}

	private void DisplayText(string textString)
	{
		TextMeshProUGUI textMeshProUGUI = textList.First((TextMeshProUGUI text) => !text.gameObject.activeSelf);
		textMeshProUGUI.gameObject.SetActive(value: true);
		textMeshProUGUI.text = textString;
	}

	private void HideContents()
	{
		textList.ForEach(delegate(TextMeshProUGUI text)
		{
			text.gameObject.SetActive(value: false);
		});
		imagesRows.ForEach(delegate(ImagesRow row)
		{
			row.images.ForEach(delegate(Image image)
			{
				image.gameObject.SetActive(value: false);
			});
		});
	}
}
