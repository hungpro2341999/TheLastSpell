using System.Collections;
using TMPro;
using TPLib.Localization;
using TheLastStand.Framework.UI;
using UnityEngine;

namespace TheLastStand.View;

public class BlockingPopupLine : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI coreText;

	[SerializeField]
	private BetterButton mainButton;

	public BetterButton MainButton => mainButton;

	public void UpdateDisplayedText(string locKey, params object[] obj)
	{
		coreText.text = Localizer.Get(locKey);
		for (int i = 0; i < obj.Length; i++)
		{
			if (obj[i] is string text)
			{
				TextMeshProUGUI textMeshProUGUI = coreText;
				textMeshProUGUI.text = textMeshProUGUI.text + ((i == 0) ? " " : ", ") + text;
			}
		}
		StartCoroutine(UpdateSize());
	}

	private IEnumerator UpdateSize()
	{
		yield return null;
		coreText.rectTransform.sizeDelta = new Vector2(coreText.preferredWidth, coreText.rectTransform.sizeDelta.y);
	}
}
