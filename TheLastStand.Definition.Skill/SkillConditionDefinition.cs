using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Lớp cơ sở trừu tượng (Abstract Base Class) cho tất cả các điều kiện sử dụng kỹ năng.
/// Mỗi điều kiện (condition) xác định một ràng buộc ngữ cảnh mà tướng/đơn vị phải thỏa mãn
/// trước khi có thể sử dụng một kỹ năng cụ thể (ví dụ: phải đứng trong tháp canh, 
/// phải ở gần công trình, chỉ được dùng trong pha đêm...).
/// Các lớp con cụ thể override phương thức Deserialize để đọc tham số từ XML.
/// </summary>
public abstract class SkillConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Properties

	/// <summary>
	/// Tên định danh của điều kiện (ví dụ: "InWatchtower", "NextToBuilding"...).
	/// Được dùng để so khớp khi deserialize từ XML và kiểm tra điều kiện runtime.
	/// </summary>
	public virtual string Name { get; }

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Constructor - khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// Gọi base constructor để tự động kích hoạt Deserialize().
	/// </summary>
	/// <param name="container">XML container chứa dữ liệu cấu hình điều kiện.</param>
	public SkillConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors
}
