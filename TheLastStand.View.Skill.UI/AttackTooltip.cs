using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class AttackTooltip : SkillActionTooltip
{
	[SerializeField]
	private RectTransform skillDetailsRectTransform;

	[SerializeField]
	private RectTransform skillDetailsParent;

	[SerializeField]
	private RectTransform skillEffectsRectTransform;

	[SerializeField]
	private RectTransform skillEffectsParent;

	[SerializeField]
	private SkillParameterDisplay attackValueDisplay;

	[SerializeField]
	private SkillParameterDisplay resistanceDisplay;

	[SerializeField]
	private SkillParameterDisplay blockDisplay;

	[SerializeField]
	private SkillParameterDisplay isolatedDisplay;

	[SerializeField]
	private SkillParameterDisplay momentumDisplay;

	[SerializeField]
	private SkillParameterDisplay opportunityDisplay;

	[SerializeField]
	private SkillParameterDisplay perkDamageDisplay;

	[SerializeField]
	private SkillParameterDisplay finalValueDisplay;

	[SerializeField]
	private FormatSkillParameterDisplay criticalValueDisplay;

	[SerializeField]
	private DataColorDictionary damageTypeColor;

	[SerializeField]
	private GameObject noBlockFeedback;

	[SerializeField]
	private GameObject dodgeResistancePanel;

	[SerializeField]
	private UnitStatDisplay resistanceStatDisplay;

	[SerializeField]
	private UnitStatDisplay dodgeStatDisplay;

	public void RefreshAttackData()
	{
		RefreshStats();
		skillEffectsRectTransform.sizeDelta = new Vector2(skillEffectsRectTransform.sizeDelta.x, skillEffectsParent.sizeDelta.y);
		AttackSkillAction attackSkillAction = skillDisplay.Skill.SkillAction as AttackSkillAction;
		skillDisplay.Skill.SkillAction.SkillActionController.EnsurePerkData(base.TargetTile, base.TargetTile?.GetDamageable(isUnitPriority: true));
		DamageRangeData damageRangeData = attackSkillAction.AttackSkillActionController.ComputeFinalDamageRange(base.TargetTile, skillDisplay.SkillOwner);
		int num = 0;
		float num2 = 0f;
		TheLastStand.Model.Unit.Unit unit = skillDisplay.SkillOwner as TheLastStand.Model.Unit.Unit;
		PlayableUnit playableUnit = skillDisplay.SkillOwner as PlayableUnit;
		if (base.TargetTile != null && base.TargetUnit != null && base.TargetUnit != skillDisplay.SkillOwner)
		{
			num = Mathf.CeilToInt(base.TargetUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Block));
			if (unit != null)
			{
				float clampedStatValue = unit.GetClampedStatValue(UnitStatDefinition.E_Stat.ResistanceReduction);
				float num3 = unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).ClampStatValue(unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).FinalClamped + ((attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Magical) ? UnitDatabase.MagicDamagePercentageResistanceReduction : 0f));
				float num4 = 0f;
				float num5 = 0f;
				if (playableUnit != null)
				{
					num4 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.FlatResistanceReduction, skillDisplay.Skill.SkillAction.PerkDataContainer);
					num5 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.PercentageResistanceReduction, skillDisplay.Skill.SkillAction.PerkDataContainer);
				}
				num2 = base.TargetUnit.GetReducedResistance(clampedStatValue + num4, num3 + num5);
			}
		}
		float num6 = 0f;
		if (unit != null)
		{
			num6 += unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Critical);
			if (playableUnit != null)
			{
				num6 += playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.Critical, attackSkillAction.PerkDataContainer);
			}
		}
		bool flag = num6 > 0f;
		if (flag)
		{
			float num7 = playableUnit?.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.CriticalPower, skillDisplay.Skill.SkillAction.PerkDataContainer) ?? 0f;
			Vector2Int v = (new Vector2(damageRangeData.FinalDamageRange.x, damageRangeData.FinalDamageRange.y) * (unit.GetClampedStatValue(UnitStatDefinition.E_Stat.CriticalPower) + num7) / 100f).RoundToInt();
			criticalValueDisplay.Refresh("AttackTooltip_Critical", v.GetSimplifiedRange(), "", num6);
		}
		criticalValueDisplay.Display(flag);
		if ((float)num > 0f || damageRangeData.ResistanceReductionRange.magnitude != 0f || damageRangeData.IsolatedDamageRange.magnitude != 0f || damageRangeData.MomentumDamageRange.magnitude != 0f || damageRangeData.OpportunismDamageRange.magnitude != 0f || damageRangeData.PerksDamageRange.magnitude != 0f)
		{
			attackValueDisplay.Refresh("AttackTooltip_AttackValue", damageRangeData.BaseDamageRange.GetSimplifiedRange());
			attackValueDisplay.Display(show: true);
			bool flag2 = damageRangeData.IsolatedDamageRange.magnitude != 0f;
			if (flag2)
			{
				Vector2Int isolatedDamageRange = damageRangeData.IsolatedDamageRange;
				string overrideSign = (((float)isolatedDamageRange.x < 0f && (float)isolatedDamageRange.y < 0f) ? "-" : "+");
				isolatedDisplay.Refresh("AttackTooltip_Isolated", isolatedDamageRange.GetSimplifiedRange("-", Mathf.Abs) ?? "", overrideSign);
			}
			bool flag3 = damageRangeData.OpportunismDamageRange.magnitude != 0f;
			if (flag3)
			{
				Vector2Int opportunismDamageRange = damageRangeData.OpportunismDamageRange;
				string overrideSign2 = (((float)opportunismDamageRange.x < 0f && (float)opportunismDamageRange.y < 0f) ? "-" : "+");
				opportunityDisplay.Refresh("AttackTooltip_Opportunistic", opportunismDamageRange.GetSimplifiedRange("-", Mathf.Abs) ?? "", overrideSign2);
			}
			bool flag4 = damageRangeData.MomentumDamageRange.magnitude > 0f;
			if (flag4)
			{
				Vector2Int momentumDamageRange = damageRangeData.MomentumDamageRange;
				momentumDisplay.Refresh("AttackTooltip_Momentum", momentumDamageRange.GetSimplifiedRange() ?? "");
			}
			bool flag5 = damageRangeData.PerksDamageRange.magnitude != 0f;
			if (flag5)
			{
				Vector2Int v2 = damageRangeData.PerksDamageRange;
				string overrideSign3 = "+";
				if ((float)v2.x < 0f && (float)v2.y < 0f)
				{
					overrideSign3 = "-";
					v2 = new Vector2Int(Mathf.Abs(v2.x), Mathf.Abs(v2.y));
				}
				perkDamageDisplay.Refresh("AttackTooltip_Perks", v2.GetSimplifiedRange() ?? "", overrideSign3);
			}
			bool flag6 = damageRangeData.ResistanceReductionRange.magnitude != 0f;
			if (flag6)
			{
				Vector2Int resistanceReductionRange = damageRangeData.ResistanceReductionRange;
				resistanceDisplay.Refresh("AttackTooltip_Resistance", resistanceReductionRange.GetSimplifiedRange("-", Mathf.Abs) ?? "", (num2 < 0f) ? "+" : "-");
			}
			bool flag7 = num > 0;
			if (flag7)
			{
				if (!attackSkillAction.HasEffect("NoBlock"))
				{
					noBlockFeedback.SetActive(value: false);
					blockDisplay.Refresh("AttackTooltip_Block", num.ToString());
				}
				else
				{
					noBlockFeedback.SetActive(value: true);
					blockDisplay.Refresh("AttackTooltip_Block", "<style=\"Outline\"><style=\"Bad\">0</style></style>");
				}
			}
			blockDisplay.Display(flag7);
			isolatedDisplay.Display(flag2);
			momentumDisplay.Display(flag4);
			opportunityDisplay.Display(flag3);
			perkDamageDisplay.Display(flag5);
			resistanceDisplay.Display(flag6);
		}
		else
		{
			attackValueDisplay.Display(show: false);
			blockDisplay.Display(show: false);
			isolatedDisplay.Display(show: false);
			momentumDisplay.Display(show: false);
			opportunityDisplay.Display(show: false);
			perkDamageDisplay.Display(show: false);
			resistanceDisplay.Display(show: false);
		}
		Color? valueColor = damageTypeColor?.GetColorById(attackSkillAction.AttackType.ToString()).Value;
		finalValueDisplay.Refresh("AttackTooltip_FinalDamage", damageRangeData.FinalDamageRange.GetSimplifiedRange(), valueColor);
		LayoutRebuilder.ForceRebuildLayoutImmediate(skillDetailsParent);
		skillDetailsRectTransform.sizeDelta = new Vector2(skillDetailsRectTransform.sizeDelta.x, skillDetailsParent.sizeDelta.y);
		skillEffectsRectTransform.localPosition = new Vector3(skillEffectsRectTransform.localPosition.x, skillDetailsRectTransform.localPosition.y - skillDetailsRectTransform.sizeDelta.y, skillEffectsRectTransform.localPosition.z);
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, Mathf.Abs(skillDetailsRectTransform.localPosition.y) + skillDetailsRectTransform.sizeDelta.y + (skillEffectsRectTransform.gameObject.activeInHierarchy ? skillEffectsRectTransform.sizeDelta.y : 0f));
	}

	protected override void RefreshContent()
	{
		base.RefreshContent();
		RefreshAttackData();
	}

	private void RefreshStats()
	{
		dodgeResistancePanel.SetActive(base.TargetUnit != null);
		if (base.TargetUnit != null)
		{
			AttackSkillAction attackSkillAction = skillDisplay.Skill.SkillAction as AttackSkillAction;
			dodgeStatDisplay.TargetUnit = base.TargetUnit;
			dodgeStatDisplay.IsDisabled = attackSkillAction.HasEffect("NoDodge");
			dodgeStatDisplay.Refresh();
			resistanceStatDisplay.TargetUnit = base.TargetUnit;
			resistanceStatDisplay.Refresh();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		dodgeStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Dodge];
		resistanceStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Resistance];
	}
}
