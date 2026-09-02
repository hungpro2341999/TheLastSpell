using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Enemy.Affix;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class EnemyUnitTooltip : UnitTooltip
{
	public static class Constants
	{
		public const string InfosPanelSpritesPathPrefix = "View/Sprites/UI/InfosPanel/InfosPanel_Box_01_";

		public const string InfosPanelEliteSpriteSuffix = "Elite";

		public const string InfosPanelEnemySpriteSuffix = "Enemies";

		public const string DamageTextFormat = "{0}-{1}";

		public const string StatFromAttackTypeFormat = "{0}Damage";
	}

	[SerializeField]
	private UnitStatDisplay accuracyDisplay;

	[SerializeField]
	private Image damageTypeIcon;

	[SerializeField]
	private TextMeshProUGUI damageText;

	[SerializeField]
	private GameObject defaultDisplayGO;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private Image portraitPanelBackground;

	[SerializeField]
	private Image healthGauge;

	[SerializeField]
	private Sprite healthGaugeInvincible;

	[SerializeField]
	private Sprite healthGaugeBase;

	[SerializeField]
	private EnemyAffixDisplay enemyAffixDisplayPrefab;

	[SerializeField]
	private RectTransform stunResistanceParent;

	[SerializeField]
	private TextMeshProUGUI stunResistanceText;

	[SerializeField]
	private RectTransform enemyAffixesParent;

	[SerializeField]
	private RectTransform backgroundImage;

	[SerializeField]
	private float backgroundHeightDeltaByAffix = 55f;

	[SerializeField]
	private DataColorDictionary attackTypeColors;

	[SerializeField]
	private DataColor enemyAffixNameColor;

	private readonly List<EnemyAffixDisplay> enemyAffixDisplays = new List<EnemyAffixDisplay>();

	public EnemyUnit EnemyUnit => base.Unit as EnemyUnit;

	public void SetContent(EnemyUnit enemyUnit)
	{
		base.Unit = enemyUnit;
	}

	protected override bool CanBeDisplayed()
	{
		if (EnemyUnit != null)
		{
			Tile originTile = EnemyUnit.OriginTile;
			if (originTile == null || !originTile.HasFog || EnemyUnit is BossUnit)
			{
				return TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.FinalBossDeath;
			}
		}
		return false;
	}

	protected virtual EnemyAffixEffectDefinition.E_EnemyAffixBoxType GetAffixBoxType(EnemyAffixDefinition affixDefinition, int index)
	{
		return affixDefinition.BoxType;
	}

	protected override void RefreshContent()
	{
		RefreshAffixes();
		base.RefreshContent();
		if (healthGauge != null)
		{
			healthGauge.sprite = (EnemyUnit.EnemyUnitTemplateDefinition.IsInvulnerable ? healthGaugeInvincible : healthGaugeBase);
		}
		if (portrait != null)
		{
			portrait.sprite = EnemyUnit.UiSprite;
		}
		if (accuracyDisplay != null)
		{
			accuracyDisplay.gameObject.SetActive(value: true);
			accuracyDisplay.TargetUnit = EnemyUnit;
			accuracyDisplay.Refresh();
		}
		if (damageTypeIcon != null && damageText != null)
		{
			bool flag = false;
			if (EnemyUnit.EnemyUnitTemplateDefinition.DamageSkillId != null)
			{
				Goal[] goals = EnemyUnit.Goals;
				foreach (Goal goal in goals)
				{
					if (goal.Skill.SkillDefinition.GroupId == EnemyUnit.EnemyUnitTemplateDefinition.DamageSkillId && goal.Skill.SkillAction.SkillActionController is AttackSkillActionController attackSkillActionController)
					{
						flag = true;
						defaultDisplayGO.SetActive(value: false);
						string text = $"{(attackSkillActionController.SkillAction as AttackSkillAction).AttackType.ToString()}Damage";
						if (!Enum.TryParse<UnitStatDefinition.E_Stat>(text, out var result))
						{
							TPSingleton<EnemyUnitManager>.Instance.Log("Could not parse attackType into Stat enum !", CLogLevel.MAJOR);
						}
						damageTypeIcon.enabled = true;
						damageTypeIcon.sprite = UnitStatDisplay.GetStatIconSprite(result, UnitStatDisplay.E_IconSize.VerySmall);
						Color? colorById = attackTypeColors.GetColorById(text);
						damageText.color = (colorById.HasValue ? colorById.Value : Color.white);
						Vector2 vector = attackSkillActionController.ComputeCasterDamageRange(EnemyUnit);
						damageText.text = $"{vector.x}-{vector.y}";
					}
				}
			}
			if (!flag)
			{
				damageTypeIcon.enabled = false;
				damageText.text = string.Empty;
				accuracyDisplay.gameObject.SetActive(value: false);
				defaultDisplayGO.SetActive(value: true);
			}
		}
		if (EnemyUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.StunResistance).FinalClamped > 0f)
		{
			portraitPanelBackground.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/InfosPanel/InfosPanel_Box_01_Elite");
			portraitPanelBackground.SetNativeSize();
			stunResistanceParent.gameObject.SetActive(value: true);
			stunResistanceText.text = $"{UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.StunResistance].ShortName} ({EnemyUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.StunResistance).FinalClamped}%)";
		}
		else
		{
			portraitPanelBackground.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/InfosPanel/InfosPanel_Box_01_Enemies");
			portraitPanelBackground.SetNativeSize();
			stunResistanceParent.gameObject.SetActive(value: false);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
		RefreshBackgroundSize();
	}

	private void RefreshAffixes()
	{
		int num = 0;
		foreach (EnemyAffix affix in EnemyUnit.Affixes)
		{
			EnemyAffixEffectDefinition.E_EnemyAffixEffect enemyAffixEffect = affix.EnemyAffixDefinition.EnemyAffixEffectDefinition.EnemyAffixEffect;
			string text = "<color=#" + enemyAffixNameColor._HexCode + ">" + affix.EnemyAffixDefinition.GetTitle() + "</color>";
			string[] array = affix.EnemyAffixDefinition.GetArguments(affix.Interpreter).ToList().Cast<string>()
				.ToArray();
			switch (enemyAffixEffect)
			{
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Reinforced:
				foreach (KeyValuePair<UnitStatDefinition.E_Stat, Node> item in (affix as EnemyReinforcedAffix).EnemyReinforcedAffixEffectDefinition.ModifiedStatsEveryXDay)
				{
					_ = item;
					AdjustEliteAffixesPoolLength(num);
					enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), array[0]);
					num++;
				}
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Aura:
				foreach (KeyValuePair<UnitStatDefinition.E_Stat, Node> statModifier in (affix as EnemyAuraAffix).EnemyAuraAffixEffectDefinition.StatModifiers)
				{
					_ = statModifier;
					AdjustEliteAffixesPoolLength(num);
					enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text + " " + array[2], array[1]);
					num++;
				}
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Regenerative:
				AdjustEliteAffixesPoolLength(num);
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text, array[1]);
				num++;
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Mirror:
				AdjustEliteAffixesPoolLength(num);
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text, array[0]);
				num++;
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Misty:
				AdjustEliteAffixesPoolLength(num);
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text, array[0]);
				num++;
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Energetic:
				AdjustEliteAffixesPoolLength(num);
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text);
				num++;
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Revenge:
				AdjustEliteAffixesPoolLength(num);
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text + " " + array[0]);
				num++;
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Purge:
			{
				AdjustEliteAffixesPoolLength(num);
				EnemyPurgeAffix enemyPurgeAffix = affix as EnemyPurgeAffix;
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text + " <sprite name=" + ImmuneToNegativeStatusEffectDefinition.GetImmuneStatusIconName(enemyPurgeAffix.EnemyPurgeAffixEffectDefinition.StatusType) + ">");
				num++;
				break;
			}
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.Barrier:
			{
				AdjustEliteAffixesPoolLength(num);
				EnemyBarrierAffix enemyBarrierAffix = affix as EnemyBarrierAffix;
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text + " <sprite name=" + ImmuneToNegativeStatusEffectDefinition.GetImmuneStatusIconName(enemyBarrierAffix.EnemyBarrierAffixEffectDefinition.StatusType) + ">");
				num++;
				break;
			}
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.HigherPlane:
				AdjustEliteAffixesPoolLength(num);
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text);
				num++;
				break;
			case EnemyAffixEffectDefinition.E_EnemyAffixEffect.HealthChunks:
				AdjustEliteAffixesPoolLength(num);
				enemyAffixDisplays[num].Refresh(enemyAffixEffect, GetAffixBoxType(affix.EnemyAffixDefinition, num), text);
				num++;
				break;
			}
		}
		for (int i = 0; i < enemyAffixDisplays.Count; i++)
		{
			enemyAffixDisplays[i].Display(i < num);
		}
	}

	private void AdjustEliteAffixesPoolLength(int affixIndex)
	{
		if (affixIndex > enemyAffixDisplays.Count - 1)
		{
			enemyAffixDisplays.Add(UnityEngine.Object.Instantiate(enemyAffixDisplayPrefab, enemyAffixesParent));
		}
	}
}
