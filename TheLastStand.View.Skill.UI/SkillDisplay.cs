using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.UI.Effect;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class SkillDisplay : SerializedMonoBehaviour
{
	public static class Constants
	{
		public static class DamageType
		{
			public const string AdaptativeDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage";

			public const string AdaptativeDamageModifierIconPath = "View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage_Modifier";

			public const string MagicalDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_MagicalDamage";

			public const string PhysicalDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_PhysicalDamage";

			public const string RangedDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_RangedDamage";
		}

		public const float GridMaxXPosition = 210f;

		public const float GridYPositionOffset = 49f;

		public const float GridXPositionOffset = -5f;

		public static readonly Color RemainingUsesColor = new Color(0f, 0f, 0f);
	}

	[SerializeField]
	private GameObject browseTextsContainer;

	[SerializeField]
	private SkillTooltip skillTooltip;

	[SerializeField]
	private SkillTooltipDisplayer skillTooltipDisplayer;

	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private TextMeshProUGUI skillNameDisplay;

	[SerializeField]
	[Tooltip("If false, the name will only be displayed when the icon is not found (Image unlinked or actual icon not found in resources).")]
	private bool alwaysDisplayName;

	[SerializeField]
	private SkillStatDisplay actionPointsCostDisplay;

	[SerializeField]
	private bool hideActionPointsCostIfZero;

	[SerializeField]
	private SkillStatDisplay healthCostDisplay;

	[SerializeField]
	private bool hideHealthCostIfZero;

	[SerializeField]
	private SkillStatDisplay movePointsCostDisplay;

	[SerializeField]
	private bool hideMovePointsCostIfZero;

	[SerializeField]
	private SkillStatDisplay manaCostDisplay;

	[SerializeField]
	private bool hideManaCostIfZero;

	[SerializeField]
	private SkillParameterDisplay remainingUseDisplay;

	[SerializeField]
	private TextMeshProUGUI remainingUseDisplayText;

	[SerializeField]
	private bool displayRemainingUseLabel;

	[SerializeField]
	private GameObject remainingUseDisplayHolder;

	[SerializeField]
	private SkillParameterDisplay remainingChargesDisplay;

	[SerializeField]
	private GameObject remainingChargesDisplaySeparator;

	[SerializeField]
	private BetterButton usesPerTurnSmallButton;

	[SerializeField]
	private TextMeshProUGUI usesPerTurnSmallText;

	[SerializeField]
	private BetterButton usesPerTurnBigButton;

	[SerializeField]
	private TextMeshProUGUI usesPerTurnBigText;

	[SerializeField]
	private TextMeshProUGUI skillPointsCostDisplay;

	[SerializeField]
	private bool displaySkillPointsCostLabel;

	[SerializeField]
	private RectTransform skillParametersParent;

	[SerializeField]
	private RectTransform skillParametersContainer;

	[SerializeField]
	private PixelPerfectVerticalLayoutGroup skillParametersLayout;

	[SerializeField]
	private TextMeshProUGUI skillDescriptionText;

	[SerializeField]
	private RectTransform skillDescriptionRect;

	[SerializeField]
	private SkillParameterDisplay damageDisplay;

	[SerializeField]
	[Tooltip("If true, the value displayed is the skill's base DMG (raw damage). Otherwise, it will be the caster DMG (all available modifiers related to the caster taken into account).")]
	private bool damageValueIsBaseDamage;

	[SerializeField]
	private DataColorDictionary damageTypeColor;

	[SerializeField]
	private Image damageTypeIcon;

	[SerializeField]
	private Image additionalDamageIcon;

	[SerializeField]
	private GameObject noDamageTypeSpacing;

	[SerializeField]
	private RectTransform additionalDamageIconContainer;

	[SerializeField]
	private RangeSkillParameterDisplay rangeDisplay;

	[SerializeField]
	private RectTransform iconsParent;

	[SerializeField]
	private SkillParameterDisplay targetsDisplay;

	[SerializeField]
	private SkillParameterDisplay maxUsesPerTurnDisplay;

	[SerializeField]
	private RectTransform verticalSeparatorRect;

	[SerializeField]
	private SkillAreaOfEffectGrid skillAreaOfEffectGrid;

	[SerializeField]
	private TextMeshProUGUI targetingDisplay;

	[SerializeField]
	private bool displayTargetingLabel;

	[SerializeField]
	private TextMeshProUGUI critDisplay;

	[SerializeField]
	private bool displayCritLabel;

	[SerializeField]
	private TextMeshProUGUI effectDisplay;

	[SerializeField]
	private GameObject effectsContainerGameObject;

	[SerializeField]
	private RectTransform effectsContainerRect;

	[SerializeField]
	private RectTransform effectsParent;

	[SerializeField]
	private SkillEffectDisplay skillEffectPrefab;

	[SerializeField]
	private bool displayPerkSkillEffects;

	protected bool fullRefreshNeeded = true;

	private TheLastStand.Model.Skill.Skill skill;

	private ISkillCaster skillOwner;

	private List<SkillEffectDisplay> effectDisplays = new List<SkillEffectDisplay>();

	public bool DisplayPerkSkillEffects => displayPerkSkillEffects;

	public SkillAreaOfEffectGrid SkillAreaOfEffectGrid => skillAreaOfEffectGrid;

	public TheLastStand.Model.Skill.Skill Skill
	{
		get
		{
			return skill;
		}
		set
		{
			if (value != skill)
			{
				skill = value;
				this.SkillChangedEvent?.Invoke(value);
				fullRefreshNeeded = true;
			}
		}
	}

	public ISkillCaster SkillOwner
	{
		get
		{
			return skillOwner;
		}
		set
		{
			if (value != skillOwner)
			{
				fullRefreshNeeded = true;
				skillOwner = value;
			}
		}
	}

	public TheLastStand.Model.Item.Item ItemSource => skill?.SkillContainer as TheLastStand.Model.Item.Item;

	public RectTransform SkillParametersContainer => skillParametersContainer;

	private bool EditorShowAlwaysDisplayNameField
	{
		get
		{
			if (skillNameDisplay != null)
			{
				return skillIcon != null;
			}
			return false;
		}
	}

	public event Action<TheLastStand.Model.Skill.Skill> SkillChangedEvent;

	public event Action SkillAreaOfEffectGridPlacedEvent;

	public void Init(SkillTooltip newSkillTooltip)
	{
		skillTooltip = newSkillTooltip;
		if (skillTooltipDisplayer != null)
		{
			skillTooltipDisplayer.Init(newSkillTooltip);
		}
	}

	public void Refresh(bool forceFullRefresh = false)
	{
		if (Skill != null)
		{
			fullRefreshNeeded |= forceFullRefresh;
			RefreshInternal();
			fullRefreshNeeded = false;
		}
	}

	public void RefreshEffects(Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		if (Skill == null)
		{
			return;
		}
		if (Skill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition attackSkillActionDefinition)
		{
			PlayableUnit playableUnit = SkillOwner as PlayableUnit;
			if (critDisplay != null)
			{
				int num = Mathf.FloorToInt(attackSkillActionDefinition.CriticProbability + (playableUnit?.GetClampedStatValue(UnitStatDefinition.E_Stat.Critical) ?? 0f));
				critDisplay.text = string.Format("{0}{1}%", displayCritLabel ? "Critical hit: " : string.Empty, num);
			}
			if (effectDisplay != null)
			{
				effectDisplay.text = ((attackSkillActionDefinition.SkillEffectDefinitions != null) ? ("Effects:\n[" + string.Join(";", attackSkillActionDefinition.SkillEffectDefinitions.Keys) + "]") : string.Empty);
			}
		}
		else
		{
			if (critDisplay != null)
			{
				critDisplay.text = string.Empty;
			}
			if (effectDisplay != null)
			{
				effectDisplay.text = string.Empty;
			}
		}
		if (!(effectsParent != null))
		{
			return;
		}
		while (effectDisplays.Count > 0)
		{
			effectDisplays[0].gameObject.SetActive(value: false);
			effectDisplays.RemoveAt(0);
		}
		Dictionary<string, List<SkillEffectDefinition>> dictionary = (displayPerkSkillEffects ? Skill.SkillAction.GetAllEffects() : Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions);
		if (dictionary != null && dictionary.Count > 0)
		{
			effectsContainerGameObject.SetActive(value: true);
			foreach (KeyValuePair<string, List<SkillEffectDefinition>> item in dictionary)
			{
				foreach (SkillEffectDefinition item2 in item.Value)
				{
					if (item2 is SurroundingEffectDefinition surroundingEffectDefinition)
					{
						foreach (SkillEffectDefinition skillEffectDefinition in surroundingEffectDefinition.SkillEffectDefinitions)
						{
							InstantiateSkillEffectDisplay(skillEffectDefinition, isSurrounding: true, casterEffect: false, statModifiers);
						}
					}
					else if (item2 is CasterEffectDefinition casterEffectDefinition)
					{
						foreach (SkillEffectDefinition skillEffectDefinition2 in casterEffectDefinition.SkillEffectDefinitions)
						{
							InstantiateSkillEffectDisplay(skillEffectDefinition2, isSurrounding: false, casterEffect: true, statModifiers);
						}
					}
					else if (!(item2 is ExileCasterEffectDefinition))
					{
						InstantiateSkillEffectDisplay(item2, isSurrounding: false, casterEffect: false, statModifiers);
					}
				}
			}
			if (effectDisplays.Count > 0)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(effectsParent);
				effectsContainerRect.sizeDelta = new Vector2(effectsContainerRect.sizeDelta.x, effectsContainerGameObject.activeInHierarchy ? effectsParent.sizeDelta.y : 10f);
			}
			else
			{
				effectsContainerGameObject.SetActive(value: false);
				effectsContainerRect.sizeDelta = new Vector2(effectsContainerRect.sizeDelta.x, 10f);
			}
		}
		else
		{
			effectsContainerGameObject.SetActive(value: false);
			effectsContainerRect.sizeDelta = new Vector2(effectsContainerRect.sizeDelta.x, 10f);
		}
	}

	public void ToggleBrowseLabel(bool state)
	{
		if (browseTextsContainer != null)
		{
			browseTextsContainer.SetActive(state);
		}
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private Vector3 ComputeSkillAreaOfEffectGridPosition()
	{
		float num = iconsParent.localPosition.x + iconsParent.sizeDelta.x + -5f;
		if (maxUsesPerTurnDisplay != null && maxUsesPerTurnDisplay.gameObject.activeSelf)
		{
			float b = maxUsesPerTurnDisplay.GetPositionAtTheEndOfLine() + -5f;
			num = Mathf.Max(num, b);
		}
		if (remainingUseDisplay != null && remainingUseDisplay.gameObject.activeSelf)
		{
			float b2 = remainingUseDisplay.GetPositionAtTheEndOfLine() + -5f;
			num = Mathf.Max(num, b2);
		}
		num = Mathf.Max(targetsDisplay.GetPositionAtTheEndOfLine() + -5f, num);
		Vector3 localPosition = skillAreaOfEffectGrid.RectTransform.localPosition;
		localPosition.x = Mathf.Min(num, 210f);
		localPosition.y = targetsDisplay.GetVerticalPosition() + 49f;
		return localPosition;
	}

	private void InstantiateSkillEffectDisplay(SkillEffectDefinition skillEffectDefinition, bool isSurrounding = false, bool casterEffect = false, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		if (skillEffectDefinition.ShouldBeDisplayed)
		{
			SkillEffectDisplay skillEffectDisplay = ((SingletonBehaviour<ObjectPooler>.Instance == null) ? UnityEngine.Object.Instantiate(skillEffectPrefab, effectsParent) : ObjectPooler.GetPooledComponent("SkillEffectDisplay", skillEffectPrefab, effectsParent));
			skillEffectDisplay.Init(isFromPerk: isSurrounding ? Skill.SkillAction.HasSurroundingEffectFromPerk(skillEffectDefinition.Id) : ((!casterEffect) ? Skill.SkillAction.HasSkillEffectFromPerk(skillEffectDefinition.Id) : Skill.SkillAction.HasCasterEffectFromPerk(skillEffectDefinition.Id)), skillEffect: skillEffectDefinition, skillOwner: SkillOwner, skillAction: Skill.SkillAction, isSurrounding: isSurrounding, casterEffect: casterEffect, statModifiers: statModifiers);
			effectDisplays.Add(skillEffectDisplay);
		}
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			Refresh(forceFullRefresh: true);
		}
	}

	private IEnumerator PlaceSkillAreaOfEffectGrid()
	{
		yield return SharedYields.WaitForEndOfFrame;
		skillAreaOfEffectGrid.transform.localPosition = ComputeSkillAreaOfEffectGridPosition();
		this.SkillAreaOfEffectGridPlacedEvent?.Invoke();
		SkillAreaOfEffectGrid.Canvas.sortingOrder++;
		yield return SharedYields.WaitForEndOfFrame;
		SkillAreaOfEffectGrid.Canvas.sortingOrder--;
	}

	private void RefreshDamageTypeIcon()
	{
		if (damageTypeIcon == null)
		{
			return;
		}
		AttackSkillAction attackSkillAction = Skill.SkillAction as AttackSkillAction;
		damageTypeIcon.enabled = attackSkillAction != null;
		if (attackSkillAction == null)
		{
			return;
		}
		if (attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Adaptative)
		{
			damageTypeIcon.enabled = false;
			if (noDamageTypeSpacing != null)
			{
				noDamageTypeSpacing.SetActive(value: true);
			}
			return;
		}
		damageTypeIcon.enabled = true;
		if (noDamageTypeSpacing != null)
		{
			noDamageTypeSpacing.SetActive(value: false);
		}
		if (noDamageTypeSpacing != null)
		{
			noDamageTypeSpacing.SetActive(value: false);
		}
		switch (attackSkillAction.AttackType)
		{
		case AttackSkillActionDefinition.E_AttackType.Physical:
			damageTypeIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_PhysicalDamage");
			break;
		case AttackSkillActionDefinition.E_AttackType.Magical:
			damageTypeIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_MagicalDamage");
			break;
		case AttackSkillActionDefinition.E_AttackType.Ranged:
			damageTypeIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_RangedDamage");
			break;
		}
	}

	private void RefreshIcon()
	{
		if (!(skillIcon == null))
		{
			skillIcon.sprite = SkillView.GetIconSprite(skill.SkillDefinition.ArtId);
			skillIcon.enabled = skillIcon.sprite != null;
		}
	}

	private void RefreshInternal()
	{
		Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null;
		if (TPSingleton<ItemManager>.Exist() && ItemSource != null && !(ItemSource.ItemSlot is EquipmentSlot))
		{
			statModifiers = (ItemSource.IsTwoHandedWeapon ? TPSingleton<ItemManager>.Instance.GetStatsDiffBetweenItems(ItemSource, TPSingleton<ItemManager>.Instance.EquippedItemBeingCompared, TPSingleton<ItemManager>.Instance.EquippedItemBeingComparedOffHand) : TPSingleton<ItemManager>.Instance.GetStatsDiffBetweenItems(ItemSource, TPSingleton<ItemManager>.Instance.EquippedItemBeingCompared));
		}
		RefreshRequirements();
		RefreshEffects(statModifiers);
		RefreshDamageTypeIcon();
		if (fullRefreshNeeded)
		{
			RefreshIcon();
			RefreshName();
			RefreshTargeting(statModifiers);
			if (skillTooltip != null)
			{
				skillTooltip.SetContent(Skill, SkillOwner);
			}
			else
			{
				SkillManager.SkillInfoPanel.SetContent(Skill, SkillOwner);
			}
		}
	}

	private void RefreshName()
	{
		if (!(skillNameDisplay == null))
		{
			if (alwaysDisplayName || skillIcon?.sprite == null)
			{
				skillNameDisplay.text = skill.Name;
				skillNameDisplay.gameObject.SetActive(value: true);
			}
			else
			{
				skillNameDisplay.gameObject.SetActive(value: false);
			}
		}
	}

	private void RefreshRequirements()
	{
		PlayableUnit playableUnit = SkillOwner as PlayableUnit;
		Skill.SkillAction.SkillActionController.EnsurePerkData();
		RefreshRequirement(actionPointsCostDisplay, Skill.BaseActionPointsCost, Skill.SkillAction.SkillActionController.ComputeActionPointsCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.ActionPoints, hideActionPointsCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.ActionPointsCost) ?? false);
		RefreshRequirement(movePointsCostDisplay, Skill.BaseMovePointsCost, Skill.SkillAction.SkillActionController.ComputeMovePointsCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.MovePoints, hideMovePointsCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.MovePointsCost) ?? false);
		RefreshRequirement(manaCostDisplay, Skill.BaseManaCost, Skill.SkillAction.SkillActionController.ComputeManaCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.Mana, hideManaCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.ManaCost) ?? false);
		RefreshRequirement(healthCostDisplay, Skill.SkillAction.SkillActionController.ComputeBaseHealthCost(playableUnit), Skill.SkillAction.SkillActionController.ComputeHealthCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.Health, hideHealthCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.HealthCost) ?? false);
		RefreshRequirementOverallRemainingCharges();
		RefreshRequirementOverallRemainingUse();
		RefreshRequirementRemainingUsePerTurn();
		RefreshRequirementSkillPointsCost();
	}

	private void RefreshRequirement(SkillStatDisplay statDisplay, int baseCost, int finalCost, UnitStatDefinition.E_Stat stat, bool hideIfZero, bool isLocked)
	{
		if (!(statDisplay == null))
		{
			PlayableUnit playableUnit = SkillOwner as PlayableUnit;
			if (playableUnit != null && playableUnit.GetClampedStatValue(stat) < (float)finalCost)
			{
				statDisplay.ColorOverride = GameView.NegativeColor;
			}
			else if (playableUnit != null && baseCost != finalCost && baseCost != 0)
			{
				statDisplay.ColorOverride = ((finalCost > baseCost) ? GameView.NegativeColor : GameView.PositiveColor);
			}
			else
			{
				statDisplay.ColorOverride = null;
			}
			if (fullRefreshNeeded)
			{
				statDisplay.Display(!isLocked && (!hideIfZero || baseCost != 0 || finalCost != 0));
				statDisplay.Skill = skill;
				statDisplay.Refresh();
			}
		}
	}

	private void RefreshRequirementOverallRemainingCharges()
	{
		BattleModule battleModule = SkillOwner as BattleModule;
		bool flag = battleModule?.BuildingParent.IsTrap ?? false;
		if (remainingChargesDisplay != null)
		{
			remainingChargesDisplay.Display(flag);
		}
		if (remainingChargesDisplaySeparator != null)
		{
			remainingChargesDisplaySeparator.SetActive(flag);
		}
		if ((fullRefreshNeeded || flag) && remainingChargesDisplay != null && flag)
		{
			string text = string.Empty;
			if (flag)
			{
				text = $"{battleModule.RemainingTrapCharges}/{battleModule.BuildingParent.BattleModule.BattleModuleDefinition.MaximumTrapCharges}";
			}
			string format = "<style=RemainingCharges>{0}</color>";
			remainingChargesDisplay.Refresh("SkillTooltip_OverallUsesCount_Trap", string.Format(format, text ?? ""));
		}
	}

	private void RefreshRequirementOverallRemainingUse()
	{
		bool flag = skill.OverallUses >= 0;
		if (remainingUseDisplayHolder != null)
		{
			remainingUseDisplayHolder.SetActive(flag);
		}
		else if (remainingUseDisplay != null)
		{
			remainingUseDisplay.Display(flag);
		}
		else
		{
			if (!(remainingUseDisplayText != null))
			{
				return;
			}
			remainingUseDisplayText.gameObject.SetActive(flag);
		}
		if ((fullRefreshNeeded || flag) && flag)
		{
			int num = ((TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day) ? skill.ComputeTotalUses(SkillOwner) : skill.OverallUsesRemaining);
			string format = ((num == 0) ? "<style=Bad>{0}</style>" : "<style=Skill>{0}</style>");
			if (remainingUseDisplay != null)
			{
				remainingUseDisplay.Refresh("SkillTooltip_OverallUsesCount", string.Format(format, string.Format("{0}{1}", displayRemainingUseLabel ? "Remaining uses: " : string.Empty, num)), 0);
			}
			else
			{
				remainingUseDisplayText.text = string.Format(format, string.Format("{0}{1}", displayRemainingUseLabel ? "Remaining uses: " : string.Empty, num));
			}
		}
	}

	private void RefreshRequirementRemainingUsePerTurn()
	{
		if (usesPerTurnSmallButton == null || usesPerTurnBigButton == null || (!fullRefreshNeeded && skill.UsesPerTurnRemaining < 0))
		{
			return;
		}
		usesPerTurnSmallButton.gameObject.SetActive(skill.UsesPerTurnRemaining >= 0 && skill.OverallUsesRemaining != -1);
		usesPerTurnBigButton.gameObject.SetActive(skill.UsesPerTurnRemaining >= 0 && skill.OverallUsesRemaining == -1);
		if (skill.UsesPerTurnRemaining >= 0)
		{
			if (skill.OverallUsesRemaining != -1)
			{
				usesPerTurnSmallText.text = $"{skill.UsesPerTurnRemaining}/{skill.UsesPerTurn}";
				UpdateUsesPerTurnTextColor(usesPerTurnSmallButton, usesPerTurnSmallText);
			}
			else
			{
				usesPerTurnBigText.text = $"{skill.UsesPerTurnRemaining}/{skill.UsesPerTurn}";
				UpdateUsesPerTurnTextColor(usesPerTurnBigButton, usesPerTurnBigText);
			}
		}
	}

	private void RefreshRequirementSkillPointsCost()
	{
		if (!(skillPointsCostDisplay == null) && fullRefreshNeeded)
		{
			int num = 0;
			skillPointsCostDisplay.text = string.Format("{0}{1}", displaySkillPointsCostLabel ? "Skill points: " : "x", num);
		}
	}

	private void RefreshTargeting(Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		AttackSkillAction attackSkillAction = Skill.SkillAction as AttackSkillAction;
		if (skillDescriptionText != null)
		{
			skillDescriptionText.text = Skill.Description;
			LayoutRebuilder.ForceRebuildLayoutImmediate(skillDescriptionText.rectTransform);
		}
		if (damageDisplay != null)
		{
			damageDisplay.gameObject.SetActive(attackSkillAction != null);
			if (attackSkillAction != null)
			{
				Vector2Int v = ((damageValueIsBaseDamage || SkillOwner == null) ? attackSkillAction.AttackSkillActionController.ComputeBaseDamageRange() : attackSkillAction.AttackSkillActionController.ComputeCasterDamageRange(SkillOwner, isSurroundingTile: false, statModifiers));
				Color? valueColor = damageTypeColor?.GetColorById(attackSkillAction.AttackType.ToString()).Value;
				damageDisplay.Refresh("SkillTooltip_Damage", v.GetSimplifiedRange(), valueColor);
			}
		}
		if (additionalDamageIcon != null)
		{
			if (attackSkillAction != null && attackSkillAction.AttackSkillActionDefinition.AttackType == AttackSkillActionDefinition.E_AttackType.Adaptative)
			{
				additionalDamageIconContainer.gameObject.SetActive(value: true);
				additionalDamageIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage_Modifier") ?? ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage");
			}
			else
			{
				additionalDamageIconContainer.gameObject.SetActive(value: false);
			}
		}
		if (rangeDisplay != null)
		{
			string empty = string.Empty;
			int num = ((SkillOwner is TheLastStand.Model.Unit.Unit unit) ? unit.UnitController.GetModifiedMaxRange(Skill, statModifiers) : Skill.SkillDefinition.Range.y);
			empty = (skill.SkillDefinition.InfiniteRange ? Localizer.Get("SkillTooltip_RangeAnywhere") : ((Skill.SkillDefinition.Range.x > 1 || Skill.SkillDefinition.Range.x != num) ? $"{Skill.SkillDefinition.Range.x}{((Skill.SkillDefinition.Range.x != num) ? $"-{num}" : string.Empty)}" : ((Skill.SkillDefinition.Range.x != 1) ? Localizer.Get("SkillTooltip_RangeSelf") : Localizer.Get("SkillTooltip_RangeMelee"))));
			bool flag = skill.SkillController.ComputeMaxRange() > 1;
			rangeDisplay.Refresh("SkillTooltip_Range", empty, flag && Skill.SkillDefinition.CardinalDirectionOnly, Skill.SkillDefinition.RangeModifiable);
		}
		if (targetingDisplay != null)
		{
			targetingDisplay.text = (displayTargetingLabel ? "Targeting: " : string.Empty) + (Skill.SkillDefinition.CardinalDirectionOnly ? Localizer.Get("SkillTooltip_Cardinal") : "Free");
		}
		if (targetsDisplay != null)
		{
			targetsDisplay.Refresh("SkillTooltip_Targets", $"{Mathf.Max(1, Skill.SkillDefinition.AffectedTilesCount)} {((Skill.SkillDefinition.SurroundingEffectTilesCount > 0) ? $" (+{Skill.SkillDefinition.SurroundingEffectTilesCount})" : string.Empty)}");
		}
		int usesPerTurnCountFromDefinition = skill.UsesPerTurnCountFromDefinition;
		if (maxUsesPerTurnDisplay != null)
		{
			maxUsesPerTurnDisplay.Refresh("SkillTooltip_UsePerTurn", ((usesPerTurnCountFromDefinition != -1) ? usesPerTurnCountFromDefinition.ToString() : Localizer.Get("SkillTooltip_UsePerTurnUnlimited")) ?? "");
		}
		if (remainingUseDisplay != null)
		{
			maxUsesPerTurnDisplay.Refresh("SkillTooltip_UsePerTurn", ((usesPerTurnCountFromDefinition != -1) ? usesPerTurnCountFromDefinition.ToString() : Localizer.Get("SkillTooltip_UsePerTurnUnlimited")) ?? "");
		}
		if (skillParametersContainer != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(skillParametersContainer);
		}
		if (verticalSeparatorRect != null)
		{
			verticalSeparatorRect.sizeDelta = new Vector2(verticalSeparatorRect.sizeDelta.x, skillParametersContainer.sizeDelta.y - (float)skillParametersLayout.padding.top - (float)skillParametersLayout.padding.bottom - skillDescriptionRect.sizeDelta.y);
		}
		if (skillParametersParent != null && skillParametersContainer != null)
		{
			skillParametersParent.sizeDelta = new Vector2(skillParametersParent.sizeDelta.x, skillParametersContainer.sizeDelta.y + (float)skillParametersLayout.padding.bottom);
		}
		if (skillAreaOfEffectGrid != null)
		{
			skillAreaOfEffectGrid.Refresh(Skill);
			if (rangeDisplay != null && skillAreaOfEffectGrid.Displayed)
			{
				StartCoroutine(PlaceSkillAreaOfEffectGrid());
			}
			else
			{
				this.SkillAreaOfEffectGridPlacedEvent?.Invoke();
			}
		}
	}

	private void Start()
	{
		if (actionPointsCostDisplay != null)
		{
			actionPointsCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
		}
		if (healthCostDisplay != null)
		{
			healthCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		}
		if (movePointsCostDisplay != null)
		{
			movePointsCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
		}
		if (manaCostDisplay != null)
		{
			manaCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Mana];
		}
		Refresh(forceFullRefresh: true);
	}

	public void UpdateUsesPerTurnTextColor(BetterButton button, TextMeshProUGUI text)
	{
		if (button.interactable)
		{
			text.color = SkillManager.AvailableUsesPerTurnColor;
		}
		else if (skill.UsesPerTurnRemaining == 0)
		{
			text.color = SkillManager.NoUsesPerTurnColor;
		}
		else
		{
			text.color = SkillManager.UnavailableUsesPerTurnColor;
		}
	}

	[ContextMenu("Force Full Refresh")]
	private void DebugForceFullRefresh()
	{
		Refresh(forceFullRefresh: true);
	}
}
