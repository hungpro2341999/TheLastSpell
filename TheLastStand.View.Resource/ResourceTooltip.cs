using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Resource;

public class ResourceTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI resourceTitle;

	[SerializeField]
	private Image resourceIcon;

	[SerializeField]
	private DataSpriteDictionary resourceIcons;

	[SerializeField]
	private DataColorDictionary resourceColors;

	[SerializeField]
	private TextMeshProUGUI resourceDescription;

	private string resourceId = string.Empty;

	public void SetContent(string newResourceId)
	{
		resourceId = newResourceId;
	}

	protected override bool CanBeDisplayed()
	{
		return true;
	}

	protected override void RefreshContent()
	{
		resourceIcon.sprite = resourceIcons.GetSpriteById(resourceId);
		Color? colorById = resourceColors.GetColorById(resourceId);
		if (colorById.HasValue)
		{
			resourceTitle.color = colorById.Value;
		}
		RefreshLocalizedText();
	}

	private void RefreshLocalizedText()
	{
		resourceTitle.text = Localizer.Get("Resources_Name_" + resourceId);
		resourceDescription.text = Localizer.Get("Resources_Description_" + resourceId);
	}
}
