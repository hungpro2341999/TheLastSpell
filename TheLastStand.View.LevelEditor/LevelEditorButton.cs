using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class LevelEditorButton : MonoBehaviour
{
	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private GameObject highlight;

	public BetterButton Button => button;

	public Toggle Toggle => toggle;

	public void Init(string text, UnityAction onClickCallback)
	{
		InitText(text);
		InitOnClick(onClickCallback);
	}

	public void Init(string text, UnityAction onClickCallback, UnityAction<bool> onToggleValueChangedCallback)
	{
		Init(text, onClickCallback);
		InitToggleValueChanged(onToggleValueChangedCallback);
	}

	public void InitText(string text)
	{
		button.ChangeText(text);
	}

	public void AppendText(string text)
	{
		button.ChangeText(button.GetText() + text);
	}

	public void InitOnClick(UnityAction onClickCallback)
	{
		Button.onClick.AddListener(onClickCallback);
		Button.onClick.AddListener(OnButtonClick);
	}

	public void InitToggleValueChanged(UnityAction<bool> onToggleValueChangedCallback)
	{
		Toggle.onValueChanged.AddListener(onToggleValueChangedCallback);
	}

	public void ToggleOff()
	{
		if (Toggle != null && Toggle.isOn)
		{
			Toggle.isOn = false;
			Toggle.interactable = true;
			highlight.SetActive(value: false);
		}
	}

	private void OnButtonClick()
	{
		if (Toggle != null)
		{
			Toggle.isOn = true;
			Toggle.interactable = false;
			highlight.SetActive(value: true);
		}
	}

	private void OnDisable()
	{
		ToggleOff();
	}
}
