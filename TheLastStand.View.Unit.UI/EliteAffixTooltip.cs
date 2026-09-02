using TMPro;
using TheLastStand.Framework;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class EliteAffixTooltip : TooltipBase
{
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image titleBG;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private GameObject descriptionSeparator;

	[SerializeField]
	private TextMeshProUGUI additionalDescriptionText;

	public EnemyAffix Affix { get; set; }

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		titleText.text = Affix.EnemyAffixDefinition.GetTitle();
		descriptionText.text = Affix.EnemyAffixDefinition.GetDescription(Affix.Interpreter);
		iconImage.sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Units/EnemiesAffixes/Icons/EnemyAffix_Icon_{Affix.EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect.ToString()}");
		string additionalDescription = Affix.EnemyAffixDefinition.GetAdditionalDescription(Affix.Interpreter);
		bool flag = additionalDescription != null;
		descriptionSeparator.SetActive(flag);
		additionalDescriptionText.gameObject.SetActive(flag);
		if (flag)
		{
			additionalDescriptionText.text = additionalDescription;
		}
	}

	protected override void RefreshLayout(bool showInstantly = false)
	{
		titleBG.rectTransform.sizeDelta = new Vector2(Mathf.Min(base.RectTransform.sizeDelta.x - titleText.rectTransform.anchoredPosition.x, Mathf.RoundToInt(titleText.rectTransform.anchoredPosition.x + titleText.rectTransform.sizeDelta.x + 20f)), titleBG.rectTransform.sizeDelta.y);
		base.RefreshLayout(showInstantly);
	}
}
