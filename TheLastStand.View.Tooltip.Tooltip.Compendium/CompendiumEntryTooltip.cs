using TMPro;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip.Tooltip.Compendium;

public abstract class CompendiumEntryTooltip : TooltipBase
{
	[SerializeField]
	protected RectTransform layoutRectTransform;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected TextMeshProUGUI title;

	[SerializeField]
	protected Image titleBG;

	[SerializeField]
	protected TextMeshProUGUI description;

	public RectTransform TooltipPanel => tooltipPanel;

	protected override void RefreshLayout(bool showInstantly = false)
	{
		titleBG.rectTransform.sizeDelta = new Vector2(Mathf.Min(rectTransform.sizeDelta.x - title.rectTransform.anchoredPosition.x, Mathf.RoundToInt(title.rectTransform.anchoredPosition.x + title.rectTransform.sizeDelta.x + 20f)), titleBG.rectTransform.sizeDelta.y);
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, layoutRectTransform.sizeDelta.y);
		base.RefreshLayout(showInstantly);
	}
}
