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
	#region Properties
	/// <summary>
	/// Model nâng cấp (UpgradeModule) của công trình.
	/// </summary>
	public UpgradeModule UpgradeModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller quản lý việc nâng cấp công trình.
	/// </summary>
	public UpgradeModuleController(BuildingController buildingControllerParent, UpgradeModuleDefinition upgradeModuleDefinition)
		: base(buildingControllerParent, upgradeModuleDefinition)
	{
		UpgradeModule = base.BuildingModule as UpgradeModule;
	}

	/// <summary>
	/// Khởi tạo Model UpgradeModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new UpgradeModule(building, buildingModuleDefinition as UpgradeModuleDefinition, this);
	}
	#endregion

	#region Upgrade Creation & Life Cycle
	/// <summary>
	/// Khởi tạo danh sách nâng cấp cá nhân và nâng cấp toàn cục (Global Upgrades) cho công trình.
	/// </summary>
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

	/// <summary>
	/// Hủy bỏ các hiệu ứng nâng cấp toàn cục khi công trình bị phá hủy.
	/// </summary>
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
	#endregion

	#region Serialization / Deserialization
	/// <summary>
	/// Giải mã và phục hồi trạng thái các nâng cấp cá nhân từ file Save.
	/// </summary>
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

	/// <summary>
	/// Giải mã và phục hồi trạng thái các nâng cấp toàn cục (Global Upgrades) từ file Save.
	/// </summary>
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
	#endregion
}
