using TMPro;
using TPLib.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericTitledTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI description;

	private Color baseColor;

	private string descriptionLocaKey;

	private string descriptionLocalized;

	private Sprite iconSprite;

	private Color titleColor;

	private string titleLocaKey;

	private string titleLocalized;

	public void SetTitleColor(Color color)
	{
		titleColor = color;
	}

	public void SetContent(string titleLocaKey, string descriptionLocaKey, Sprite iconSprite = null)
	{
		titleLocalized = string.Empty;
		this.titleLocaKey = titleLocaKey;
		this.descriptionLocaKey = descriptionLocaKey;
		this.iconSprite = iconSprite;
		titleColor = baseColor;
	}

	public void SetContentLocalized(string title, string description, Sprite iconSprite = null)
	{
		titleLocalized = title;
		descriptionLocalized = description;
		titleLocaKey = string.Empty;
		descriptionLocaKey = string.Empty;
		this.iconSprite = iconSprite;
		titleColor = baseColor;
	}

	protected override void Awake()
	{
		base.Awake();
		baseColor = title.color;
	}

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		icon.gameObject.SetActive(iconSprite != null);
		if (iconSprite != null)
		{
			icon.sprite = iconSprite;
			icon.rectTransform.sizeDelta = iconSprite.rect.size;
		}
		RefreshLocalizedText();
		title.color = titleColor;
	}

	private void RefreshLocalizedText()
	{
		title.text = (string.IsNullOrEmpty(titleLocaKey) ? titleLocalized : Localizer.Get(titleLocaKey));
		description.text = (string.IsNullOrEmpty(descriptionLocaKey) ? descriptionLocalized : Localizer.Get(descriptionLocaKey));
	}
}
