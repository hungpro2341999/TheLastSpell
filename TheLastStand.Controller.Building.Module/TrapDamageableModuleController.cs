using TPLib;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class TrapDamageableModuleController : DamageableModuleController
{
	#region Constants & Properties
	public static class Constants
	{
		public const string DisabledSuffix = "_Disabled";
	}

	/// <summary>
	/// Model nhận sát thương của cạm bẫy (TrapDamageableModule).
	/// </summary>
	public TrapDamageableModule TrapDamageableModule { get; }
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller nhận sát thương của cạm bẫy.
	/// </summary>
	public TrapDamageableModuleController(BuildingController buildingControllerParent, DamageableModuleDefinition damageableModuleDefinition)
		: base(buildingControllerParent, damageableModuleDefinition)
	{
		TrapDamageableModule = base.BuildingModule as TrapDamageableModule;
	}

	/// <summary>
	/// Khởi tạo Model TrapDamageableModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new TrapDamageableModule(building, buildingModuleDefinition as DamageableModuleDefinition, this);
	}
	#endregion

	#region Repair & Display Logic
	/// <summary>
	/// Sửa chữa bẫy: Hồi phục tối đa số lần kích hoạt (charges) của bẫy.
	/// </summary>
	public override float Repair()
	{
		return Repair(base.BuildingControllerParent.BattleModuleController.BattleModule.BattleModuleDefinition.MaximumTrapCharges);
	}

	/// <summary>
	/// Sửa chữa bẫy: Thêm số lần kích hoạt chỉ định cho bẫy và cập nhật hiển thị.
	/// </summary>
	public float Repair(int amount)
	{
		base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges = Mathf.Min(base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges + amount, base.BuildingControllerParent.BattleModuleController.BattleModule.BattleModuleDefinition.MaximumTrapCharges);
		RefreshDisplayedBuilding();
		return base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges;
	}

	/// <summary>
	/// Cập nhật hình ảnh bẫy (hiển thị dạng Disabled nếu hết lượt kích hoạt).
	/// </summary>
	private void RefreshDisplayedBuilding()
	{
		TheLastStand.Model.Building.Building building = base.BuildingControllerParent.Building;
		TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingInstantly(building, building.OriginTile, (base.BuildingControllerParent.BattleModuleController.BattleModule.RemainingTrapCharges <= 0) ? "_Disabled" : string.Empty);
	}
	#endregion
}
