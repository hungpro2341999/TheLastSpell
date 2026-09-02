using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI.Effect;

public class SkillEffectDisplay : MonoBehaviour
{
	[SerializeField]
	private Image bgImage;

	[SerializeField]
	[Tooltip("Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgSprites;

	[SerializeField]
	[Tooltip("[Surrounding Effects] Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgSurroundingSprites;

	[SerializeField]
	[Tooltip("[Negative Effects] Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgNegativeSprites;

	[SerializeField]
	[Tooltip("[Caster Effects] Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgCasterEffectSprites;

	[SerializeField]
	private Sprite perkEffectHighlightDefaultSprite;

	[SerializeField]
	private DataSpriteTable perkEffectHighlightSurroundingSprites;

	[SerializeField]
	private Image perkEffectHighlightFeedbackImage;

	[SerializeField]
	private Image icon;

	[SerializeField]
	[Tooltip("Choice of sprite depending on the effect.")]
	private DataSpriteDictionary skillEffectIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private float paddingBetweenTitleAndIcon;

	[SerializeField]
	[Tooltip("Choice of color depending on the effect.")]
	private DataColorDictionary skillEffectColors;

	[SerializeField]
	[Tooltip("Color of the probablility if reduced by something.")]
	private DataColor negativeColor;

	[SerializeField]
	private GameObject buffStatPanel;

	[SerializeField]
	private Image buffStatIcon;

	[SerializeField]
	private GameObject turnPanel;

	[SerializeField]
	private TextMeshProUGUI turnCount;

	[SerializeField]
	private GameObject turnInfiniteIcon;

	[SerializeField]
	private GameObject damagePanel;

	[SerializeField]
	private TextMeshProUGUI damage;

	[SerializeField]
	private GameObject propagationPanel;

	[SerializeField]
	private TextMeshProUGUI propagationBounces;

	[SerializeField]
	private GameObject surroundingDamagePanel;

	[SerializeField]
	private TextMeshProUGUI surroundingDamage;

	[SerializeField]
	private GameObject inaccuracyPercentagePanel;

	[SerializeField]
	private TextMeshProUGUI inaccuracyPercentage;

	[SerializeField]
	private GameObject isolatedPanel;

	[SerializeField]
	private TextMeshProUGUI isolatedPercentage;

	[SerializeField]
	private GameObject momentumPanel;

	[SerializeField]
	private TextMeshProUGUI momentumPercentage;

	[SerializeField]
	private GameObject opportunisticPanel;

	[SerializeField]
	private TextMeshProUGUI opportunisticPercentage;

	[SerializeField]
	private GameObject immunityPanel;

	[SerializeField]
	private Image immunityStatusIcon;

	private Vector3[] titleTextWorldCorners = new Vector3[4];

	private Vector3[] iconWorldCorners = new Vector3[4];

	private bool isSurrounding;

	private float stunChanceStatModifier;

	private SkillEffectDefinition SkillEffectDefinition { get; set; }

	private ISkillCaster SkillOwner { get; set; }

	private TheLastStand.Model.Unit.Unit SkillOwnerUnit { get; set; }

	public virtual void Init(SkillEffectDefinition skillEffect, ISkillCaster skillOwner, TheLastStand.Model.Skill.SkillAction.SkillAction skillAction, bool isSurrounding, bool casterEffect, bool isFromPerk, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers)
	{
		SkillEffectDefinition = skillEffect;
		SkillOwner = skillOwner;
		SkillOwnerUnit = SkillOwner as TheLastStand.Model.Unit.Unit;
		this.isSurrounding = isSurrounding;
		stunChanceStatModifier = statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.StunChanceModifier) ?? 0f;
		bool flag = false;
		icon.sprite = skillEffectIcons.GetSpriteById(SkillEffectDefinition.Id);
		RefreshTitle();
		int num = 0;
		turnPanel.SetActive(value: false);
		damagePanel.SetActive(value: false);
		immunityPanel.SetActive(value: false);
		surroundingDamagePanel.SetActive(value: false);
		inaccuracyPercentagePanel.SetActive(value: false);
		isolatedPanel.SetActive(value: false);
		momentumPanel.SetActive(value: false);
		opportunisticPanel.SetActive(value: false);
		propagationPanel.SetActive(value: false);
		buffStatPanel.SetActive(value: false);
		SkillEffectDefinition skillEffectDefinition = SkillEffectDefinition;
		if (!(skillEffectDefinition is StatusEffectDefinition statusEffectDefinition))
		{
			if (!(skillEffectDefinition is DamageSurroundingEffectDefinition))
			{
				if (!(skillEffectDefinition is InaccurateSkillEffectDefinition inaccurateSkillEffectDefinition))
				{
					if (!(skillEffectDefinition is IsolatedSkillEffectDefinition isolatedSkillEffectDefinition))
					{
						if (!(skillEffectDefinition is KillSkillEffectDefinition))
						{
							if (!(skillEffectDefinition is MomentumEffectDefinition momentumEffectDefinition))
							{
								if (!(skillEffectDefinition is MultiHitSkillEffectDefinition multiHitSkillEffectDefinition))
								{
									if (!(skillEffectDefinition is OpportunisticSkillEffectDefinition opportunisticSkillEffectDefinition))
									{
										if (!(skillEffectDefinition is PropagationSkillEffectDefinition propagationSkillEffectDefinition))
										{
											if (!(skillEffectDefinition is ArmorShreddingEffectDefinition armorShreddingEffectDefinition))
											{
												if (!(skillEffectDefinition is RemoveStatusEffectDefinition removeStatusEffectDefinition))
												{
													if (skillEffectDefinition is ExtinguishBrazierSkillEffectDefinition extinguishBrazierSkillEffectDefinition)
													{
														num = 1;
														damagePanel.SetActive(value: true);
														damage.text = extinguishBrazierSkillEffectDefinition.BrazierDamage.ToString();
													}
												}
												else if (removeStatusEffectDefinition.Id != "Discharge")
												{
													buffStatPanel.SetActive(value: true);
													buffStatIcon.sprite = skillEffectIcons.GetSpriteById(removeStatusEffectDefinition.RemoveStatusDefinition.Status.ToString());
												}
											}
											else
											{
												num = 1;
												float num2 = armorShreddingEffectDefinition.BonusDamage * 100f;
												num2 += SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.ArmorShreddingAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.ArmorShreddingAttacks)) ?? 0f;
												momentumPanel.SetActive(value: true);
												momentumPercentage.text = $"+{num2}%";
											}
										}
										else
										{
											num = 1;
											int num3 = ((SkillOwnerUnit != null) ? SkillOwnerUnit.UnitController.GetModifiedPropagationsCount(propagationSkillEffectDefinition.GetPropagationsCount(propagationSkillEffectDefinition.GetPerkContext(SkillOwnerUnit)), statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PropagationBouncesModifier)) : propagationSkillEffectDefinition.PropagationsCount);
											if (SkillOwnerUnit != null)
											{
												damagePanel.SetActive(value: true);
												damage.text = $"{SkillOwnerUnit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.PropagationDamage, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PropagationDamage)):N0}%";
												num = 2;
											}
											propagationPanel.SetActive(value: true);
											propagationBounces.text = $"x{num3}";
										}
									}
									else
									{
										num = 3;
										float damageMultiplier = opportunisticSkillEffectDefinition.GetDamageMultiplier(opportunisticSkillEffectDefinition.GetPerkContext(SkillOwner));
										int num4 = Mathf.RoundToInt((SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.OpportunisticAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.OpportunisticAttacks)) ?? 0f) * damageMultiplier);
										opportunisticPanel.SetActive(value: true);
										opportunisticPercentage.text = $"x{damageMultiplier} ({num4}%)";
									}
								}
								else
								{
									num = 1;
									int num5 = ((SkillOwnerUnit != null) ? SkillOwnerUnit.UnitController.GetModifiedMultiHitsCount(multiHitSkillEffectDefinition.GetHitsCount(multiHitSkillEffectDefinition.GetPerkContext(SkillOwnerUnit)), statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.MultiHitsCountModifier)) : multiHitSkillEffectDefinition.HitsCount);
									damagePanel.SetActive(value: true);
									damage.text = $"x{num5}";
								}
							}
							else
							{
								num = 1;
								float num6 = momentumEffectDefinition.GetDamageBonusPerTile(momentumEffectDefinition.GetPerkContext(SkillOwnerUnit)) * 100f;
								num6 += SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.MomentumAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.MomentumAttacks)) ?? 0f;
								momentumPanel.SetActive(value: true);
								momentumPercentage.text = $"+{num6}%";
							}
						}
						else
						{
							num = 0;
						}
					}
					else
					{
						num = 3;
						float damageMultiplier2 = isolatedSkillEffectDefinition.GetDamageMultiplier(isolatedSkillEffectDefinition.GetPerkContext(SkillOwnerUnit));
						float num7 = SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.IsolatedAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.IsolatedAttacks)) ?? 0f;
						int num8 = Mathf.RoundToInt(damageMultiplier2 * num7);
						isolatedPanel.SetActive(value: true);
						isolatedPercentage.text = $"x{damageMultiplier2} ({num8}%)";
					}
				}
				else
				{
					num = 1;
					inaccuracyPercentagePanel.SetActive(value: true);
					inaccuracyPercentage.text = $"x{1f + inaccurateSkillEffectDefinition.GetMalus(inaccurateSkillEffectDefinition.GetPerkContext(SkillOwnerUnit))}";
					flag = true;
				}
			}
			else
			{
				num = 3;
				surroundingDamagePanel.SetActive(value: true);
				Vector2Int vector2Int = (skillAction.SkillActionController as AttackSkillActionController).ComputeCasterDamageRange(SkillOwner, isSurroundingTile: true, statModifiers);
				surroundingDamage.text = $"{vector2Int.x}-{vector2Int.y}";
			}
		}
		else
		{
			num = 1;
			turnPanel.SetActive(value: true);
			int turnsCount = statusEffectDefinition.GetTurnsCount(statusEffectDefinition.GetPerkContext(SkillOwner));
			if ((float)turnsCount == -1f)
			{
				turnInfiniteIcon.SetActive(value: true);
				turnCount.gameObject.SetActive(value: false);
			}
			else
			{
				turnInfiniteIcon.SetActive(value: false);
				turnCount.gameObject.SetActive(value: true);
				turnCount.text = SkillOwner?.ComputeStatusDuration(statusEffectDefinition.StatusType, turnsCount, skillAction.PerkDataContainer).ToString() ?? turnsCount.ToString();
			}
			if (SkillEffectDefinition is PoisonEffectDefinition poisonEffectDefinition)
			{
				float num9 = poisonEffectDefinition.GetDamagePerTurn(poisonEffectDefinition.GetPerkContext(SkillOwnerUnit));
				if (SkillOwnerUnit != null)
				{
					num9 = SkillOwnerUnit.UnitController.GetModifiedPoisonDamage(num9, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PoisonDamageModifier));
				}
				if (!poisonEffectDefinition.IgnoreDamageScale && skillAction.Skill.SkillContainer is TheLastStand.Model.Item.Item item)
				{
					num9 *= SkillDatabase.PoisonDamageScaleDefinition.GetMultiplierAtLevel(item.Level);
				}
				damagePanel.SetActive(value: true);
				damage.text = Mathf.CeilToInt(num9).ToString();
				num++;
			}
			else if (SkillEffectDefinition is ImmuneToNegativeStatusEffectDefinition immuneToNegativeStatusEffectDefinition)
			{
				immunityPanel.SetActive(value: true);
				immunityStatusIcon.sprite = GetStatusImmunityIcon(immuneToNegativeStatusEffectDefinition.StatusImmunity);
			}
		}
		if (this.isSurrounding)
		{
			bgImage.sprite = bgSurroundingSprites.GetSpriteAt(num);
		}
		else if (casterEffect)
		{
			bgImage.sprite = bgCasterEffectSprites.GetSpriteAt(num);
		}
		else
		{
			bgImage.sprite = (flag ? bgNegativeSprites.GetSpriteAt(num) : bgSprites.GetSpriteAt(num));
		}
		perkEffectHighlightFeedbackImage.enabled = isFromPerk;
		if (isFromPerk)
		{
			perkEffectHighlightFeedbackImage.sprite = (this.isSurrounding ? perkEffectHighlightSurroundingSprites.GetSpriteAt(num) : perkEffectHighlightDefaultSprite);
		}
		StartCoroutine(DelayedResizeTitleTextAccordingToIconPosition());
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	[ContextMenu("Resize TitleText")]
	private void ResizeTitleTextAccordingToIconPosition()
	{
		titleText.rectTransform.GetWorldCorners(titleTextWorldCorners);
		icon.rectTransform.GetWorldCorners(iconWorldCorners);
		Vector2 sizeDelta = titleText.rectTransform.sizeDelta;
		sizeDelta.x = Mathf.Abs(titleTextWorldCorners[3].x - iconWorldCorners[3].x) - paddingBetweenTitleAndIcon;
		titleText.rectTransform.sizeDelta = sizeDelta;
	}

	private IEnumerator DelayedResizeTitleTextAccordingToIconPosition()
	{
		yield return SharedYields.WaitForEndOfFrame;
		ResizeTitleTextAccordingToIconPosition();
	}

	private Sprite GetStatusImmunityIcon(Status.E_StatusType statusImmunity)
	{
		if ((statusImmunity & Status.E_StatusType.AllNegativeImmunity) == Status.E_StatusType.AllNegativeImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.AllNegative.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.StunImmunity) == Status.E_StatusType.StunImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Stun.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.PoisonImmunity) == Status.E_StatusType.PoisonImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Poison.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.DebuffImmunity) == Status.E_StatusType.DebuffImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Debuff.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.ContagionImmunity) == Status.E_StatusType.ContagionImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Contagion.ToString());
		}
		return skillEffectIcons.GetSpriteById(Status.E_StatusType.AllNegativeImmunity.ToString());
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private string ColoredName(string name)
	{
		if (SkillEffectDefinition == null)
		{
			return string.Empty;
		}
		string colorHexCodeById = skillEffectColors.GetColorHexCodeById(SkillEffectDefinition.Id);
		if (colorHexCodeById == null)
		{
			return name;
		}
		return "<color=#" + colorHexCodeById + ">" + name + "</color>";
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		float num = 1f;
		bool flag = false;
		bool flag2 = false;
		SkillEffectDefinition skillEffectDefinition = SkillEffectDefinition;
		string text;
		if (!(skillEffectDefinition is StatusEffectDefinition statusEffectDefinition))
		{
			text = ((skillEffectDefinition is IsolatedSkillEffectDefinition) ? (UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.IsolatedAttacks].Name ?? "") : ((skillEffectDefinition is OpportunisticSkillEffectDefinition) ? (UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.OpportunisticAttacks].Name ?? "") : ((skillEffectDefinition is RegenStatSkillEffectDefinition regenStatSkillEffectDefinition) ? (regenStatSkillEffectDefinition.Stat.GetValueStylized(regenStatSkillEffectDefinition.GetBonus(regenStatSkillEffectDefinition.GetPerkContext(SkillOwner))) + " " + UnitStatDisplay.GetStatIconToString(regenStatSkillEffectDefinition.Stat) + "<color=#f8d8b3>" + regenStatSkillEffectDefinition.StatName + "</color>") : ((!(skillEffectDefinition is DecreaseStatSkillEffectDefinition decreaseStatSkillEffectDefinition)) ? ColoredName(SkillManager.GetSkillEffectName(SkillEffectDefinition.Id)) : (decreaseStatSkillEffectDefinition.Stat.GetValueStylized(0f - decreaseStatSkillEffectDefinition.GetLossValue(decreaseStatSkillEffectDefinition.GetPerkContext(SkillOwner))) + " " + UnitStatDisplay.GetStatIconToString(decreaseStatSkillEffectDefinition.Stat) + "<color=#f8d8b3>" + decreaseStatSkillEffectDefinition.StatName + "</color>")))));
		}
		else
		{
			BuffEffectDefinition buffEffectDefinition = statusEffectDefinition as BuffEffectDefinition;
			DebuffEffectDefinition debuffEffectDefinition = statusEffectDefinition as DebuffEffectDefinition;
			if ((flag2 = statusEffectDefinition is StunEffectDefinition) && SkillOwnerUnit != null)
			{
				num = SkillOwnerUnit.UnitController.GetModifiedStunChance(statusEffectDefinition.GetBaseChance(statusEffectDefinition.GetPerkContext(SkillOwner)), stunChanceStatModifier);
				if (!isSurrounding)
				{
					Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
					if (tile != null && tile.Unit != null)
					{
						float num2 = num;
						num = TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Unit.UnitController.GetResistedStunChance(num);
						if (num2 != num)
						{
							flag = true;
						}
					}
				}
			}
			else
			{
				num = statusEffectDefinition.GetBaseChance(statusEffectDefinition.GetPerkContext(SkillOwner));
			}
			if (buffEffectDefinition != null || debuffEffectDefinition != null)
			{
				UnitStatDefinition.E_Stat e_Stat = UnitStatDefinition.E_Stat.Undefined;
				string text2 = string.Empty;
				if (buffEffectDefinition != null)
				{
					e_Stat = buffEffectDefinition.Stat;
					text2 = $"+{buffEffectDefinition.GetModifierValue(buffEffectDefinition.GetPerkContext(SkillOwner))}";
				}
				else if (debuffEffectDefinition != null)
				{
					e_Stat = debuffEffectDefinition.Stat;
					text2 = $"-{debuffEffectDefinition.GetModifierValue(debuffEffectDefinition.GetPerkContext(SkillOwner))}";
				}
				text = ColoredName(string.Format("<size=125%>{0}</size>{1} <sprite name=\"{2}\"> {3}", text2, e_Stat.ShownAsPercentage() ? "%" : string.Empty, e_Stat, UnitDatabase.UnitStatDefinitions[e_Stat].Name));
			}
			else
			{
				text = ColoredName(SkillManager.GetSkillEffectName(SkillEffectDefinition.Id));
			}
		}
		if (flag2)
		{
			titleText.text = (flag ? $"{text} (<color=#{negativeColor._HexCode}>{Mathf.RoundToInt(num * 100f)}%</color>)" : $"{text} ({Mathf.RoundToInt(num * 100f)}%)");
		}
		else
		{
			titleText.text = text;
		}
	}
}
