using TMPro;
using UnityEngine;

namespace TheLastStand.View.Generic;

public class RawTextTooltip : TooltipBase
{
	[SerializeField]
	protected TextMeshProUGUI tooltipText;

	protected string currentText = string.Empty;

	public void SetContent(string text)
	{
		currentText = text;
	}

	protected override bool CanBeDisplayed()
	{
		return currentText != string.Empty;
	}

	protected override void RefreshContent()
	{
		tooltipText.text = currentText;
	}
}
