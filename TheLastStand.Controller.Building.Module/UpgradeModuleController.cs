using System;
using System.Collections.Generic;
using TPLib.Log;
using TheLastStand.Controller.Building.BuildingUpgrade;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class UpgradeModuleController : BuildingModuleController
{
	public UpgradeModule UpgradeModule { get; }

	public UpgradeModuleController(BuildingController buildingControllerParent, UpgradeModuleDefinition upgradeModuleDefinition)
		: base(buildingControllerParent, upgradeModuleDefinition)
	{
		UpgradeModule = base.BuildingModule as UpgradeModule;
	}

	public void OnDeath()
	{
		if (UpgradeModule.BuildingGlobalUpgrades == null)
		{
			return;
		}
		foreach (BuildingGlobalUpgrade buildingGlobalUpgrade in UpgradeModule.BuildingGlobalUpgrades)
		{
			buildingGlobalUpgrade.BuildingUpgradeLevel.BuildingGlobalUpgrades.Remove(buildingGlobalUpgrade);
		}
	}

	public void CreateUpgrades()
	{
		if (UpgradeModule.UpgradeModuleDefinition.BuildingUpgradeDefinitions == null)
		{
			return;
		}
		UpgradeModule.BuildingUpgrades = new List<TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade>();
		UpgradeModule.BuildingGlobalUpgrades = new List<BuildingGlobalUpgrade>();
		int i = 0;
		for (int count = UpgradeModule.UpgradeModuleDefinition.BuildingUpgradeDefinitions.Count; i < count; i++)
		{
			if (UpgradeModule.UpgradeModuleDefinition.BuildingUpgradeDefinitions[i].IsGlobal && ApplicationManager.CurrentStateName != "LevelEditor")
			{
				UpgradeModule.BuildingGlobalUpgrades.Add(new BuildingGlobalUpgradeController(UpgradeModule.UpgradeModuleDefinition.BuildingUpgradeDefinitions[i], UpgradeModule.BuildingParent).BuildingGlobalUpgrade);
			}
			else
			{
				UpgradeModule.BuildingUpgrades.Add(new BuildingUpgradeController(UpgradeModule.UpgradeModuleDefinition.BuildingUpgradeDefinitions[i], UpgradeModule.BuildingParent).BuildingUpgrade);
			}
		}
	}

	public void DeserializeUpgrades(List<SerializedUpgrade> upgradesElement)
	{
		UpgradeModule.BuildingUpgrades = new List<TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade>();
		if (upgradesElement == null)
		{
			return;
		}
		foreach (SerializedUpgrade item in upgradesElement)
		{
			try
			{
				UpgradeModule.BuildingUpgrades.Add(new BuildingUpgradeController(item, UpgradeModule.BuildingParent).BuildingUpgrade);
			}
			catch (Exception arg)
			{
				CLoggerManager.Log($"Unable to Deserialize Upgrade: {item.Id}, skipping.\n{arg}", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}

	public void DeserializeGlobalUpgrades(List<SerializedGlobalUpgrade> upgradesElement)
	{
		UpgradeModule.BuildingGlobalUpgrades = new List<BuildingGlobalUpgrade>();
		if (upgradesElement == null || upgradesElement.Count == 0)
		{
			return;
		}
		foreach (SerializedGlobalUpgrade item in upgradesElement)
		{
			try
			{
				UpgradeModule.BuildingGlobalUpgrades.Add(new BuildingGlobalUpgradeController(item, UpgradeModule.BuildingParent).BuildingGlobalUpgrade);
			}
			catch (Exception arg)
			{
				CLoggerManager.Log($"Unable to Deserialize GlobalUpgrade: {item.Id}, skipping.\n{arg}", LogType.Error, CLogLevel.MAJOR);
			}
		}
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new UpgradeModule(building, buildingModuleDefinition as UpgradeModuleDefinition, this);
	}
}
