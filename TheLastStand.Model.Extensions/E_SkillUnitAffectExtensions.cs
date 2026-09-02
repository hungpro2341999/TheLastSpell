using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Unit.Enemy.Affix;
using TheLastStand.Database;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Model.Extensions;

public static class E_SkillUnitAffectExtensions
{
	private class ModifyAffectedUnits
	{
		public AffectingUnitSkillEffectDefinition.E_SkillUnitAffect AffectedUnitsToAdd { get; private set; }

		public AffectingUnitSkillEffectDefinition.E_SkillUnitAffect AffectedUnitsToRemove { get; private set; }

		public List<string> IdsListsToExclude { get; } = new List<string>();

		public List<string> IdsListsToInclude { get; } = new List<string>();

		public void AddAffectedUnitToAdd(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnit)
		{
			AffectedUnitsToAdd |= affectedUnit;
		}

		public void AddAffectedUnitToRemove(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnit)
		{
			AffectedUnitsToRemove |= affectedUnit;
		}

		public void AddIdsListToExclude(string idsList)
		{
			if (!IdsListsToExclude.Contains(idsList))
			{
				IdsListsToExclude.Add(idsList);
			}
		}

		public void AddIdsListToInclude(string idsList)
		{
			if (!IdsListsToInclude.Contains(idsList))
			{
				IdsListsToInclude.Add(idsList);
			}
		}

		public void AddModifyAffectedUnitsEffectDefinition(ModifyAffectedUnitsEffectDefinition effectDefinition)
		{
			if (effectDefinition.AffectedUnitsToAdd != AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.None)
			{
				AddAffectedUnitToAdd(effectDefinition.AffectedUnitsToAdd);
			}
			if (effectDefinition.AffectedUnitsToRemove != AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.None)
			{
				AddAffectedUnitToRemove(effectDefinition.AffectedUnitsToRemove);
			}
			foreach (string item in effectDefinition.IdsListsToInclude)
			{
				AddIdsListToInclude(item);
			}
			foreach (string item2 in effectDefinition.IdsListsToExclude)
			{
				AddIdsListToExclude(item2);
			}
		}

		public void Reset()
		{
			AffectedUnitsToAdd = AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.None;
			AffectedUnitsToRemove = AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.None;
			IdsListsToInclude.Clear();
			IdsListsToExclude.Clear();
		}
	}

	private static ModifyAffectedUnits modifyAffectedUnits = new ModifyAffectedUnits();

	public static bool AffectsUnitType(this AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect unitType)
	{
		return affectedUnits.HasFlag(unitType);
	}

