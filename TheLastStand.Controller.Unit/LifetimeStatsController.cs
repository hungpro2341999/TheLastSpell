using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Controller.Unit;

public class LifetimeStatsController
{
	public LifetimeStats LifetimeStats { get; }

	public LifetimeStatsController(SerializedLifetimeStats container)
	{
		LifetimeStats = new LifetimeStats(container, this);
	}

	public LifetimeStatsController()
	{
		LifetimeStats = new LifetimeStats(this);
	}

	public void DecreaseTilesCrossed(int amount)
	{
		LifetimeStats.TilesCrossed -= amount;
		TPSingleton<MetaConditionManager>.Instance.DecreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.TilesCrossed, amount);
	}

	public void IncreaseCriticalHits()
	{
		LifetimeStats.CriticalHits++;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.CriticalHits, 1.0);
	}

	public void IncreaseDamagesBlocked(int damagesBlocked)
	{
		LifetimeStats.DamagesBlocked += damagesBlocked;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.DamageBlocked, damagesBlocked);
	}

	public void IncreaseDamagesInflicted(float damagesInflicted)
	{
		LifetimeStats.DamagesInflicted += damagesInflicted;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.DamageInflicted, damagesInflicted);
	}

	public void IncreaseDamagesInflictedToEnemyType(EnemyUnit enemyUnit, float damagesInflicted)
	{
		if (damagesInflicted <= 0f)
		{
			return;
		}
		TierDefinition valueOrDefault = EnemyUnitDatabase.TierDefinitions.GetValueOrDefault(enemyUnit.EnemyUnitTemplateDefinition.Tier);
		if (valueOrDefault == null)
		{
			TPSingleton<EnemyUnitManager>.Instance.LogError($"Tried to IncreaseDamagesInflictedToEnemyType with enemy {enemyUnit.Id}, but its tier ({enemyUnit.EnemyUnitTemplateDefinition.Tier}) isn't defined in EnemyUnitDatabase.TierDefinitions, Skipping.");
			return;
		}
		float lifetimeStatsWeight = valueOrDefault.LifetimeStatsWeight;
		float num = damagesInflicted / enemyUnit.HealthTotal * lifetimeStatsWeight;
		if (!LifetimeStats.DamagesInflictedToEnemies.ContainsKey(enemyUnit.EnemyUnitTemplateDefinition.Id))
		{
			LifetimeStats.DamagesInflictedToEnemies.Add(enemyUnit.EnemyUnitTemplateDefinition.Id, num);
		}
		else
		{
			LifetimeStats.DamagesInflictedToEnemies[enemyUnit.EnemyUnitTemplateDefinition.Id] += num;
		}
		TPSingleton<MetaConditionManager>.Instance.IncreaseDamageInflictedToEnemyType(enemyUnit.EnemyUnitTemplateDefinition.Id, damagesInflicted);
	}

	public void IncreaseDamagesMitigatedByResistance(float damagesMitigated)
	{
		LifetimeStats.DamagesMitigatedByResistance += damagesMitigated;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.DamageMitigatedByResistance, damagesMitigated);
	}

	public void IncreaseDamagesReceivedByEnemy(string enemyId, float damagesReceived)
	{
		if (!(damagesReceived <= 0f))
		{
			if (!LifetimeStats.DamagesTakenByEnemyType.ContainsKey(enemyId))
			{
				LifetimeStats.DamagesTakenByEnemyType.Add(enemyId, damagesReceived);
			}
			else
			{
				LifetimeStats.DamagesTakenByEnemyType[enemyId] += damagesReceived;
			}
			TPSingleton<MetaConditionManager>.Instance.IncreaseDamageTakenByEnemyType(enemyId, damagesReceived);
		}
	}

	public void IncreaseDamagesTakenOnArmor(float damagesOnArmor)
	{
		LifetimeStats.DamagesTakenOnArmor += damagesOnArmor;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.DamageTakenOnArmor, damagesOnArmor);
	}

	public void IncreaseDodges()
	{
		LifetimeStats.Dodges++;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.Dodges, 1.0);
	}

	public void IncreaseHealthLost(float healthLost)
	{
		LifetimeStats.HealthLost += healthLost;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.HealthLost, healthLost);
	}

	public void IncreaseJumpsOverWallUsed()
	{
		LifetimeStats.JumpsOverWallUsed++;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.JumpsOverWallUsed, 1.0);
	}

	public void IncreaseKills(int kills = 1, bool isIsolated = false)
	{
		LifetimeStats.Kills += kills;
		if (isIsolated)
		{
			LifetimeStats.IsolatedKills += kills;
			if (LifetimeStats.IsolatedKills >= 200)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_KILL_200_ISOLATED_ENEMIES_RUN);
			}
		}
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.Kills, kills);
	}

	public void IncreaseManaSpent(int manaSpent)
	{
		if (manaSpent != 0)
		{
			LifetimeStats.ManaSpent += manaSpent;
			TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.ManaSpent, manaSpent);
			if ((int)TPSingleton<MetaConditionManager>.Instance.RunContext.GetDouble(MetaConditionSpecificContext.E_ValueCategory.ManaSpent) >= 500)
			{
				TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_SPEND_500_MANA_RUN);
			}
		}
	}

	public void IncreasePunchesUsed()
	{
		LifetimeStats.PunchesUsed++;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.PunchesUsed, 1.0);
	}

	public void IncreaseSkillUses(string weaponId, int actionPointsCost)
	{
		if (!LifetimeStats.UsesPerWeapon.ContainsKey(weaponId))
		{
			LifetimeStats.UsesPerWeapon.Add(weaponId, 1);
		}
		else
		{
			LifetimeStats.UsesPerWeapon[weaponId]++;
		}
		TPSingleton<MetaConditionManager>.Instance.IncreaseUsesPerWeapon(weaponId, actionPointsCost);
	}

	public void IncreaseStunnedEnemies(int stunnedEnemies = 1)
	{
		LifetimeStats.StunnedEnemies += stunnedEnemies;
	}

	public void IncreaseTilesCrossed(int tilesCrossed)
	{
		LifetimeStats.TilesCrossed += tilesCrossed;
		TPSingleton<MetaConditionManager>.Instance.IncreaseDoubleValue(MetaConditionSpecificContext.E_ValueCategory.TilesCrossed, tilesCrossed);
	}

	public bool TryGetBestFiend(out string bestFiendId)
	{
		bestFiendId = "None";
		if (LifetimeStats.DamagesInflictedToEnemies.Count == 0)
		{
			return false;
		}
		float num = 0f;
		foreach (KeyValuePair<string, float> damagesInflictedToEnemy in LifetimeStats.DamagesInflictedToEnemies)
		{
			if (damagesInflictedToEnemy.Value > num)
			{
				bestFiendId = damagesInflictedToEnemy.Key;
				num = damagesInflictedToEnemy.Value;
			}
		}
		return true;
	}

	public bool TryGetBestFiend(out EnemyUnitTemplateDefinition bestFiendTemplateDefinition)
	{
		if (TryGetBestFiend(out string bestFiendId))
		{
			if (!EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(bestFiendId, out bestFiendTemplateDefinition) && BossUnitDatabase.BossUnitTemplateDefinitions.TryGetValue(bestFiendId, out var value))
			{
				bestFiendTemplateDefinition = value;
			}
			return true;
		}
		bestFiendTemplateDefinition = null;
		return false;
	}

	public bool TryGetNemesisId(out string nemesisId)
	{
		nemesisId = "None";
		if (LifetimeStats.DamagesTakenByEnemyType.Count == 0)
		{
			return false;
		}
		float num = 0f;
		foreach (KeyValuePair<string, float> item in LifetimeStats.DamagesTakenByEnemyType)
		{
			if (item.Value > num)
			{
				nemesisId = item.Key;
				num = item.Value;
			}
		}
		return true;
	}

	public bool TryGetNemesisTemplateDefinition(out EnemyUnitTemplateDefinition nemesisTemplateDefinition)
	{
		if (TryGetNemesisId(out var nemesisId))
		{
			if (!EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(nemesisId, out nemesisTemplateDefinition) && BossUnitDatabase.BossUnitTemplateDefinitions.TryGetValue(nemesisId, out var value))
			{
				nemesisTemplateDefinition = value;
			}
			return true;
		}
		nemesisTemplateDefinition = null;
		return false;
	}

	public bool TryGetPreferredWeaponId(out Tuple<string, int> weaponUses)
	{
		weaponUses = null;
		if (LifetimeStats.UsesPerWeapon.Count == 0)
		{
			return false;
		}
		string item = string.Empty;
		int num = 0;
		foreach (KeyValuePair<string, int> item2 in LifetimeStats.UsesPerWeapon)
		{
			if (item2.Value > num)
			{
				item = item2.Key;
				num = item2.Value;
			}
		}
		weaponUses = new Tuple<string, int>(item, num);
		return true;
	}

	public bool TryGetPreferredWeaponItemDefinition(out Tuple<ItemDefinition, int> weaponUses)
	{
		weaponUses = null;
		if (TryGetPreferredWeaponId(out var weaponUses2))
		{
			weaponUses = new Tuple<ItemDefinition, int>(ItemDatabase.ItemDefinitions[weaponUses2.Item1], weaponUses2.Item2);
			return true;
		}
		return false;
	}

	public void TryOverrideBestBlow(float blowValue)
	{
		if (blowValue > LifetimeStats.BestBlow)
		{
			LifetimeStats.BestBlow = blowValue;
		}
		TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.BestBlow, blowValue);
	}

	public void TryOverrideMostUnitsKilledInOneBlow(int killsCount)
	{
		if (killsCount != 0)
		{
			if (killsCount > LifetimeStats.MostUnitsKilledInOneBlow)
			{
				LifetimeStats.MostUnitsKilledInOneBlow = killsCount;
			}
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MostUnitsKilledInOneBlow, killsCount);
		}
	}

	public string DebugGetLifetimeStatsLog(string unitName)
	{
		string text = "#--- Lifetime stats (" + unitName + ") ---#\n";
		text += $"Best blow : {LifetimeStats.BestBlow}\n";
		text += $"Critical Hits : {LifetimeStats.CriticalHits}\n";
		text += $"Damages Blocked : {LifetimeStats.DamagesBlocked}\n";
		text += $"Damages Inflicted : {LifetimeStats.DamagesInflicted}\n";
		text += $"Damages Mitigated By Res : {LifetimeStats.DamagesMitigatedByResistance}\n";
		text += $"Damages Taken On Armor : {LifetimeStats.DamagesTakenOnArmor}\n";
		text += $"Dodges : {LifetimeStats.Dodges}\n";
		text += $"Health Lost : {LifetimeStats.HealthLost}\n";
		text += $"Jumps Over Wall Used : {LifetimeStats.JumpsOverWallUsed}\n";
		text += $"Kills : {LifetimeStats.Kills}\n";
		text += $"Mana Spent : {LifetimeStats.ManaSpent}\n";
		text += $"Most Units Killed In One Blow : {LifetimeStats.MostUnitsKilledInOneBlow}\n";
		text = ((!TryGetNemesisId(out var nemesisId)) ? (text + "Nemesis : None\n") : (text + "Nemesis : " + nemesisId + "\n"));
		text = ((!TryGetPreferredWeaponId(out var weaponUses)) ? (text + "Preferred weapon : None\n") : (text + $"Preferred weapon : {weaponUses.Item1} ({weaponUses.Item2} uses)\n"));
		text += $"Punches Used : {LifetimeStats.PunchesUsed}\n";
		text += $"Stunned Enemies : {LifetimeStats.StunnedEnemies}\n";
		text += $"Tiles Crossed: {LifetimeStats.TilesCrossed}\n";
		return text + "#-------------------------#";
	}
}
