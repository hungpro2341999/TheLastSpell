using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller quản lý các nâng cấp toàn cục (Global Upgrade) áp dụng cho nhiều/tất cả các công trình thuộc cùng loại.
/// Kế thừa từ BuildingUpgradeController.
/// </summary>
public class BuildingGlobalUpgradeController : BuildingUpgradeController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp toàn cục.
	/// </summary>
	public BuildingGlobalUpgrade BuildingGlobalUpgrade => base.BuildingUpgrade as BuildingGlobalUpgrade;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp toàn cục từ dữ liệu lưu trữ.
	/// </summary>
	public BuildingGlobalUpgradeController(SerializedGlobalUpgrade container, TheLastStand.Model.Building.Building building)
		: base(container, building)
	{
	}

	/// <summary>
	/// Khởi tạo controller nâng cấp toàn cục từ định nghĩa.
	/// </summary>
	public BuildingGlobalUpgradeController(BuildingUpgradeDefinition definition, TheLastStand.Model.Building.Building building)
		: base(definition, building)
	{
	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// Tạo đối tượng Model dữ liệu cho nâng cấp toàn cục (từ dữ liệu lưu trữ).
	/// </summary>
	protected override TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(SerializedUpgrade container, BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new BuildingGlobalUpgrade(container, definition, controller, building);
	}

	/// <summary>
	/// Tạo đối tượng Model dữ liệu cho nâng cấp toàn cục (từ định nghĩa).
	/// </summary>
	protected override TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade CreateModel(BuildingUpgradeDefinition definition, BuildingUpgradeController controller, TheLastStand.Model.Building.Building building)
	{
		return new BuildingGlobalUpgrade(definition, controller, building);
	}

	/// <summary>
	/// Phát hiệu ứng kỹ năng / thị giác cho tất cả các công trình bị ảnh hưởng bởi nâng cấp toàn cục này.
	/// </summary>
	protected override void PlayFx()
	{
		foreach (BuildingGlobalUpgrade buildingGlobalUpgrade in BuildingGlobalUpgrade.BuildingUpgradeLevel.BuildingGlobalUpgrades)
		{
			(buildingGlobalUpgrade.BuildingUpgradeLevels[buildingGlobalUpgrade.UpgradeLevel].OverrideCastFx ?? buildingGlobalUpgrade.CastFx)?.CastFxController.PlayCastFxs(TileObjectSelectionManager.E_Orientation.NONE, default(Vector2), base.BuildingUpgrade.Building);
		}
	}

	#endregion
}

