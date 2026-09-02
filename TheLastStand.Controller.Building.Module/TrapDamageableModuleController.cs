using TPLib;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class TrapDamageableModuleController : DamageableModuleController
{
	public static class Constants
	{
		public const string DisabledSuffix = "_Disabled";
	}

	public TrapDamageableModule TrapDamageableModule { get; }

	public TrapDamageableModuleController(BuildingController buildingControllerParent, DamageableModuleDefinition damageableModuleDefinition)
		: base(buildingControllerParent, damageableModuleDefinition)
	{
		TrapDamageableModule = base.BuildingModule as TrapDamageableModule;
	}

	public override float Repair()
	{
		return Repair(base.BuildingControllerParent.BattleModuleController.BattleModule.BattleModuleDefinition.MaximumTrapCharges);
	}

	public float Repair(int amount)
	{
		base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges = Mathf.Min(base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges + amount, base.BuildingControllerParent.BattleModuleController.BattleModule.BattleModuleDefinition.MaximumTrapCharges);
		RefreshDisplayedBuilding();
		return base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges;
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new TrapDamageableModule(building, buildingModuleDefinition as DamageableModuleDefinition, this);
	}

	private void RefreshDisplayedBuilding()
	{
		TheLastStand.Model.Building.Building building = base.BuildingControllerParent.Building;
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingInstantly(building, building.OriginTile, (base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges <= 0) ? "_Disabled" : string.Empty);
	}
}
