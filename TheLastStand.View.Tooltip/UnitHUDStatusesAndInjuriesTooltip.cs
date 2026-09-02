using TMPro;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class UnitHUDStatusesAndInjuriesTooltip : TooltipBase
{
	[SerializeField]
	private Image titleBG;

	[SerializeField]
	private Sprite baseBG;

	[SerializeField]
	private Sprite injuriesBG;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI linesText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	public string TitleLocalized { get; set; }

	public string LinesLocalized { get; set; }

	public string DescriptionLocalized { get; set; }

	public bool UseInjuryBox { get; set; }

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		titleText.text = TitleLocalized;
		titleBG.sprite = (UseInjuryBox ? injuriesBG : baseBG);
		linesText.text = LinesLocalized;
		descriptionText.text = DescriptionLocalized;
	}

	protected override void RefreshLayout(bool showInstantly = false)
	{
		titleBG.rectTransform.sizeDelta = new Vector2(Mathf.Min(base.RectTransform.sizeDelta.x - titleText.rectTransform.anchoredPosition.x, Mathf.RoundToInt(titleText.rectTransform.anchoredPosition.x + titleText.rectTransform.sizeDelta.x + 20f)), titleBG.rectTransform.sizeDelta.y);
		base.RefreshLayout(showInstantly);
	}
}
