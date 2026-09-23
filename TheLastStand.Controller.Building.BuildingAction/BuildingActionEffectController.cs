using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Model.Building.BuildingAction;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller.Building.BuildingAction;

/// <summary>
/// Lớp cơ sở trừu tượng (Base Controller) cho tất cả các hiệu ứng của hành động công trình (Building Action Effects).
/// Quản lý việc kiểm tra tính hợp lệ của mục tiêu (ô Tile) và thực thi logic hiệu ứng cụ thể.
/// </summary>
public abstract class BuildingActionEffectController
{
	#region Properties & Model

	/// <summary>
	/// Model lưu trữ dữ liệu trạng thái và thông số của hiệu ứng hành động.
	/// </summary>
	public BuildingActionEffect BuildingActionEffect { get; protected set; }

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo Controller hiệu ứng hành động cho công trình sản xuất.
	/// </summary>
	/// <param name="definition">Dữ liệu định nghĩa hiệu ứng (từ file cấu hình/XML).</param>
	/// <param name="productionBuilding">Module sản xuất của công trình sở hữu hành động này.</param>
	protected BuildingActionEffectController(BuildingActionEffectDefinition definition, ProductionModule productionBuilding)
	{
	}

	#endregion

	#region Abstract Interface Methods

	/// <summary>
	/// Kiểm tra xem hiệu ứng có thể thực hiện trên ô Tile chỉ định hay không.
	/// </summary>
	/// <param name="tile">Ô Tile mục tiêu cần kiểm tra.</param>
	/// <returns>True nếu ô mục tiêu hợp lệ để kích hoạt hiệu ứng, ngược lại False.</returns>
	public abstract bool CanExecuteActionEffectOnTile(Tile tile);

	/// <summary>
	/// Thực thi logic của hiệu ứng hành động (cộng tài nguyên, hồi máu/mana, xóa sương mù, chế đồ, v.v.).
	/// </summary>
	public abstract void ExecuteActionEffect();

	#endregion
}

