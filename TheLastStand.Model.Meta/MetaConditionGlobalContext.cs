using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Meta;

public class MetaConditionGlobalContext : MetaConditionContext
{
	private List<PropertyInfo> lifetimeStatsProperties;

	public double MajorVersion => ApplicationManager.MajorVersion;

	public double MinorVersion => ApplicationManager.MinorVersion;

	public double PatchVersion => ApplicationManager.PatchVersion;

	public double HotfixVersion => ApplicationManager.HotfixVersion;

	public MetaConditionGlobalContext()
	{
		lifetimeStatsProperties = typeof(LifetimeStats).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
	}

	public double GetMaxLifetimeStatValue(string statId)
	{
		return GetLifetimeStats(statId).max;
	}

	public double GetMinLifetimeStatValue(string statId)
	{
		return GetLifetimeStats(statId).min;
	}

	public double HasActivatedMetaUpgrade(string upgradeId)
	{
		return TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Count((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == upgradeId);
	}

	public double HasAtLeastFulfilledMetaUpgrade(string upgradeId)
	{
		return (double)TPSingleton<MetaUpgradesManager>.Instance.FulfilledUpgrades.Count((MetaUpgrade o) => o.MetaUpgradeDefinition.Id == upgradeId) + HasActivatedMetaUpgrade(upgradeId);
	}

	public double VanillaNotActivatedMetaUpgradesOtherThan(string excludedUpgradeId)
	{
		int num = TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Count((MetaUpgrade o) => !o.MetaUpgradeDefinition.IsLinkedToDLC && o.MetaUpgradeDefinition.Id != excludedUpgradeId);
		int num2 = TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Count((MetaUpgrade o) => !o.MetaUpgradeDefinition.IsLinkedToDLC && o.MetaUpgradeDefinition.Id != excludedUpgradeId);
		return num + num2;
	}

	public double OwnedDLC(string dlcId)
	{
		return TPSingleton<DLCManager>.Instance.IsDLCOwned(dlcId) ? 1 : 0;
	}

	private (double max, double min) GetLifetimeStats(string statId)
	{
		PropertyInfo propertyInfo = lifetimeStatsProperties.Find((PropertyInfo o) => o.Name == statId);
		double num = double.NegativeInfinity;
		double num2 = double.PositiveInfinity;
		if (propertyInfo != null)
		{
			if (!MetaConditionContext.SupportedNumberTypes.Contains(propertyInfo.PropertyType))
			{
				throw new Exception("The lifetime stat " + statId + " is not a numeric type, and therefore is not supported");
			}
			if (!TPSingleton<PlayableUnitManager>.Exist() || TPSingleton<PlayableUnitManager>.Instance.PlayableUnits == null)
			{
				return (max: 0.0, min: 0.0);
			}
			foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				num = Math.Max(num, Convert.ToDouble(propertyInfo.GetValue(playableUnit.LifetimeStats)));
				num2 = Math.Min(num2, Convert.ToDouble(propertyInfo.GetValue(playableUnit.LifetimeStats)));
			}
			return (max: num, min: num2);
		}
		throw new Exception("Unkown lifetime stat " + statId + ". Known stats are:" + string.Join(", ", lifetimeStatsProperties.Select((PropertyInfo o) => o.Name)));
	}
}
