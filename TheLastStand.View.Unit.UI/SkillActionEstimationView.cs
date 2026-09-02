using System;
using System.Collections.Generic;
using TPLib.Log;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;
using TheLastStand.Framework;
using TheLastStand.Framework.Animation;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class SkillActionEstimationView : MonoBehaviour
{
	private static class Constants
	{
		public const string ResourcesMainPath = "View/Sprites/UI/Units/AttackEstimation";

		public const string SecondaryIconsFolderName = "SecondaryIcons";

		public const string IconPrefix = "Icon_";

		public const string FriendlyFireFolderName = "FriendlyFire";

		public const string SurroundingFolderName = "Surrounding";

		public const string ImpossibleDeathFolderName = "NoDeath";

		public const string PossibleDeathFolderName = "PossibleDeath";

		public const string GuaranteedDeathFolderName = "GuaranteedDeath";
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private MultipleImageAnimator multipleImageAnimator;

	[SerializeField]
	private ImageAnimator mainEstimationImageAnimator;

	[SerializeField]
	private Image secondEstimationImage;

	[SerializeField]
	[Range(0f, 1f)]
	private float invalidSkillAlpha = 0.3f;

	private bool mainIconHasBeenSet;

	private string mainSpritesPath;

	private List<string> mainEffectSprites = new List<string>();

	private bool secondaryIconHasBeenSet;

	private string secondarySpritesPath;

	private bool hasCanvas;

	private bool isDisplayed;

	public event Action<bool> OnViewToggled = delegate
	{
	};

	public void Display(SkillActionExecution skillActionExecution, Tile targetTile, TheLastStand.Model.Unit.Unit affectedUnit, bool isSurroundingEffect, bool casterEffect)
	{
		if (affectedUnit == null)
		{
			return;
		}
		if (!isDisplayed)
		{
			this.OnViewToggled(obj: true);
		}
		isDisplayed = true;
		if (hasCanvas)
		{
			canvas.enabled = true;
		}
		float a = (SkillManager.IsSelectedSkillValid ? 1f : invalidSkillAlpha);
		multipleImageAnimator.Image.color = multipleImageAnimator.Image.color.WithA(a);
		mainEstimationImageAnimator.Image.color = mainEstimationImageAnimator.Image.color.WithA(a);
		secondEstimationImage.color = secondEstimationImage.color.WithA(a);
		mainIconHasBeenSet = false;
		secondaryIconHasBeenSet = false;
		mainEffectSprites.Clear();
		mainSpritesPath = "View/Sprites/UI/Units/AttackEstimation/";
		secondarySpritesPath = "View/Sprites/UI/Units/AttackEstimation/SecondaryIcons/";
		EnemyUnit enemyUnit = affectedUnit as EnemyUnit;
		bool flag = skillActionExecution.Skill.SkillDefinition.CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster, isSurroundingEffect);
		bool flag2 = skillActionExecution.Skill.SkillDefinition.CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit, isSurroundingEffect);
		bool flag3 = skillActionExecution.Skill.SkillDefinition.CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit, isSurroundingEffect);
		bool flag4 = affectedUnit == skillActionExecution.Caster;
		if (affectedUnit is PlayableUnit && flag2 && !casterEffect && (!flag4 || flag))
		{
			SkillAction skillAction = skillActionExecution.Skill.SkillAction;
			if (!(skillAction is AttackSkillAction))
			{
				if (skillAction is GenericSkillAction genericSkillAction)
				{
					DisplayGenericEstimationForPlayableUnit(genericSkillAction);
				}
				else
				{
					CLoggerManager.Log("Unhandled SkillAction type SkillAction to display skill execution estimation!", LogType.Warning);
				}
			}
			else
			{
				DisplayAttackEstimationForPlayableUnit(flag4);
			}
		}
		else if (enemyUnit != null && flag3)
		{
			if (isSurroundingEffect)
			{
				SurroundingEffectDefinition firstEffect = skillActionExecution.Skill.SkillAction.GetFirstEffect<SurroundingEffectDefinition>("SurroundingEffect");
				DisplaySurroundingEstimationForEnemyUnit(skillActionExecution, firstEffect, enemyUnit, targetTile);
			}
			else
			{
				SkillAction skillAction = skillActionExecution.Skill.SkillAction;
				if (!(skillAction is AttackSkillAction attackSkillAction))
				{
					if (skillAction is GenericSkillAction genericSkillAction2)
					{
						DisplayGenericEstimationForEnemyUnit(genericSkillAction2);
					}
					else
					{
						CLoggerManager.Log("Unhandled SkillAction type SkillAction to display skill execution estimation!", LogType.Warning);
					}
				}
				else
				{
					DisplayAttackEstimationForEnemyUnit(skillActionExecution, attackSkillAction, targetTile, enemyUnit);
				}
			}
		}
		if (casterEffect && skillActionExecution.Skill.SkillAction.TryGetFirstEffect<CasterEffectDefinition>("CasterEffect", out var effect))
		{
			DisplayCasterEstimation(effect);
		}
		if (mainIconHasBeenSet)
		{
			OnSpritesPathsComputed();
			return;
		}
		enemyUnit?.EnemyUnitView.EnemyUnitHUD.DisplayIconFeedback();
		Hide();
	}

	public void DisplayBuildingAction(BuildingAction buildingAction, TheLastStand.Model.Unit.Unit affectedUnit)
	{
		if (affectedUnit == null)
		{
			return;
		}
		if (!isDisplayed)
		{
			this.OnViewToggled(obj: true);
		}
		isDisplayed = true;
		if (hasCanvas)
		{
			canvas.enabled = true;
		}
		float a = (buildingAction.BuildingActionController.CanExecuteActionOnTile(affectedUnit.OriginTile) ? 1f : invalidSkillAlpha);
		multipleImageAnimator.Image.color = multipleImageAnimator.Image.color.WithA(a);
		mainEstimationImageAnimator.Image.color = mainEstimationImageAnimator.Image.color.WithA(a);
		mainIconHasBeenSet = false;
		secondaryIconHasBeenSet = false;
		mainEffectSprites.Clear();
		mainSpritesPath = "View/Sprites/UI/Units/AttackEstimation/";
		secondarySpritesPath = "View/Sprites/UI/Units/AttackEstimation/SecondaryIcons/";
		foreach (BuildingActionEffect buildingActionEffect in buildingAction.BuildingActionEffects)
		{
			string actionEstimationIconId = buildingActionEffect.BuildingActionEffectDefinition.ActionEstimationIconId;
			if (!string.IsNullOrEmpty(actionEstimationIconId))
			{
				mainEffectSprites.Add(mainSpritesPath + actionEstimationIconId);
				mainIconHasBeenSet = true;
			}
		}
		if (mainIconHasBeenSet)
		{
			OnSpritesPathsComputed();
		}
		else
		{
			Hide();
		}
	}

	public void DisplayHoveredBuildingActionEffects(BuildingAction buildingAction, TheLastStand.Model.Unit.Unit affectedUnit)
	{
		bool flag = buildingAction.BuildingActionController.CanExecuteActionOnTile(affectedUnit.OriginTile);
		mainIconHasBeenSet = false;
		secondaryIconHasBeenSet = false;
		mainEffectSprites.Clear();
		mainSpritesPath = "View/Sprites/UI/Units/AttackEstimation/";
		secondarySpritesPath = "View/Sprites/UI/Units/AttackEstimation/SecondaryIcons/";
		foreach (BuildingActionEffect buildingActionEffect in buildingAction.BuildingActionEffects)
		{
			HealBuildingActionEffect healBuildingActionEffect = buildingActionEffect as HealBuildingActionEffect;
			if (healBuildingActionEffect != null && healBuildingActionEffect.HealBuildingActionDefinition.BuildingActionTargeting != BuildingActionEffectDefinition.E_BuildingActionTargeting.All)
			{
				continue;
			}
			HealManaBuildingActionEffect healManaBuildingActionEffect = buildingActionEffect as HealManaBuildingActionEffect;
			if ((healManaBuildingActionEffect == null || healManaBuildingActionEffect.HealManaBuildingActionDefinition.BuildingActionTargeting == BuildingActionEffectDefinition.E_BuildingActionTargeting.All) && (healBuildingActionEffect != null || healManaBuildingActionEffect != null))
			{
				if (healBuildingActionEffect != null)
				{
					flag &= affectedUnit.Health < affectedUnit.HealthTotal;
				}
				if (healManaBuildingActionEffect != null)
				{
					flag &= affectedUnit.Mana < affectedUnit.ManaTotal;
				}
				string actionEstimationIconId = buildingActionEffect.BuildingActionEffectDefinition.ActionEstimationIconId;
				if (!string.IsNullOrEmpty(actionEstimationIconId))
				{
					mainEffectSprites.Add(mainSpritesPath + actionEstimationIconId);
					mainIconHasBeenSet = true;
				}
			}
		}
		if (mainIconHasBeenSet)
		{
			float a = (flag ? 1f : invalidSkillAlpha);
			multipleImageAnimator.Image.color = multipleImageAnimator.Image.color.WithA(a);
			mainEstimationImageAnimator.Image.color = mainEstimationImageAnimator.Image.color.WithA(a);
			OnSpritesPathsComputed();
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		if (isDisplayed)
		{
			this.OnViewToggled(obj: false);
		}
		isDisplayed = false;
		if (hasCanvas)
		{
			canvas.enabled = false;
		}
		multipleImageAnimator.Stop();
		multipleImageAnimator.gameObject.SetActive(value: false);
		mainEstimationImageAnimator.Stop();
		mainEstimationImageAnimator.gameObject.SetActive(value: false);
		secondEstimationImage.gameObject.SetActive(value: false);
	}

	private void Awake()
	{
		hasCanvas = canvas != null;
	}

	private void DisplayDamageEstimation(ISkillCaster skillCaster, AttackSkillAction attackSkillAction, Tile targetTile, EnemyUnit affectedEnemyUnit, bool isSurrounding = false)
	{
		Vector2Int finalDamageRange = attackSkillAction.AttackSkillActionController.ComputeFinalDamageRange(targetTile, skillCaster, isSurrounding).FinalDamageRange;
		attackSkillAction.AttackSkillActionController.SplitDamageBetweenArmorAndHealth(affectedEnemyUnit, finalDamageRange.x, out var armorDamage, out var healthDamage);
		if (healthDamage >= affectedEnemyUnit.Health)
		{
			mainSpritesPath += "GuaranteedDeath";
			return;
		}
		attackSkillAction.AttackSkillActionController.SplitDamageBetweenArmorAndHealth(affectedEnemyUnit, finalDamageRange.y, out armorDamage, out var healthDamage2);
		mainSpritesPath += ((healthDamage2 >= affectedEnemyUnit.Health) ? "PossibleDeath" : "NoDeath");
	}

	private void DisplayAttackEstimationForPlayableUnit(bool isCaster)
	{
		if (!isCaster)
		{
			mainSpritesPath += "FriendlyFire";
			mainIconHasBeenSet = true;
		}
	}

	private void DisplayAttackEstimationForEnemyUnit(SkillActionExecution skillActionExecution, AttackSkillAction attackSkillAction, Tile targetTile, EnemyUnit affectedEnemyUnit)
	{
		if (attackSkillAction.AttackSkillActionDefinition.DamageMultiplier != 0f)
		{
			DisplayDamageEstimation(skillActionExecution.Caster, attackSkillAction, targetTile, affectedEnemyUnit);
			mainIconHasBeenSet = true;
		}
		if (attackSkillAction.HasEffect("Stun"))
		{
			SetIcon("Stun");
		}
		else if (attackSkillAction.HasEffect("Poison"))
		{
			SetIcon("Poison");
		}
		else if (attackSkillAction.HasEffect("Debuff"))
		{
			SetIcon("Debuff");
		}
		else if (attackSkillAction.HasEffect("RemoveStatus"))
		{
			SetIcon("RemoveStatus");
		}
	}

	private void DisplayCasterEstimation(CasterEffectDefinition casterEffectDefinition)
	{
		foreach (SkillEffectDefinition skillEffectDefinition in casterEffectDefinition.SkillEffectDefinitions)
		{
			SetIcon(skillEffectDefinition.Id);
		}
		OnSpritesPathsComputed();
	}

	private void DisplayGenericEstimationForPlayableUnit(GenericSkillAction genericSkillAction)
	{
		foreach (KeyValuePair<string, List<SkillEffectDefinition>> allEffect in genericSkillAction.GetAllEffects())
		{
			if (allEffect.Key == "IgnoreLineOfSight")
			{
				continue;
			}
			if (allEffect.Key != "Buff" && allEffect.Key != "Debuff" && allEffect.Key != "CasterEffect" && allEffect.Key != "RegenStat" && allEffect.Key != "Charged" && allEffect.Key != "RemoveStatus" && allEffect.Key != "Dispel" && allEffect.Key != "NegativeStatusImmunityEffect" && !allEffect.Key.Contains("DecreaseStat"))
			{
				mainSpritesPath += "FriendlyFire";
				mainIconHasBeenSet = true;
				break;
			}
			if (!(allEffect.Key == "CasterEffect"))
			{
				if (genericSkillAction.TryGetEffects(allEffect.Key, out List<RegenStatSkillEffectDefinition> effects, onlyNative: false))
				{
					SetIcons(effects);
				}
				else
				{
					SetIcon(allEffect.Key);
				}
			}
		}
	}

	private void DisplaySurroundingEstimationForEnemyUnit(SkillActionExecution skillActionExecution, SurroundingEffectDefinition surroundingEffectDefinition, EnemyUnit targetUnit, Tile targetTile)
	{
		mainSpritesPath += "Surrounding/";
		secondarySpritesPath += "Surrounding/";
		if (skillActionExecution.Skill.SkillAction is AttackSkillAction attackSkillAction && surroundingEffectDefinition.HasEffect<DamageSurroundingEffectDefinition>())
		{
			DisplayDamageEstimation(skillActionExecution.Caster, attackSkillAction, targetTile, targetUnit, isSurrounding: true);
			mainIconHasBeenSet = true;
		}
		if (surroundingEffectDefinition.HasEffect<StunEffectDefinition>())
		{
			SetIcon("Stun");
		}
		if (surroundingEffectDefinition.HasEffect<PoisonEffectDefinition>())
		{
			SetIcon("Poison");
		}
		if (surroundingEffectDefinition.HasEffect<DebuffEffectDefinition>())
		{
			SetIcon("Debuff");
		}
	}

	private void DisplayGenericEstimationForEnemyUnit(GenericSkillAction genericSkillAction)
	{
		Dictionary<string, List<SkillEffectDefinition>> allEffects = genericSkillAction.GetAllEffects();
		foreach (string key in allEffects.Keys)
		{
			if (key != "Buff" && key != "RegenStat" && key != "Kill" && key != "IgnoreLineOfSight" && key != "CasterEffect")
			{
				SetIcon(key);
				continue;
			}
			SkillEffectDefinition skillEffectDefinition = allEffects[key][0];
			if (!(skillEffectDefinition is BuffEffectDefinition buffEffectDefinition))
			{
				if (!(skillEffectDefinition is RemoveStatusEffectDefinition removeStatusEffectDefinition))
				{
					if (skillEffectDefinition is RegenStatSkillEffectDefinition regenStatSkillEffectDefinition && regenStatSkillEffectDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit))
					{
						SetIcon(regenStatSkillEffectDefinition.StatName);
					}
					continue;
				}
				if (!removeStatusEffectDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit))
				{
					continue;
				}
			}
			else if (!buffEffectDefinition.AffectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit))
			{
				continue;
			}
			SetIcon(key);
		}
	}

	private void OnSpritesPathsComputed()
	{
		if (mainEffectSprites.Count > 0)
		{
			multipleImageAnimator.gameObject.SetActive(value: true);
			multipleImageAnimator.AllSpritesPath = mainEffectSprites;
			multipleImageAnimator.Refresh();
		}
		else
		{
			mainEstimationImageAnimator.gameObject.SetActive(value: true);
			mainEstimationImageAnimator.SpritesPath = mainSpritesPath;
			mainEstimationImageAnimator.Refresh();
		}
		secondEstimationImage.sprite = ResourcePooler<Sprite>.LoadOnce(secondarySpritesPath, failSilently: true);
	}

	private void SetIcon(string effectName)
	{
		if (!(effectName == "Propagation"))
		{
			if (!mainIconHasBeenSet)
			{
				mainSpritesPath += effectName;
				mainIconHasBeenSet = true;
			}
			else if (!secondaryIconHasBeenSet)
			{
				secondarySpritesPath = secondarySpritesPath + "Icon_" + effectName;
				secondEstimationImage.gameObject.SetActive(value: true);
				secondaryIconHasBeenSet = true;
			}
		}
	}

	private void SetIcons(List<RegenStatSkillEffectDefinition> regenStatSkillEffectDefinitions)
	{
		mainEffectSprites.Clear();
		if (!mainIconHasBeenSet)
		{
			regenStatSkillEffectDefinitions.ForEach(delegate(RegenStatSkillEffectDefinition o)
			{
				mainEffectSprites.Add(mainSpritesPath + o.Stat);
			});
			mainIconHasBeenSet = true;
		}
	}
}