	public static void Deserialize(this ref AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnits, XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("CanAffectPlayableUnits");
		if (xElement != null)
		{
			if (bool.TryParse(xElement.Value, out var result))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit;
				if (result)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit;
				}
			}
			else
			{
				CLoggerManager.Log("Could not parse CanAffectPlayableUnits into a bool : " + xElement.Value, TPSingleton<SkillManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SkillManager");
			}
		}
		XElement xElement2 = obj.Element("CanAffectEnemyUnits");
		if (xElement2 != null)
		{
			if (bool.TryParse(xElement2.Value, out var result2))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit;
				if (result2)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit;
				}
			}
			else
			{
				CLoggerManager.Log("Could not parse CanAffectEnemyUnits into a bool : " + xElement2.Value, TPSingleton<SkillManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SkillManager");
			}
		}
		XElement xElement3 = obj.Element("CanAffectBossUnits");
		if (xElement3 != null)
		{
			if (bool.TryParse(xElement3.Value, out var result3))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				if (result3)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				}
			}
			else
			{
				CLoggerManager.Log("Could not parse CanAffectBossUnits into a bool : " + xElement3.Value, TPSingleton<SkillManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SkillManager");
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				if (affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit))
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
				}
			}
		}
		else
		{
			affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
			if (affectedUnits.AffectsUnitType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit))
			{
				affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit;
			}
		}
		XElement xElement4 = obj.Element("CanAffectCaster");
		if (xElement4 != null)
		{
			if (bool.TryParse(xElement4.Value, out var result4))
			{
				affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster;
				if (result4)
				{
					affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster;
				}
			}
			else
			{
				CLoggerManager.Log("Could not parse CanAffectCaster into a bool : " + xElement4.Value, TPSingleton<SkillManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SkillManager");
			}
		}
		XElement xElement5 = obj.Element("CanAffectBuildings");
		if (xElement5 == null)
		{
			return;
		}
		if (bool.TryParse(xElement5.Value, out var result5))
		{
			affectedUnits &= ~AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Building;
			if (result5)
			{
				affectedUnits |= AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Building;
			}
		}
		else
		{
			CLoggerManager.Log("Could not parse CanAffectBuildings into a bool : " + xElement5.Value, TPSingleton<SkillManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SkillManager");
		}
	}

	public static bool ShouldDamageableBeAffected(this AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnits, ISkillCaster caster, IDamageable damageable, TheLastStand.Model.Skill.Skill skill = null)
	{
		if (damageable is EnemyUnit enemyUnit)
		{
			EnemyHigherPlaneAffixController.TargetingValidity targetingValidity = new EnemyHigherPlaneAffixController.TargetingValidity(caster, newValidity: true);
			enemyUnit.EnemyUnitController.TriggerAffixes(E_EffectTime.OnTargetingComputation, targetingValidity);
			if (!targetingValidity.validity)
			{
				return false;
			}
		}
		modifyAffectedUnits.Reset();
		bool checkModifiedAffectedUnits = false;
		if (skill != null && skill.SkillAction.TryGetEffects("ModifyAffectedUnits", out List<ModifyAffectedUnitsEffectDefinition> effects, onlyNative: false))
		{
			foreach (ModifyAffectedUnitsEffectDefinition item in effects)
			{
				modifyAffectedUnits.AddModifyAffectedUnitsEffectDefinition(item);
				checkModifiedAffectedUnits = true;
			}
		}
		DamageableModule damageableModule;
		if (damageable is ISkillCaster skillCaster)
		{
			if (skillCaster == caster)
			{
				return ShouldUnitBeAffected(skillCaster.Id, affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster, checkModifiedAffectedUnits);
			}
			damageableModule = damageable as DamageableModule;
			if (damageableModule != null)
			{
				goto IL_0106;
			}
			if (damageable is PlayableUnit playableUnit)
			{
				return ShouldUnitBeAffected(playableUnit.Id, affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.PlayableUnit, checkModifiedAffectedUnits);
			}
			if (damageable is BossUnit bossUnit)
			{
				return ShouldUnitBeAffected(bossUnit.Id, affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.BossUnit, checkModifiedAffectedUnits);
			}
			if (damageable is EnemyUnit enemyUnit2)
			{
				return ShouldUnitBeAffected(enemyUnit2.Id, affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.EnemyUnit, checkModifiedAffectedUnits);
			}
		}
		else
		{
			damageableModule = damageable as DamageableModule;
			if (damageableModule != null)
			{
				goto IL_0106;
			}
			if (damageable == null)
			{
				return false;
			}
		}
		return true;
		IL_0106:
		if (caster != null && caster == damageableModule.BuildingParent?.BattleModule)
		{
			return ShouldUnitBeAffected(damageableModule.BuildingParent?.Id, affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Caster, checkModifiedAffectedUnits);
		}
		return ShouldUnitBeAffected(damageableModule.BuildingParent?.Id, affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.Building, checkModifiedAffectedUnits);
	}

	private static bool ShouldUnitBeAffected(string unitId, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect affectedUnits, AffectingUnitSkillEffectDefinition.E_SkillUnitAffect currentUnit, bool checkModifiedAffectedUnits)
	{
		if (!checkModifiedAffectedUnits)
		{
			return affectedUnits.AffectsUnitType(currentUnit);
		}
		foreach (string item in modifyAffectedUnits.IdsListsToExclude)
		{
			if (GenericDatabase.IdsListDefinitions.TryGetValue(item, out var value) && value.Ids.Contains(unitId))
			{
				return false;
			}
		}
		foreach (string item2 in modifyAffectedUnits.IdsListsToInclude)
		{
			if (GenericDatabase.IdsListDefinitions.TryGetValue(item2, out var value2) && value2.Ids.Contains(unitId))
			{
				return true;
			}
		}
		if (modifyAffectedUnits.AffectedUnitsToAdd != AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.None)
		{
			affectedUnits |= modifyAffectedUnits.AffectedUnitsToAdd;
		}
		if (modifyAffectedUnits.AffectedUnitsToRemove != AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.None)
		{
			affectedUnits &= ~modifyAffectedUnits.AffectedUnitsToRemove;
		}
		return affectedUnits.AffectsUnitType(currentUnit);
	}
}
