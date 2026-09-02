using TMPro;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.View.Generic;
using TheLastStand.View.Unit.Injury;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class UnitTooltip : TooltipBase
{
	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private UnitStatDisplay movePointsDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceDisplay;

	[SerializeField]
	private UnitStatDisplay dodgeDisplay;

	[SerializeField]
	private UnitStatDisplay blockDisplay;

	[SerializeField]
	private UnitStatDisplay healthDisplay;

	[SerializeField]
	private UnitStatDisplay armorDisplay;

	[SerializeField]
	private Image armorShreddingImage;

	[SerializeField]
	private UnitStatusDisplay statusPrefab;

	[SerializeField]
	private UnitInjuriesDisplay injuriesDisplay;

	[SerializeField]
	private UnitInjuryEffectDisplay injuryEffectPrefab;

	[SerializeField]
	private UnitInjuryStatusEffectDisplay injuryStatusEffectPrefab;

	[SerializeField]
	private UnitInjuryEffectPanicDisplay injuryEffectPanicPrefab;

	[SerializeField]
	protected RectTransform portraitParent;

	[SerializeField]
	protected LayoutElement mainStatsLayoutElement;

	[SerializeField]
	protected RectTransform statusParent;

	[SerializeField]
	protected float bgDeltaHeight = 12f;

	[SerializeField]
	private float mainStatsParentMinHeight = 65f;

	[SerializeField]
	private float mainStatsParentMaxHeight = 83f;

	[SerializeField]
	private float healthAndInjuryBoxMinPos = -25f;

	[SerializeField]
	private float healthAndInjuryBoxMaxPos = -8f;

	[SerializeField]
	private RectTransform healthAndInjuryBox;

	public TheLastStand.Model.Unit.Unit Unit { get; set; }

	protected override bool CanBeDisplayed()
	{
		return Unit != null;
	}

	protected override void OnDisplay()
	{
		if (SkillManager.AttackInfoPanel.Displayed)
		{
			SkillManager.AttackInfoPanel.Refresh();
		}
		if (SkillManager.GenericActionInfoPanel.Displayed)
		{
			SkillManager.GenericActionInfoPanel.Refresh();
		}
	}

	protected override void OnHide()
	{
		if (SkillManager.AttackInfoPanel.Displayed)
		{
			SkillManager.AttackInfoPanel.Refresh();
		}
		if (SkillManager.GenericActionInfoPanel.Displayed)
		{
			SkillManager.GenericActionInfoPanel.Refresh();
		}
	}

	protected override void RefreshContent()
	{
		if (titleText != null)
		{
			titleText.text = Unit.Name;
		}
		AttackSkillAction attackSkillAction = PlayableUnitManager.SelectedSkill?.SkillAction as AttackSkillAction;
		if (movePointsDisplay != null)
		{
			movePointsDisplay.TargetUnit = Unit;
			movePointsDisplay.Refresh();
		}
		if (resistanceDisplay != null)
		{
			resistanceDisplay.TargetUnit = Unit;
			resistanceDisplay.UseSelectedSkill = false;
			resistanceDisplay.Refresh();
		}
		if (dodgeDisplay != null)
		{
			dodgeDisplay.TargetUnit = Unit;
			dodgeDisplay.UseSelectedSkill = false;
			dodgeDisplay.Refresh();
		}
		if (blockDisplay != null)
		{
			blockDisplay.TargetUnit = Unit;
			blockDisplay.UseSelectedSkill = false;
			blockDisplay.Refresh();
		}
		if (healthDisplay != null)
		{
			healthDisplay.TargetUnit = Unit;
			healthDisplay.Refresh();
		}
		if (armorDisplay != null && MustDisplayArmorGauge(Unit))
		{
			armorDisplay.gameObject.SetActive(value: true);
			armorDisplay.TargetUnit = Unit;
			armorDisplay.IsDisabled = attackSkillAction?.HasEffect("ArmorPiercing") ?? false;
			armorDisplay.Refresh();
			armorShreddingImage.enabled = attackSkillAction?.HasEffect("ArmorShredding") ?? false;
			mainStatsLayoutElement.minHeight = mainStatsParentMaxHeight;
			healthAndInjuryBox.anchoredPosition = new Vector2(healthAndInjuryBox.anchoredPosition.x, healthAndInjuryBoxMinPos);
		}
		else
		{
			armorDisplay.gameObject.SetActive(value: false);
			mainStatsLayoutElement.minHeight = mainStatsParentMinHeight;
			healthAndInjuryBox.anchoredPosition = new Vector2(healthAndInjuryBox.anchoredPosition.x, healthAndInjuryBoxMaxPos);
		}
		injuriesDisplay?.Refresh(Unit);
		statusParent?.DestroyChildren();
		statusParent.gameObject.SetActive(ShouldDisplayStatusBox());
		if (Unit.UnitStatsController is EnemyUnitStatsController enemyUnitStatsController && enemyUnitStatsController.EnemyUnitStats.Stats[UnitStatDefinition.E_Stat.Panic].Injuries != 0f)
		{
			Object.Instantiate(injuryEffectPanicPrefab, statusParent).Init(enemyUnitStatsController.EnemyUnitStats.Stats[UnitStatDefinition.E_Stat.Panic].Injuries, Unit.UnitStatsController.UnitStats.InjuryStage);
		}
		foreach (UnitStatDefinition.E_Stat statsKey in Unit.UnitStatsController.UnitStats.StatsKeys)
		{
			if (statsKey != UnitStatDefinition.E_Stat.Panic)
			{
				UnitStat stat = Unit.UnitStatsController.GetStat(statsKey);
				if (stat.Injuries != 0f)
				{
					Object.Instantiate(injuryEffectPrefab, statusParent).Init(statsKey, stat.Injuries, Unit.UnitStatsController.UnitStats.InjuryStage);
				}
				if (stat.InjuriesValueMultiplier != InjuryDefinition.E_ValueMultiplier.None)
				{
					float injuryMultiplierLoss = stat.InjuryMultiplierLoss;
					Object.Instantiate(injuryEffectPrefab, statusParent).Init(statsKey, injuryMultiplierLoss, Unit.UnitStatsController.UnitStats.InjuryStage);
				}
			}
		}
		for (int i = 0; i < Unit.StatusList.Count; i++)
		{
			if (Unit.StatusList[i].IsFromInjury)
			{
				Object.Instantiate(injuryStatusEffectPrefab, statusParent).Init(Unit.StatusList[i], Unit.UnitStatsController.UnitStats.InjuryStage);
			}
		}
		foreach (string preventedSkillsLocalizationId in Unit.GetPreventedSkillsLocalizationIds())
		{
			Object.Instantiate(injuryEffectPrefab, statusParent).Init(preventedSkillsLocalizationId, Unit.UnitStatsController.UnitStats.InjuryStage);
		}
		for (int j = 0; j < Unit.StatusList.Count; j++)
		{
			if (!Unit.StatusList[j].IsFromInjury)
			{
				Object.Instantiate(statusPrefab, statusParent).Init(Unit.StatusList[j]);
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(statusParent);
		RefreshBackgroundSize();
	}

	protected virtual void RefreshBackgroundSize()
	{
		float y = Mathf.Abs(portraitParent.localPosition.y) + portraitParent.sizeDelta.y + mainStatsLayoutElement.minHeight + ((Unit.StatusList != null && statusParent.childCount > 0) ? statusParent.sizeDelta.y : 0f) + bgDeltaHeight;
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, y);
		LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
		if (SkillManager.AttackInfoPanel.Displayed)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.parent as RectTransform);
		}
		if (SkillManager.GenericActionInfoPanel.Displayed)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.parent as RectTransform);
		}
	}

	protected virtual bool ShouldDisplayStatusBox()
	{
		if (Unit.StatusList.Count <= 0)
		{
			return Unit.UnitStatsController.UnitStats.InjuryStage > 0;
		}
		return true;
	}

	protected override void Awake()
	{
		base.Awake();
		healthDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		healthDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		movePointsDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
		dodgeDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Dodge];
		resistanceDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Resistance];
		blockDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Block];
		armorDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Armor];
		armorDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ArmorTotal];
	}

	private bool MustDisplayArmorGauge(TheLastStand.Model.Unit.Unit unit)
	{
		if (armorDisplay.SecondaryStatDefinition == null)
		{
			return false;
		}
		if (unit != null)
		{
			return unit.GetClampedStatValue(armorDisplay.SecondaryStatDefinition.Id) != 0f;
		}
		return false;
	}
}
