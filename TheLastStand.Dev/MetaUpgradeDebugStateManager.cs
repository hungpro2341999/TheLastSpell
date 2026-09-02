using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using UnityEngine;

namespace TheLastStand.Dev;

public class MetaUpgradeDebugStateManager : MonoBehaviour
{
	public static void ActivateUpgrade(MetaUpgrade metaUpgrade)
	{
		MetaUpgrade upgrade;
		MetaUpgradesManager.E_MetaState e_MetaState = MetaUpgradesManager.TryGetUpgradeState(metaUpgrade.MetaUpgradeDefinition.Id, out upgrade);
		if (e_MetaState == MetaUpgradesManager.E_MetaState.NA || e_MetaState == MetaUpgradesManager.E_MetaState.Activated)
		{
			if (e_MetaState == MetaUpgradesManager.E_MetaState.Activated)
			{
				Debug.Log(metaUpgrade.MetaUpgradeDefinition.Id + " upgrade is already activated.");
			}
			return;
		}
		Debug.Log("Activating upgrade " + metaUpgrade.MetaUpgradeDefinition.Id + ".");
		if (e_MetaState == MetaUpgradesManager.E_MetaState.Unlocked)
		{
			TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Remove(upgrade);
			MetaUpgradesManager.UnlockedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		else
		{
			TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Remove(upgrade);
			MetaUpgradesManager.LockedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Add(upgrade);
		MetaUpgradesManager.ActivatedUpgradesEffects.MetaUpgradeEffectsController.AddUpgradeEffects(upgrade);
	}

	public static void LockUpgrade(MetaUpgrade metaUpgrade)
	{
		MetaUpgrade upgrade;
		MetaUpgradesManager.E_MetaState e_MetaState = MetaUpgradesManager.TryGetUpgradeState(metaUpgrade.MetaUpgradeDefinition.Id, out upgrade);
		if (e_MetaState == MetaUpgradesManager.E_MetaState.NA || e_MetaState == MetaUpgradesManager.E_MetaState.Locked)
		{
			if (e_MetaState == MetaUpgradesManager.E_MetaState.Locked)
			{
				Debug.Log(metaUpgrade.MetaUpgradeDefinition.Id + " upgrade is already locked.");
			}
			return;
		}
		Debug.Log("Locking upgrade " + metaUpgrade.MetaUpgradeDefinition.Id + ".");
		if (e_MetaState == MetaUpgradesManager.E_MetaState.Unlocked)
		{
			TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Remove(upgrade);
			MetaUpgradesManager.UnlockedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		else
		{
			TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Remove(upgrade);
			MetaUpgradesManager.ActivatedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Add(upgrade);
		MetaUpgradesManager.LockedUpgradesEffects.MetaUpgradeEffectsController.AddUpgradeEffects(upgrade);
		TPSingleton<MetaShopsManager>.Instance.RemoveMetaUpgradeFromAlreadySeen(upgrade);
	}

	public static void UnlockUpgrade(MetaUpgrade metaUpgrade)
	{
		MetaUpgrade upgrade;
		MetaUpgradesManager.E_MetaState e_MetaState = MetaUpgradesManager.TryGetUpgradeState(metaUpgrade.MetaUpgradeDefinition.Id, out upgrade);
		if (e_MetaState == MetaUpgradesManager.E_MetaState.NA || e_MetaState == MetaUpgradesManager.E_MetaState.Unlocked)
		{
			if (e_MetaState == MetaUpgradesManager.E_MetaState.Unlocked)
			{
				Debug.Log(metaUpgrade.MetaUpgradeDefinition.Id + " upgrade is already unlocked.");
			}
			return;
		}
		Debug.Log("Unlocking upgrade " + metaUpgrade.MetaUpgradeDefinition.Id + ".");
		if (e_MetaState == MetaUpgradesManager.E_MetaState.Locked)
		{
			TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Remove(upgrade);
			MetaUpgradesManager.LockedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		else
		{
			TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Remove(upgrade);
			MetaUpgradesManager.ActivatedUpgradesEffects.MetaUpgradeEffectsController.RemoveUpgradesEffects(upgrade);
		}
		TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Add(upgrade);
		MetaUpgradesManager.UnlockedUpgradesEffects.MetaUpgradeEffectsController.AddUpgradeEffects(upgrade);
	}
}
