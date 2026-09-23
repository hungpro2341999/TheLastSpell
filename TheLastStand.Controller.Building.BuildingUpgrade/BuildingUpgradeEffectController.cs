using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Lớp cơ sở trừu tượng (Abstract Base Class) cho tất cả các controller xử lý hiệu ứng của nâng cấp công trình (Upgrade Effect).
/// </summary>
public abstract class BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu chứa thông tin hiệu ứng nâng cấp.
	/// </summary>
	public BuildingUpgradeEffect BuildingUpgradeEffect { get; protected set; }

	#endregion

	#region Public Methods

	/// <summary>
	/// Kích hoạt và áp dụng hiệu ứng nâng cấp vào hệ thống/công trình.
	/// </summary>
	/// <param name="onLoad">True nếu hiệu ứng đang được kích hoạt lại trong quá trình tải game (Save/Load).</param>
	public abstract void TriggerEffect(bool onLoad = false);

	#endregion
}

