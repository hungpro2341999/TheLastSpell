using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển logic tạo một mục vật phẩm trong danh sách bày bán (Item Roster) của Shop.
/// Đóng vai trò thành phần phụ trợ cấu hình quy tắc và cây xác suất tạo item cho GenerateNewItemsRosterController.
/// </summary>
public class CreateRosterItemController
{
	#region Properties

	/// <summary>
	/// Model lưu trữ dữ liệu quy tắc tạo item và cây xác suất cấp độ tương ứng.
	/// </summary>
	public CreateRosterItem CreateRosterItem { get; protected set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo CreateRosterItemController liên kết với module sản xuất và định nghĩa tạo item.
	/// </summary>
	/// <param name="buildingProductionModule">Module sản xuất của công trình cha.</param>
	/// <param name="createRosterItemDefinition">Định nghĩa cấu hình tạo item trong roster.</param>
	public CreateRosterItemController(ProductionModule buildingProductionModule, CreateRosterItemDefinition createRosterItemDefinition)
	{
		CreateRosterItem = new CreateRosterItem(buildingProductionModule, createRosterItemDefinition, this);
	}

	#endregion
}
