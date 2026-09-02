using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class SettingsPageView : TabbedPageView
{
	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	private RectTransform viewport;

	[SerializeField]
	private SelectionEvents[] selectables;

	protected override void Start()
	{
		base.Start();
		SelectionEvents[] array = selectables;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].AddSelectListener(AdjustScroll);
		}
	}

	private void AdjustScroll(RectTransform rectTransform)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(rectTransform, viewport, scrollbar, 0.01f, 0.01f);
	}

	private void OnDestroy()
	{
		SelectionEvents[] array = selectables;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RemoveSelectListener(AdjustScroll);
		}
	}
}
