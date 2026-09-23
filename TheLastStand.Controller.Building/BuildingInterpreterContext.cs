using TheLastStand.Manager.Building;
using TheLastStand.Model;

namespace TheLastStand.Controller.Building;

/// <summary>
/// Ngữ cảnh thông dịch công thức (Formula Interpreter Context) cho hệ thống công trình (Building).
/// Cung cấp các biến runtime (như số lượng pháp sư tại Magic Circle) để bộ phân tích công thức tính toán chỉ số động.
/// </summary>
public class BuildingInterpreterContext : FormulaInterpreterContext
{
	#region Properties

	/// <summary>
	/// Số lượng pháp sư (Mage) hiện tại đang làm phép bảo vệ Vòng tròn phép thuật (Magic Circle).
	/// Dùng làm biến số ngữ cảnh cho các công thức tính toán liên quan đến hiệu ứng của công trình.
	/// </summary>
	private int MageCount => BuildingManager.MagicCircle.MageCount;

	#endregion
}
