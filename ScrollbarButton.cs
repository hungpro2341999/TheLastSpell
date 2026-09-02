using UnityEngine;
using UnityEngine.UI;

public class ScrollbarButton : MonoBehaviour
{
	private enum E_ChangeScrollbarValue
	{
		Increase,
		Decrease
	}

	[SerializeField]
	private E_ChangeScrollbarValue changeScrollbarValue;

	[SerializeField]
	private Scrollbar itemsPanelScrollbar;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	public void ChangeValue()
	{
		itemsPanelScrollbar.value += ((changeScrollbarValue == E_ChangeScrollbarValue.Increase) ? scrollButtonsSensitivity : (0f - scrollButtonsSensitivity));
	}
}
