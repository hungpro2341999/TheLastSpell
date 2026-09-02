using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.View.Tooltip.Tooltip.Compendium;
using UnityEngine;

namespace TheLastStand.View.Tooltip.Compendium;

public class AttackTypeTooltip : CompendiumEntryTooltip
{
	[SerializeField]
	private DataSpriteDictionary damageTypeIcons;

	[SerializeField]
	private DataColorDictionary damageTypeColors;

	public AttackSkillActionDefinition.E_AttackType AttackType { get; set; }

	protected override bool CanBeDisplayed()
	{
		return AttackType != AttackSkillActionDefinition.E_AttackType.None;
	}

	protected override void OnHide()
	{
		base.OnHide();
		AttackType = AttackSkillActionDefinition.E_AttackType.None;
	}

	protected override void RefreshContent()
	{
		icon.sprite = damageTypeIcons.GetSpriteById(AttackType.ToString());
		title.text = Localizer.Get(string.Format("{0}{1}", "DamageTypeName_", AttackType));
		title.color = damageTypeColors.GetColorById(AttackType.ToString()).Value;
		description.text = Localizer.Get(string.Format("{0}{1}", "DamageTypeDescription_", AttackType));
	}
}
