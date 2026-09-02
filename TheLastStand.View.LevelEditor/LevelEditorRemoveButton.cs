using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.LevelEditor;

public class LevelEditorRemoveButton : MonoBehaviour
{
	[SerializeField]
	private BetterButton button;

	public BetterButton Button => button;

	public void Init(string text, UnityAction onClickCallback)
	{
		InitText(text);
		InitOnClick(onClickCallback);
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
		button.onClick.RemoveAllListeners();
		Button.onClick.AddListener(onClickCallback);
	}
}
