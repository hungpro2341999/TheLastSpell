using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.View.Skill;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemCarouselEntryIconDisplay : MonoBehaviour
{
	[SerializeField]
	private RectTransform displayRectTransform;

	[SerializeField]
	private GameObject perkContainer;

	[SerializeField]
	private Image perkIcon;

	[SerializeField]
	private Vector2 perkDisplaySize;

	[SerializeField]
	private GameObject skillContainer;

	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private Vector2 skillDisplaySize;

	[SerializeField]
	private GameObject nextTextsSkillContainer;

	[SerializeField]
	private GameObject nextTextsPerkContainer;

	public PerkDefinition PerkDefinition { get; private set; }

	public SkillDefinition SkillDefinition { get; private set; }

	public void Refresh()
	{
		perkContainer.SetActive(PerkDefinition != null);
		skillContainer.SetActive(SkillDefinition != null);
		if (PerkDefinition != null && perkIcon != null)
		{
			perkIcon.sprite = PerkDefinition.PerkSprite;
			if (displayRectTransform != null)
			{
				displayRectTransform.sizeDelta = perkDisplaySize;
			}
		}
		if (SkillDefinition != null && skillIcon != null)
		{
			skillIcon.sprite = SkillView.GetIconSprite(SkillDefinition.ArtId);
			if (displayRectTransform != null)
			{
				displayRectTransform.sizeDelta = skillDisplaySize;
			}
		}
	}

	public void SetContent(PerkDefinition perkDefinition)
	{
		PerkDefinition = perkDefinition;
		SkillDefinition = null;
	}

	public void SetContent(SkillDefinition skillDefinition)
	{
		SkillDefinition = skillDefinition;
		PerkDefinition = null;
	}

	public void ToggleNextElementLabel(bool mustDisplay, bool isNextEntryASkill)
	{
		if (nextTextsSkillContainer != null && nextTextsPerkContainer != null)
		{
			nextTextsSkillContainer.SetActive(mustDisplay && isNextEntryASkill);
			nextTextsPerkContainer.SetActive(mustDisplay && !isNextEntryASkill);
		}
	}
}
