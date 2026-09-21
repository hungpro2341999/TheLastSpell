using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu đơn vị KHÔNG được đang đứng bên trong bất kỳ công trình nào.
/// Ngược lại với InWatchtowerCondition - kỹ năng này chỉ khả dụng khi tướng đang ở ngoài trời.
/// Ví dụ: kỹ năng di chuyển tầm xa chỉ dùng được khi không ở trong Watchtower.
/// Không có tham số bổ sung.
/// </summary>
public class NotInBuildingConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string NotInBuildingName = "NotInBuilding";

	#endregion Constants

	#region Properties

	/// <summary>
	/// Tên điều kiện - trả về "NotInBuilding".
	/// </summary>
	public override string Name => "NotInBuilding";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container (không chứa tham số bổ sung).</param>
	public NotInBuildingConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize - không có tham số bổ sung cần đọc.
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
	}

	#endregion Public Methods
}
