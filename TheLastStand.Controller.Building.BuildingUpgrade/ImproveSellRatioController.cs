using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp tỷ lệ giá bán đồ (Sell Ratio) trong Cửa hàng (Shop).
/// </summary>
public class ImproveSellRatioController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp tỷ lệ bán.
	/// </summary>
	public ImproveSellRatio ImproveSellRatio => base.BuildingUpgradeEffect as ImproveSellRatio;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp tỷ lệ bán.
	/// </summary>
	public ImproveSellRatioController(ImproveSellRatioDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveSellRatio(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Tăng cấp độ tỷ lệ bán hàng của cửa hàng trong BuildingManager.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		TPSingleton<BuildingManager>.Instance.Shop.SellRatioLevel += ImproveSellRatio.ImproveSellRatioDefinition.Value;
	}

	#endregion
}

