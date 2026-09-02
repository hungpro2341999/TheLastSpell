using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Settings.KeyRemapping;

public class KeyRemappingBindingView : MonoBehaviour
{
	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private TextMeshProUGUI bindingText;

	[SerializeField]
	private TextMeshProUGUI remappingFeedback;

	[SerializeField]
	private GameObject selector;

	[SerializeField]
	private DataColor hoverColor;

	[SerializeField]
	private TextMeshProUGUI debugKeyCodeText;

	[SerializeField]
	private LocalizedFont localizedFont;

	private Color initColor;

	private KeyCode keyCode;

	public BetterButton Button => button;

	public void DisplayRemappingFeedback(bool show)
	{
		remappingFeedback.gameObject.SetActive(show);
		if (show)
		{
			SetText(string.Empty);
			remappingFeedback.text = Localizer.Get("KeyRemapping_PressAnyKeyToBind");
		}
	}

	public void Highlight(bool state)
	{
		if (!TPSingleton<KeyRemappingManager>.Instance.RemappingInProgress)
		{
			selector.SetActive(state);
			bindingText.color = (state ? hoverColor._Color : initColor);
		}
	}

	public void RefreshLocalizedKeyCode()
	{
		if (Localizer.TryGet($"KeyCode_{keyCode}", out var value) && !string.IsNullOrEmpty(value))
		{
			bindingText.text = value;
		}
		localizedFont?.RefreshFont();
	}

	public void RefreshText(string defaultName)
	{
		SetText((Localizer.TryGet($"KeyCode_{keyCode}", out var value) && !string.IsNullOrEmpty(value)) ? value : defaultName);
	}

	public void SetKeyCode(KeyCode keyCode)
	{
		this.keyCode = keyCode;
		debugKeyCodeText.text = ((keyCode == KeyCode.None) ? string.Empty : keyCode.ToString());
	}

	public void SetText(string text)
	{
		bindingText.text = text;
	}

	private void Awake()
	{
		initColor = bindingText.color;
	}

	public void DebugShowRawKeyCode(bool show)
	{
		debugKeyCodeText.gameObject.SetActive(show);
	}
}
