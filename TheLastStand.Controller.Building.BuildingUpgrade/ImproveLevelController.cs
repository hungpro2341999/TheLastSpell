using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp cấp độ sản xuất của công trình (hoặc cấp độ sản xuất vật phẩm toàn cục).
/// </summary>
public class ImproveLevelController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp cấp độ.
	/// </summary>
	public ImproveLevel ImproveLevel => base.BuildingUpgradeEffect as ImproveLevel;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp cấp độ công trình.
	/// </summary>
	public ImproveLevelController(ImproveLevelDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveLevel(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Kích hoạt cộng thêm cấp độ sản xuất (chỉ chạy khi không phải tải lại dữ liệu lưu).
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		if (!onLoad)
		{
			if (base.BuildingUpgradeEffect.BuildingUpgrade.BuildingUpgradeDefinition.IsGlobal)
			{
				TPSingleton<BuildingManager>.Instance.GlobalItemProductionUpgradeLevel.Level += ImproveLevel.ImproveLevelDefinition.LevelsCount;
			}
			else
			{
				base.BuildingUpgradeEffect.BuildingUpgrade.Building.ProductionModule.Level += ImproveLevel.ImproveLevelDefinition.LevelsCount;
			}
		}
	}

	#endregion
}

