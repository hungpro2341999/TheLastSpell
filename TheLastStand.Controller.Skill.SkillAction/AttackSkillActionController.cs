using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Building.Module;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Controller.Skill.SkillAction;

public class AttackSkillActionController : SkillActionController
{
	private AttackSkillAction AttackSkillAction => base.SkillAction as AttackSkillAction;

	public AttackSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new AttackSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new AttackSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	public static float ComputeMomentumPercentage(ISkillCaster caster, MomentumEffectDefinition momentumEffect, PerkDataContainer perkDataContainer = null)
	{
		if (momentumEffect == null || !(caster is PlayableUnit playableUnit))
		{
			return 0f;
		}
		float num = playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MomentumAttacks).Final;
		if (perkDataContainer != null && caster is PlayableUnit playableUnit2)
		{
			num += playableUnit2.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.MomentumAttacks, perkDataContainer) / 100f;
		}
		num = playableUnit.UnitStatsController.ClampToBoundaries(num, UnitStatDefinition.E_Stat.MomentumAttacks);
		num = momentumEffect.GetDamageBonusPerTile(momentumEffect.GetPerkContext(caster)) + num * 0.01f;
		return Mathf.Min((float)playableUnit.MomentumTilesActive * num, 4f);
	}

	public Vector2Int ComputeBaseDamageRange(bool isSurroundingTile = false)
	{
		Vector2 baseDamage = AttackSkillAction.BaseDamage;
		float num = AttackSkillAction.AttackSkillActionDefinition.DamageMultiplier;
		if (isSurroundingTile && AttackSkillAction.TryGetFirstEffect<SurroundingEffectDefinition>("SurroundingEffect", out var effect))
		{
			List<DamageSurroundingEffectDefinition> effects = effect.GetEffects<DamageSurroundingEffectDefinition>();
			if (effects.Count > 0)
			{
				num = 0f;
				foreach (DamageSurroundingEffectDefinition item in effects)
				{
					num += item.Multiplier;
				}
			}
			else
			{
				num = 0f;
			}
		}
		baseDamage *= num;
		Vector2Int zero = Vector2Int.zero;
		zero.x = Mathf.Max(Mathf.RoundToInt(baseDamage.x), 0);
		zero.y = Mathf.Max(Mathf.RoundToInt(baseDamage.y), zero.x);
		return zero;
	}

	public Vector2Int ComputeCasterDamageRange(ISkillCaster caster, bool isSurroundingTile = false, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		Vector2Int vector2Int = ComputeBaseDamageRange(isSurroundingTile);
		if (vector2Int.y > 0)
		{
			Vector2 vector = vector2Int;
			if (caster is PlayableUnit playableUnit)
			{
				float final = playableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Reliability).Final;
				final += (float)(int)(statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.Reliability) ?? 0f);
				final += playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.Reliability, base.SkillAction.PerkDataContainer);
				final = playableUnit.UnitStatsController.ClampToBoundaries(final, UnitStatDefinition.E_Stat.Reliability);
				vector.x = Mathf.Min(vector.y, vector.x + (vector.y - vector.x) / 100f * final);
			}
			else if (caster is BattleModule battleModule)
			{
				int num = battleModule.ComputeModifyDefensesDamagePerksPercentage();
				num += TPSingleton<GlyphManager>.Instance.DefensesDamagePercentageModifier;
				if (num != 0)
				{
					vector = vector2Int.AddPercentage((float)num * 0.01f);
				}
			}
			float num2 = 100f;
			TheLastStand.Model.Unit.Unit unit = caster as TheLastStand.Model.Unit.Unit;
			if (unit != null)
			{
				num2 = AttackSkillAction.AttackType switch
				{
					AttackSkillActionDefinition.E_AttackType.Physical => unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.PhysicalDamage, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PhysicalDamage)), 
					AttackSkillActionDefinition.E_AttackType.Magical => unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.MagicalDamage, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.MagicalDamage)), 
					AttackSkillActionDefinition.E_AttackType.Ranged => unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.RangedDamage, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.RangedDamage)), 
					_ => num2, 
				};
			}
			vector *= num2 / 100f;
			if (unit != null)
			{
				vector *= (100f + unit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.OverallDamage, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.OverallDamage))) / 100f;
				vector *= unit.GetClampedStatValue(UnitStatDefinition.E_Stat.InjuryDamageMultiplier) / 100f;
				if (unit is EnemyUnit)
				{
					vector *= unit.GetClampedStatValue(UnitStatDefinition.E_Stat.EnemyEvolutionDamageMultiplier) / 100f;
				}
			}
			vector2Int.x = Mathf.Max(Mathf.RoundToInt(vector.x), 0);
			vector2Int.y = Mathf.Max(Mathf.RoundToInt(vector.y), vector2Int.x);
		}
		return vector2Int;
	}

	public DamageRangeData ComputeFinalDamageRange(Tile targetTile, ISkillCaster caster, bool isSurroundingTile = false)
	{
		EnsurePerkData(base.SkillAction.PerkDataContainer?.TargetTile ?? targetTile, targetTile?.GetDamageable(isUnitPriority: true));
		DamageRangeData damageRangeData = new DamageRangeData
		{
			BaseDamageRange = ComputeCasterDamageRange(caster, isSurroundingTile)
		};
		if (targetTile == null)
		{
			return damageRangeData;
		}
		TheLastStand.Model.Unit.Unit source = caster as TheLastStand.Model.Unit.Unit;
		TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
		if (unit != null)
		{
			damageRangeData.IsolatedDamageRange = ComputeIsolatedDamageRange(damageRangeData.FinalDamageRange, unit, caster, isSurroundingTile);
		}
		if (unit != null)
		{
			damageRangeData.OpportunismDamageRange = ComputeOpportunisticDamageRange(damageRangeData.FinalDamageRange, unit, caster, isSurroundingTile);
		}
		if (base.SkillAction.HasEffect("Momentum"))
		{
			damageRangeData.MomentumDamageRange = ComputeMomentumDamageRange(damageRangeData.FinalDamageRange, caster);
		}
		if (caster is PlayableUnit caster2)
		{
			damageRangeData.PerksDamageRange = ComputePerkDamageRange(damageRangeData.FinalDamageRange, unit, caster2);
		}
		if (damageRangeData.FinalDamageRange.y <= 0 || !IsUnitAffected(targetTile))
		{
			return damageRangeData;
		}
		if (unit != null)
		{
			damageRangeData.ResistanceReductionRange = unit.UnitController.ComputeResistanceReductionRange(damageRangeData.FinalDamageRange, source, AttackSkillAction.AttackType, base.SkillAction.PerkDataContainer);
			damageRangeData.BlockReductionRange = unit.UnitController.ComputeBlockReductionRange(!AttackSkillAction.HasEffect("NoBlock"));
		}
		return damageRangeData;
	}

	public Vector2Int ComputeIsolatedDamageRange(Vector2Int damageRange, TheLastStand.Model.Unit.Unit targetUnit, ISkillCaster caster, bool isSurroundingTile)
	{
		return damageRange.GetRangeFromPercentage(ComputeIsolatedPercentage(targetUnit, caster, isSurroundingTile));
	}

	public float ComputeIsolatedPercentage(TheLastStand.Model.Unit.Unit targetUnit, ISkillCaster caster, bool isSurroundingTile)
	{
		IsolatedSkillEffectDefinition firstEffect = AttackSkillAction.GetFirstEffect<IsolatedSkillEffectDefinition>("Isolated");
		if (!targetUnit.IsIsolated)
		{
			return 0f;
		}
		if (!(caster is TheLastStand.Model.Unit.Unit unit))
		{
			return firstEffect?.GetDamageMultiplier(null) ?? 0f;
		}
		float num = unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.IsolatedAttacks).Final;
		if (caster is PlayableUnit playableUnit)
		{
			num += playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.IsolatedAttacks, base.SkillAction.PerkDataContainer) / 100f;
		}
		num = unit.UnitStatsController.ClampToBoundaries(num, UnitStatDefinition.E_Stat.IsolatedAttacks);
		float num2 = ((firstEffect == null || isSurroundingTile) ? (num * 0.01f) : (firstEffect.GetDamageMultiplier(firstEffect.GetPerkContext(caster)) * num * 0.01f));
		return num2 - 1f;
	}

	public Vector2Int ComputeMomentumDamageRange(Vector2Int damageRange, ISkillCaster caster)
	{
		return damageRange.GetRangeFromPercentage(ComputeMomentumPercentage(caster));
	}

	public float ComputeMomentumPercentage(ISkillCaster caster)
	{
		MomentumEffectDefinition firstEffect = AttackSkillAction.GetFirstEffect<MomentumEffectDefinition>("Momentum");
		return ComputeMomentumPercentage(caster, firstEffect, base.SkillAction.PerkDataContainer);
	}

	public Vector2Int ComputeOpportunisticDamageRange(Vector2Int damageRange, TheLastStand.Model.Unit.Unit targetUnit, ISkillCaster caster, bool isSurroundingTile)
	{
		return damageRange.GetRangeFromPercentage(ComputeOpportunisticPercentage(targetUnit, caster, isSurroundingTile));
	}

	public float ComputeOpportunisticPercentage(TheLastStand.Model.Unit.Unit targetUnit, ISkillCaster caster, bool isSurroundingTile)
	{
		if ((targetUnit.StatusOwned & TheLastStand.Model.Status.Status.E_StatusType.AllNegative) == 0)
		{
			return 0f;
		}
		OpportunisticSkillEffectDefinition firstEffect = AttackSkillAction.GetFirstEffect<OpportunisticSkillEffectDefinition>("Opportunistic");
		if (!(caster is TheLastStand.Model.Unit.Unit unit))
		{
			return firstEffect?.GetDamageMultiplier(null) ?? 0f;
		}
		float num = unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.OpportunisticAttacks).Final;
		if (caster is PlayableUnit playableUnit)
		{
			num += playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.OpportunisticAttacks, base.SkillAction.PerkDataContainer) / 100f;
		}
		num = unit.UnitStatsController.ClampToBoundaries(num, UnitStatDefinition.E_Stat.OpportunisticAttacks);
		float num2 = ((firstEffect == null || isSurroundingTile) ? (num * 0.01f) : (firstEffect.GetDamageMultiplier(firstEffect.GetPerkContext(caster)) * (num * 0.01f)));
		return num2 - 1f;
	}

	public Vector2Int ComputePerkDamageRange(Vector2Int damageRange, TheLastStand.Model.Unit.Unit targetUnit, PlayableUnit caster)
	{
		Vector2Int rangeFromPercentage = damageRange.GetRangeFromPercentage(caster.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.OverallDamage, base.SkillAction.PerkDataContainer) / 100f);
		int num = (int)caster.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.FlatDamage, base.SkillAction.PerkDataContainer);
		return rangeFromPercentage + new Vector2Int(num, num);
	}

	public override bool CanTargetUnitOverBuilding(Tile targetTile)
	{
		if (targetTile.Unit == null || targetTile.Building == null)
		{
			return false;
		}
		if (targetTile.IsCrossableBy(targetTile.Unit) || targetTile.Building.BlueprintModule.IsIndestructible)
		{
			return true;
		}
		if (targetTile.Unit is PlayableUnit && targetTile.Building.IsWatchtower)
		{
			return false;
		}
		if (targetTile.Unit is EnemyUnit && base.SkillAction.SkillActionExecution.Caster is EnemyUnit)
		{
			return false;
		}
		if (targetTile.Unit is EnemyUnit && (base.SkillAction.SkillActionExecution.Caster is PlayableUnit || base.SkillAction.SkillActionExecution.Caster is BattleModule))
		{
			return true;
		}
		return false;
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		if (targetTile.Building != null && targetTile.CanAffectThroughFog(base.SkillAction.SkillActionExecution.Caster) && !targetTile.Building.BlueprintModule.IsIndestructible && !targetTile.Building.DamageableModule.IsDead && !targetTile.Building.IsTrap)
		{
			if (targetTile.Unit != null)
			{
				return !CanTargetUnitOverBuilding(targetTile);
			}
			return true;
		}
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
		if (unit != null && unit.CanBeDamaged() && targetTile.CanAffectThroughFog(base.SkillAction.SkillActionExecution.Caster) && !targetTile.Unit.IsDead)
		{
			if (targetTile.Building != null)
			{
				return CanTargetUnitOverBuilding(targetTile);
			}
			return true;
		}
		return false;
	}

	public void SplitDamageBetweenArmorAndHealth(IDamageable target, AttackSkillActionExecutionTileData attackData, float armorShreddingBonus = 0f, bool ignoreSkillEffects = false)
	{
		SplitDamageBetweenArmorAndHealth(target, attackData.TotalDamage, out var armorDamage, out var healthDamage, armorShreddingBonus, ignoreSkillEffects);
		attackData.ArmorDamage = armorDamage;
		attackData.HealthDamage = healthDamage;
	}

	public void SplitDamageBetweenArmorAndHealth(IDamageable target, float attackDamage, out float armorDamage, out float healthDamage, float armorShreddingBonus = 0f, bool ignoreSkillEffects = false)
	{
		if (ignoreSkillEffects || !base.SkillAction.HasEffect("ArmorPiercing"))
		{
			ArmorShreddingEffectDefinition firstEffect = base.SkillAction.GetFirstEffect<ArmorShreddingEffectDefinition>("ArmorShredding");
			float num = 1f;
			if (!ignoreSkillEffects && firstEffect != null)
			{
				num += armorShreddingBonus;
				num += firstEffect.BonusDamage;
			}
			if (base.SkillAction is AttackSkillAction { AttackType: AttackSkillActionDefinition.E_AttackType.Physical })
			{
				num += SkillDatabase.DamageTypeModifiersDefinition.MeleeArmorShreddingBonus - 1f;
			}
			armorDamage = Mathf.Min(attackDamage * num, target.Armor);
			healthDamage = Mathf.Min(attackDamage - Mathf.Ceil(armorDamage / num), target.Health);
		}
		else
		{
			armorDamage = 0f;
			healthDamage = Mathf.Min(attackDamage, target.Health);
		}
	}

	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas skillActionResultDatas = new SkillActionResultDatas();
		bool flag = IsUnitAffected(targetTile);
		bool flag2 = IsBuildingAffected(targetTile);
		PropagationSkillEffectDefinition effect;
		bool flag3 = AttackSkillAction.TryGetFirstEffect<PropagationSkillEffectDefinition>("Propagation", out effect);
		float rangeForcedRandom = -1f;
		AttackSkillActionExecutionTileData attackSkillActionExecutionTileData;
		if (flag3)
		{
			rangeForcedRandom = RandomManager.GetRandomRange(TPSingleton<SkillManager>.Instance, 0f, 1f);
			attackSkillActionExecutionTileData = ComputeAttackData(targetTile, caster, flag, flag2, isSurroundingTile: false, rangeForcedRandom);
		}
		else
		{
			attackSkillActionExecutionTileData = ComputeAttackData(targetTile, caster, flag, flag2);
		}
		if (flag)
		{
			skillActionResultDatas.AddAffectedUnit(targetTile.Unit);
		}
		else if (flag2)
		{
			skillActionResultDatas.AddAffectedBuilding(targetTile.Building);
		}
		if (flag)
		{
			TheLastStand.Model.Unit.Unit unit = targetTile.Unit;
			if (unit is PlayableUnit playableUnit && attackSkillActionExecutionTileData.Dodged)
			{
				playableUnit.LifetimeStats.LifetimeStatsController.IncreaseDodges();
			}
			if (caster is PlayableUnit playableUnit2 && !attackSkillActionExecutionTileData.Dodged && attackSkillActionExecutionTileData.IsCrit)
			{
				playableUnit2.LifetimeStats.LifetimeStatsController.IncreaseCriticalHits();
			}
			float num = AttackTarget(unit, attackSkillActionExecutionTileData, caster, skillActionResultDatas, ignoreSkillEffects: false, applyStatuses: true);
			skillActionResultDatas.TotalDamagesToUnits += attackSkillActionExecutionTileData.TotalDamage;
			if (flag3)
			{
				Tile tile = targetTile;
				List<TheLastStand.Model.Unit.Unit> list = new List<TheLastStand.Model.Unit.Unit> { unit };
				Dictionary<TheLastStand.Model.Unit.Unit, float> dictionary = new Dictionary<TheLastStand.Model.Unit.Unit, float>();
				List<Tuple<TheLastStand.Model.Unit.Unit, AttackSkillActionExecutionTileData>> list2 = new List<Tuple<TheLastStand.Model.Unit.Unit, AttackSkillActionExecutionTileData>>();
				int num2 = ((caster is TheLastStand.Model.Unit.Unit unit2) ? unit2.UnitController.GetModifiedPropagationsCount(effect.GetPropagationsCount(effect.GetPerkContext(unit2))) : effect.PropagationsCount);
				int num3 = 0;
				while (num3 < num2)
				{
					List<Tile> enumerable = ((!(base.SkillAction.Skill.Owner is PlayableUnit playableUnit3) || !playableUnit3.AllowDiagonalPropagation(base.SkillAction.PerkDataContainer)) ? tile.GetAdjacentTiles() : tile.GetAdjacentTilesWithDiagonals());
					IEnumerable<Tile> enumerable2 = RandomManager.Shuffle(this, enumerable);
					bool flag4 = false;
					foreach (Tile item in enumerable2)
					{
						if (item.HasFog || item.Unit == null || item.Unit == caster || !(item.Unit.Health > 0f) || list.Contains(item.Unit) || (dictionary.TryGetValue(item.Unit, out var value) && !(value > 0f)))
						{
							continue;
						}
						flag4 = true;
						base.SkillAction.SkillActionExecution.SkillExecutionController.AddPropagationAffectedUnit(base.SkillAction.SkillActionExecution.HitIndex, item.Unit);
						item.Unit.FreezeIsolationState(freeze: true);
						list.Add(item.Unit);
						skillActionResultDatas.AddAffectedUnit(item.Unit);
						tile = item;
						num3++;
						AttackSkillActionExecutionTileData attackSkillActionExecutionTileData2 = ComputePropagatedAttackData(item, caster, rangeForcedRandom, num3, attackSkillActionExecutionTileData.IsCrit);
						list2.Add(new Tuple<TheLastStand.Model.Unit.Unit, AttackSkillActionExecutionTileData>(item.Unit, attackSkillActionExecutionTileData2));
						if (attackSkillActionExecutionTileData2.Dodged)
						{
							break;
						}
						if (value == 0f)
						{
							float num4 = item.Unit.Armor;
							if (AttackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Physical)
							{
								num4 = Mathf.Round(num4 / 2f);
							}
							dictionary[item.Unit] = item.Unit.Health + num4 - attackSkillActionExecutionTileData2.TotalDamage;
						}
						else
						{
							dictionary[item.Unit] = value - attackSkillActionExecutionTileData2.TotalDamage;
						}
						break;
					}
					if (!flag4)
					{
						if (list.Count <= 1)
						{
							break;
						}
						list.Clear();
						list.Add(tile.Unit);
					}
				}
				foreach (Tuple<TheLastStand.Model.Unit.Unit, AttackSkillActionExecutionTileData> item2 in list2)
				{
					if (item2.Item1.CanBeDamaged())
					{
						num += AttackTarget(item2.Item1, item2.Item2, caster, skillActionResultDatas, ignoreSkillEffects: false, applyStatuses: true);
						skillActionResultDatas.TotalDamagesToUnits += item2.Item2.TotalDamage;
					}
				}
			}
			skillActionResultDatas.TotalDamagesToHealth += num;
		}
		if (flag2)
		{
			AttackTarget(targetTile.Building.DamageableModule, attackSkillActionExecutionTileData, caster, skillActionResultDatas);
		}
		return skillActionResultDatas;
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas skillActionResultDatas = new SkillActionResultDatas();
		if (!AttackSkillAction.TryGetFirstEffect<SurroundingEffectDefinition>("SurroundingEffect", out var effect))
		{
			TPSingleton<SkillManager>.Instance.LogError("No surrounding effect found on skill " + base.SkillAction.Skill.SkillDefinition.Id + "!", CLogLevel.DETAILED);
			return skillActionResultDatas;
		}
		bool hitsUnit = IsUnitAffected(targetTile);
		bool hitsBuilding = IsBuildingAffected(targetTile);
		ApplySkillEffectsOnTile(targetTile, caster, skillActionResultDatas, effect.SkillEffectDefinitions, hitsUnit, hitsBuilding);
		return skillActionResultDatas;
	}

	protected override void ApplySkillEffectsOnTile(Tile targetTile, ISkillCaster caster, SkillActionResultDatas resultDatas, List<SkillEffectDefinition> skillEffectDefinitions, bool hitsUnit, bool hitsBuilding, bool forceApply = false)
	{
		AttackSkillActionExecutionTileData attackSkillActionExecutionTileData = ComputeAttackData(targetTile, caster, hitsUnit, hitsBuilding, isSurroundingTile: true, -1f, forceApply);
		if (attackSkillActionExecutionTileData.Dodged)
		{
			if (hitsUnit)
			{
				resultDatas.PotentiallyAffectedDamageables.Add(targetTile.Unit);
				AttackTarget(targetTile.Unit, attackSkillActionExecutionTileData, caster, resultDatas);
			}
			return;
		}
		foreach (SkillEffectDefinition skillEffectDefinition in skillEffectDefinitions)
		{
			if (skillEffectDefinition is DamageSurroundingEffectDefinition)
			{
				if (hitsUnit)
				{
					resultDatas.AddAffectedUnit(targetTile.Unit);
					resultDatas.PotentiallyAffectedDamageables.Add(targetTile.Unit);
					resultDatas.TotalDamagesToHealth += AttackTarget(targetTile.Unit, attackSkillActionExecutionTileData, caster, resultDatas);
					resultDatas.TotalDamagesToUnits += attackSkillActionExecutionTileData.TotalDamage;
				}
				if (hitsBuilding)
				{
					resultDatas.AddAffectedBuilding(targetTile.Building);
					resultDatas.PotentiallyAffectedDamageables.Add(targetTile.Building.DamageableModule);
					resultDatas.TotalDamagesToHealth += AttackTarget(targetTile.Building.DamageableModule, attackSkillActionExecutionTileData, caster, resultDatas);
				}
			}
			else if (hitsUnit)
			{
				resultDatas.PotentiallyAffectedDamageables.Add(targetTile.Unit);
				ApplySkillEffectToUnit(targetTile.Unit, caster, resultDatas, skillEffectDefinition);
			}
		}
	}

	private float AttackTarget(IDamageable target, AttackSkillActionExecutionTileData attackData, ISkillCaster attacker, SkillActionResultDatas resultDatas, bool ignoreSkillEffects = false, bool applyStatuses = false)
	{
		PlayableUnit playableUnit = attacker as PlayableUnit;
		TheLastStand.Model.Unit.Unit unit = target as TheLastStand.Model.Unit.Unit;
		IEntity entity = attacker as IEntity;
		float num = 0f;
		EnsurePerkData(attackData: attackData, targetTile: base.SkillAction.PerkDataContainer?.TargetTile ?? attackData.TargetTile, targetDamageable: target, isTriggeredByPerk: base.SkillAction.Skill.SkillContainer is Perk);
		if (!attackData.Dodged)
		{
			if (attackData.DamageRangeData != null && attackData.DamageRangeData.OpportunismDamageRange.magnitude > 0f && playableUnit != null)
			{
				int num2 = Mathf.FloorToInt(Mathf.Lerp(attackData.DamageRangeData.OpportunismDamageRange.x, attackData.DamageRangeData.OpportunismDamageRange.y, attackData.ComputationRandom));
				TrophyManager.AppendValueToTrophiesConditions<OpportunismDamageInflictedTrophyConditionController>(new object[2] { playableUnit.RandomId, num2 });
				TrophyManager.AppendValueToTrophiesConditions<OpportunisticTriggeredTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
			}
			if (attackData.DamageRangeData != null && attackData.DamageRangeData.ResistanceReductionRange.magnitude > 0f && (float)attackData.DamageRangeData.ResistanceReductionRange.x < 0f && attacker is EnemyUnit && unit is PlayableUnit playableUnit2)
			{
				int num3 = Mathf.Abs(Mathf.FloorToInt(attackData.DamageRangeData.ResistanceReductionRange.Lerp(attackData.ComputationRandom)));
				playableUnit2.LifetimeStats.LifetimeStatsController.IncreaseDamagesMitigatedByResistance(num3);
			}
			if (!(base.SkillAction.Skill.SkillContainer is Perk))
			{
				target.DamageableController.OnHit(attacker);
			}
			SplitDamageBetweenArmorAndHealth(target, attackData, (attacker is TheLastStand.Model.Unit.Unit unit2) ? (unit2.GetClampedStatValue(UnitStatDefinition.E_Stat.ArmorShreddingAttacks) / 100f) : 0f, ignoreSkillEffects);
			unit?.Events.GetValueOrDefault(E_EffectTime.OnAttackDataComputed)?.Invoke(base.SkillAction.PerkDataContainer);
			TPSingleton<AchievementManager>.Instance.HandleOnAttackDataComputed(attackData);
			target.DamageableController.OnAttackDataComputed(base.SkillAction.PerkDataContainer);
			if (attackData.TotalDamage > 0f && target is PlayableUnit playableUnit3 && entity is EnemyUnit enemyUnit)
			{
				playableUnit3.LifetimeStats.LifetimeStatsController.IncreaseDamagesReceivedByEnemy(enemyUnit.Id, attackData.TotalDamage);
			}
			if (attackData.ArmorDamage > 0f)
			{
				target.DamageableController.LoseArmor(attackData.ArmorDamage, attacker, refreshHud: false);
			}
			attackData.TargetRemainingArmor = target.Armor;
			bool flag = attackData.HealthDamage >= target.DamageableController.Damageable.HealthTotal;
			if (attackData.HealthDamage > 0f)
			{
				if (target.DamageableController is DamageableModuleController damageableModuleController)
				{
					damageableModuleController.ChangeCanPrepareForDeath(canPrepareForDeath: false);
				}
				target.DamageableController.LoseHealth(attackData.HealthDamage, attacker, refreshHud: false, base.SkillAction.Skill.SkillDefinition.Id);
				num += attackData.HealthDamage;
				if (entity is EnemyUnit enemyUnit2 && target is EnemyUnit && enemyUnit2.EnemyUnitTemplateDefinition.Id == "Boomer" && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
				{
					TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.BoomerDamage, attackData.HealthDamage);
				}
			}
			attackData.TargetRemainingHealth = target.Health;
			object obj;
			if (!(target is DamageableModule damageableModule))
			{
				obj = target as IBarker;
			}
			else
			{
				IBarker blueprintModule = damageableModule.BuildingParent.BlueprintModule;
				obj = blueprintModule;
			}
			IBarker attackedObject = (IBarker)obj;
			TPSingleton<BarkManager>.Instance.CheckAttack(attackedObject, attackData, attacker);
			if (playableUnit != null)
			{
				if (!(target is EnemyUnit))
				{
					if (!(target is PlayableUnit))
					{
						if (target is DamageableModule damageableModule2 && !AttackSkillAction.HeroHitsBuildings.Contains(damageableModule2.BuildingParent))
						{
							AttackSkillAction.HeroHitsBuildings.Add(damageableModule2.BuildingParent);
						}
					}
					else
					{
						AttackSkillAction.HeroHitsHeroCount++;
						TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_FRIENDLY_FIRE);
					}
				}
				else
				{
					AttackSkillAction.HeroHitsEnemyCount++;
				}
				if (target.DamageableController is UnitController)
				{
					playableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesInflicted(attackData.HealthDamageIncludingOverkill);
					if (target is EnemyUnit enemyUnit3)
					{
						playableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesInflictedToEnemyType(enemyUnit3, attackData.HealthDamageIncludingOverkill);
						if (flag && base.SkillAction.HasEffect("Momentum"))
						{
							TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MomentumOneShots, 1.0);
						}
						if (base.SkillAction.Skill.IsPunch && target.IsDead)
						{
							TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.PunchKills, 1.0);
							TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_PUNCH_KILL);
						}
					}
					TPSingleton<MetaConditionManager>.Instance.RefreshMaxSingleHitDamageByType(attackData.TotalDamage, AttackSkillAction.AttackType);
					TrophyManager.AppendValueToTrophiesConditions<DamageInflictedTrophyConditionController>(new object[2] { playableUnit.RandomId, attackData.HealthDamageIncludingOverkill });
					TrophyManager.SetValueToTrophiesConditions<DamageInflictedSingleAttackTrophyConditionController>(new object[2] { playableUnit.RandomId, attackData.HealthDamageIncludingOverkill });
				}
			}
			AttackFeedback attackFeedback = target.DamageableView.AttackFeedback;
			attackFeedback.AddDamageInstance(attackData);
			target.DamageableController.AddEffectDisplay(attackFeedback);
			playableUnit?.Events.GetValueOrDefault(E_EffectTime.OnTargetHitBeforeSkillEffectsApplied)?.Invoke(base.SkillAction.PerkDataContainer);
			Dictionary<string, List<SkillEffectDefinition>> allEffects = base.SkillAction.GetAllEffects();
			if (applyStatuses && allEffects.Count > 0)
			{
				List<SkillEffectDefinition> list = new List<SkillEffectDefinition>();
				foreach (KeyValuePair<string, List<SkillEffectDefinition>> item in allEffects)
				{
					list.AddRange(item.Value);
				}
				base.ApplySkillEffectsOnTile(attackData.TargetTile, attacker, resultDatas, list, hitsUnit: true, hitsBuilding: false);
			}
			unit?.Events.GetValueOrDefault(E_EffectTime.OnHitTaken)?.Invoke(base.SkillAction.PerkDataContainer);
			playableUnit?.Events.GetValueOrDefault(E_EffectTime.OnTargetHit)?.Invoke(base.SkillAction.PerkDataContainer);
			if (attackData.TargetRemainingHealth <= 0f)
			{
				playableUnit?.Events.GetValueOrDefault(E_EffectTime.OnTargetKilled)?.Invoke(base.SkillAction.PerkDataContainer);
			}
			unit?.UnitController.UpdateInjuryStage(updateHud: false);
		}
		else
		{
			if (target is EnemyUnit enemyUnit4)
			{
				enemyUnit4.EnemyUnitController.EnemyUnit.HasDodged = true;
			}
			AttackFeedback attackFeedback2 = target.DamageableView.AttackFeedback;
			attackFeedback2.AddDamageInstance(attackData);
			target.DamageableController.AddEffectDisplay(attackFeedback2);
			unit?.Events.GetValueOrDefault(E_EffectTime.OnDodge)?.Invoke(base.SkillAction.PerkDataContainer);
			playableUnit?.Events.GetValueOrDefault(E_EffectTime.OnTargetDodge)?.Invoke(base.SkillAction.PerkDataContainer);
		}
		if (playableUnit != null)
		{
			if (ComputeMomentumPercentage(playableUnit) >= 4f)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_MOMENTUM_REACH_CAP);
			}
			if (target is EnemyUnit enemyUnit5 && !attackData.Dodged)
			{
				if (enemyUnit5.IsIsolated && base.SkillAction.HasEffect("Isolated") && attackData.TargetRemainingHealth <= 0f)
				{
					TrophyManager.AppendValueToTrophiesConditions<EnemiesKilledByIsolatedTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
				}
				if (attackData.ArmorDamage >= 0f && base.SkillAction.HasEffect("ArmorShredding"))
				{
					TrophyManager.AppendValueToTrophiesConditions<ArmoredEnemiesDamagedByArmorShreddingTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
				}
			}
		}
		entity.Log($"Dealt {attackData.TotalDamage} damage to {(attackData.Damageable as IEntity)?.UniqueIdentifier}.");
		entity.Log($"DETAILS:\n{attackData}");
		return num;
	}

	private AttackSkillActionExecutionTileData ComputeAttackData(Tile targetTile, ISkillCaster caster, bool hitsUnit, bool hitsBuilding, bool isSurroundingTile = false, float rangeForcedRandom = -1f, bool forceApply = false, List<SkillEffectDefinition> skillEffectDefinitions = null)
	{
		AttackSkillActionExecutionTileData obj = new AttackSkillActionExecutionTileData
		{
			TargetTile = targetTile,
			IsPropagatedTile = false
		};
		object damageable;
		if (!hitsUnit)
		{
			if (!hitsBuilding)
			{
				damageable = null;
			}
			else
			{
				IDamageable damageableModule = targetTile.Building.DamageableModule;
				damageable = damageableModule;
			}
		}
		else
		{
			IDamageable damageableModule = targetTile.Unit;
			damageable = damageableModule;
		}
		obj.Damageable = (IDamageable)damageable;
		AttackSkillActionExecutionTileData tileAttackData = obj;
		ComputeHealthAndArmorData(targetTile, hitsUnit, hitsBuilding, ref tileAttackData);
		SurroundingEffectDefinition effect;
		bool flag = !isSurroundingTile || (AttackSkillAction.TryGetFirstEffect<SurroundingEffectDefinition>("SurroundingEffect", out effect) && effect.HasEffect<DamageSurroundingEffectDefinition>());
		if (!forceApply && flag)
		{
			ComputeDodgeData(targetTile, caster, ref tileAttackData);
		}
		if (!tileAttackData.Dodged && flag)
		{
			ComputeDamageData(targetTile, caster, hitsUnit, ref tileAttackData, isSurroundingTile, rangeForcedRandom);
		}
		else if (targetTile.Unit is PlayableUnit playableUnit)
		{
			TrophyManager.AppendValueToTrophiesConditions<DodgesPerformedTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
		}
		return tileAttackData;
	}

	private AttackSkillActionExecutionTileData ComputePropagatedAttackData(Tile targetTile, ISkillCaster caster, float rangeForcedRandom, int propagationIndex, bool isCrit)
	{
		AttackSkillActionExecutionTileData tileAttackData = new AttackSkillActionExecutionTileData
		{
			TargetTile = targetTile,
			IsPropagatedTile = true,
			Damageable = targetTile.Unit
		};
		ComputeHealthAndArmorData(targetTile, hitsUnit: true, hitsBuilding: false, ref tileAttackData);
		ComputeDodgeData(targetTile, caster, ref tileAttackData);
		if (!tileAttackData.Dodged)
		{
			ComputePropagatedDamageData(targetTile, caster, rangeForcedRandom, isCrit, propagationIndex, ref tileAttackData);
		}
		else if (targetTile.Unit is PlayableUnit playableUnit)
		{
			TrophyManager.AppendValueToTrophiesConditions<DodgesPerformedTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
		}
		return tileAttackData;
	}

	private void ComputeDamageData(Tile targetTile, ISkillCaster caster, bool hitsUnit, ref AttackSkillActionExecutionTileData tileAttackData, bool isSurroundingTile = false, float rangeForcedRandom = -1f)
	{
		EnsurePerkData(attackData: tileAttackData, targetTile: base.SkillAction.PerkDataContainer?.TargetTile ?? targetTile, targetDamageable: hitsUnit ? targetTile.Unit : null, isTriggeredByPerk: base.SkillAction.Skill.SkillContainer is Perk);
		tileAttackData.DamageRangeData = ComputeFinalDamageRange(targetTile, caster, isSurroundingTile);
		tileAttackData.ComputationRandom = ((rangeForcedRandom >= 0f) ? Mathf.Min(rangeForcedRandom, 1f) : RandomManager.GetRandomRange(this, 0f, 1f));
		tileAttackData.TotalDamage = Mathf.Clamp(Mathf.Round((float)tileAttackData.DamageRangeData.FinalDamageRange.x + (float)(tileAttackData.DamageRangeData.FinalDamageRange.y - tileAttackData.DamageRangeData.FinalDamageRange.x) * tileAttackData.ComputationRandom), tileAttackData.DamageRangeData.FinalDamageRange.x, tileAttackData.DamageRangeData.FinalDamageRange.y);
		ComputeBlockedDamageData(ref tileAttackData);
		IncreaseTargetTileUnitLifetimeBlockedDamagesStat(targetTile, Mathf.FloorToInt(tileAttackData.BlockedDamage));
		if (caster is PlayableUnit playableUnit)
		{
			float perkModifierForComputationStat = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.Critical, base.SkillAction.PerkDataContainer);
			tileAttackData.IsCrit = (float)RandomManager.GetRandomRange(this, 0, 100) < playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Critical) + (float)Mathf.FloorToInt(AttackSkillAction.AttackSkillActionDefinition.CriticProbability) + perkModifierForComputationStat;
			if (tileAttackData.IsCrit)
			{
				float perkModifierForComputationStat2 = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.CriticalPower, base.SkillAction.PerkDataContainer);
				tileAttackData.TotalDamage *= (playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.CriticalPower) + perkModifierForComputationStat2) / 100f;
				TrophyManager.AppendValueToTrophiesConditions<CriticalsInflictedSingleTurnTrophyConditionController>(new object[2] { playableUnit.RandomId, 1 });
			}
		}
		else if (caster is EnemyUnit enemyUnit)
		{
			tileAttackData.IsCrit = (float)RandomManager.GetRandomRange(this, 0, 100) < enemyUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Critical) + (float)Mathf.FloorToInt(AttackSkillAction.AttackSkillActionDefinition.CriticProbability);
			if (tileAttackData.IsCrit)
			{
				tileAttackData.TotalDamage *= enemyUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.CriticalPower) / 100f;
			}
		}
		tileAttackData.TotalDamage = Mathf.Round(tileAttackData.TotalDamage);
	}

	private void ComputeDodgeData(Tile targetTile, ISkillCaster caster, ref AttackSkillActionExecutionTileData tileAttackData)
	{
		if (!AttackSkillAction.HasEffect("NoDodge"))
		{
			float num = 1f;
			if (base.SkillAction.SkillActionExecution.SkillSourceTile != null && AttackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Ranged && base.SkillAction.SkillActionExecution.InRangeTiles.IsInRange(targetTile))
			{
				int num2 = TileMapController.DistanceBetweenTiles(targetTile, base.SkillAction.SkillActionExecution.SkillSourceTile);
				foreach (KeyValuePair<int, float> item in SkillDatabase.DamageTypeModifiersDefinition.DodgeMultiplierByDistance)
				{
					if (num2 < item.Key)
					{
						break;
					}
					num = item.Value;
				}
			}
			if (base.SkillAction.TryGetFirstEffect<InaccurateSkillEffectDefinition>("Inaccurate", out var effect))
			{
				num *= 1f + effect.GetMalus(effect.GetPerkContext(caster));
			}
			float num3 = 0f;
			if (caster is TheLastStand.Model.Unit.Unit unit && !(tileAttackData.Damageable is TheLastStand.Model.Unit.Unit { IsStunned: not false }))
			{
				num3 += unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Accuracy);
				if (caster is PlayableUnit playableUnit)
				{
					num3 += playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.Accuracy, base.SkillAction.PerkDataContainer);
				}
			}
			int randomRange = RandomManager.GetRandomRange(TPSingleton<SkillManager>.Instance, 0, 100);
			float num4 = ((tileAttackData.Damageable is TheLastStand.Model.Unit.Unit unit3) ? (unit3.GetClampedStatValue(UnitStatDefinition.E_Stat.Dodge) * num - num3) : (0f - num3));
			tileAttackData.Dodged = (float)randomRange < num4;
		}
		else if (tileAttackData.Damageable is EnemyUnit enemyUnit && enemyUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Dodge) > 0f && caster is PlayableUnit playableUnit2)
		{
			TrophyManager.AppendValueToTrophiesConditions<NoDodgeTriggeredTrophyConditionController>(new object[2] { playableUnit2.RandomId, 1 });
		}
	}

	private void ComputeHealthAndArmorData(Tile targetTile, bool hitsUnit, bool hitsBuilding, ref AttackSkillActionExecutionTileData tileAttackData)
	{
		if (hitsUnit)
		{
			tileAttackData.TargetHealthTotal = targetTile.Unit.HealthTotal;
			tileAttackData.TargetArmorTotal = targetTile.Unit.ArmorTotal;
		}
		else if (hitsBuilding)
		{
			tileAttackData.TargetHealthTotal = targetTile.Building.DamageableModule.HealthTotal;
			tileAttackData.TargetArmorTotal = targetTile.Building.DamageableModule.ArmorTotal;
		}
	}

	private void ComputePropagatedDamageData(Tile targetTile, ISkillCaster caster, float rangeForcedRandom, bool isCrit, int propagationIndex, ref AttackSkillActionExecutionTileData tileAttackData)
	{
		tileAttackData.DamageRangeData = ComputeFinalDamageRange(targetTile, caster);
		tileAttackData.ComputationRandom = rangeForcedRandom;
		tileAttackData.TotalDamage = Mathf.Clamp(Mathf.Round((float)tileAttackData.DamageRangeData.FinalDamageRange.x + (float)(tileAttackData.DamageRangeData.FinalDamageRange.y - tileAttackData.DamageRangeData.FinalDamageRange.x) * rangeForcedRandom), tileAttackData.DamageRangeData.FinalDamageRange.x, tileAttackData.DamageRangeData.FinalDamageRange.y);
		ComputeBlockedDamageData(ref tileAttackData);
		IncreaseTargetTileUnitLifetimeBlockedDamagesStat(targetTile, Mathf.FloorToInt(tileAttackData.BlockedDamage));
		if (isCrit && caster is TheLastStand.Model.Unit.Unit unit)
		{
			float num = 0f;
			if (caster is PlayableUnit playableUnit)
			{
				num = playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.CriticalPower, base.SkillAction.PerkDataContainer);
			}
			tileAttackData.TotalDamage *= (unit.GetClampedStatValue(UnitStatDefinition.E_Stat.CriticalPower) + num) / 100f;
		}
		tileAttackData.TotalDamage *= Mathf.Pow((caster is TheLastStand.Model.Unit.Unit unit2) ? (unit2.GetClampedStatValue(UnitStatDefinition.E_Stat.PropagationDamage) * 0.01f) : 1f, propagationIndex);
		tileAttackData.TotalDamage = Mathf.Round(tileAttackData.TotalDamage);
	}

	private void ComputeBlockedDamageData(ref AttackSkillActionExecutionTileData tileAttackData)
	{
		if (tileAttackData.TotalDamage > 0f)
		{
			tileAttackData.BlockedDamage = Mathf.Abs(tileAttackData.DamageRangeData.BlockReductionRange.x);
			return;
		}
		Vector2Int blockableDamageRange = tileAttackData.DamageRangeData.BlockableDamageRange;
		tileAttackData.BlockedDamage = Mathf.Clamp(Mathf.Round((float)blockableDamageRange.x + (float)(blockableDamageRange.y - blockableDamageRange.x) * tileAttackData.ComputationRandom), blockableDamageRange.x, blockableDamageRange.y);
	}

	private void IncreaseTargetTileUnitLifetimeBlockedDamagesStat(Tile targetTile, int blockedDamage)
	{
		if (targetTile != null && targetTile.Unit != null && targetTile.Unit is PlayableUnit playableUnit)
		{
			playableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesBlocked(blockedDamage);
		}
	}
}
