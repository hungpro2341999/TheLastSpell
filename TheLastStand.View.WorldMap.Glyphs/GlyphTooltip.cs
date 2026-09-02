using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Manager.Meta;
using TheLastStand.View.Generic;
using TheLastStand.View.Unit.Perk;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphTooltip : TooltipBase
{
	[SerializeField]
	private PerkTooltip perkTooltip;

	[SerializeField]
	private Image glyphIcon;

	[SerializeField]
	private Image glyphCustomModeIcon;

	[SerializeField]
	private TextMeshProUGUI glyphTitle;

	[SerializeField]
	private TextMeshProUGUI glyphDescription;

	[SerializeField]
	private GameObject glyphCostIcon;

	[SerializeField]
	private RectTransform glyphCostIconsContainer;

	private bool displayTowardsRight;

	private GlyphDefinition glyphDefinition;

	private List<GameObject> glyphIcons = new List<GameObject>();

	protected override void Awake()
	{
		base.Awake();
		TPSingleton<GlyphManager>.Instance.GlyphTooltip = this;
	}

	public void Init(GlyphDefinition newGlyphDefinition, bool newDisplayTowardsRight = true)
	{
		displayTowardsRight = newDisplayTowardsRight;
		glyphDefinition = newGlyphDefinition;
	}

	protected override bool CanBeDisplayed()
	{
		return glyphDefinition != null;
	}

	protected override void OnHide()
	{
		perkTooltip.Hide();
	}

	protected override void RefreshContent()
	{
		glyphTitle.text = glyphDefinition.GetName();
		glyphDescription.text = glyphDefinition.GetDescription(null);
		glyphIcon.sprite = AGlyphDisplay.GetGlyphIcon(glyphDefinition);
		glyphCustomModeIcon.enabled = glyphDefinition.IsCustom;
		while (glyphIcons.Count > glyphDefinition.Cost)
		{
			Object.Destroy(glyphIcons[0]);
			glyphIcons.RemoveAt(0);
		}
		while (glyphIcons.Count < glyphDefinition.Cost)
		{
			glyphIcons.Add(Object.Instantiate(glyphCostIcon, glyphCostIconsContainer));
		}
		if (glyphDefinition.PerkToShow != null)
		{
			perkTooltip.SetContent(null, glyphDefinition.PerkToShow);
			perkTooltip.Display();
		}
		else
		{
			perkTooltip.Hide();
		}
		RefreshAnchors();
	}

	private void RefreshAnchors()
	{
		Vector2 vector;
		if (displayTowardsRight)
		{
			perkTooltip.transform.SetAsLastSibling();
			vector = Vector2.up;
		}
		else
		{
			perkTooltip.transform.SetAsFirstSibling();
			vector = Vector2.one;
		}
		rectTransform.anchorMin = vector;
		rectTransform.anchorMax = vector;
		rectTransform.pivot = vector;
		perkTooltip.UpdateAnchors(displayTowardsRight);
	}
}
