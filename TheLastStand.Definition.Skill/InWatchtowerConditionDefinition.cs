using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu đơn vị phải đang đứng trong Tháp Canh (Watchtower).
/// Kỹ năng có điều kiện này chỉ khả dụng khi tướng đã vào bên trong Watchtower
/// (thông qua kỹ năng GoIntoWatchtower).
/// Không có tham số bổ sung - chỉ kiểm tra trạng thái có/không ở trong tháp canh.
/// </summary>
public class InWatchtowerConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string InWatchtowerName = "InWatchtower";

	#endregion Constants

	#region Properties

	/// <summary>
	/// Tên điều kiện - trả về "InWatchtower".
	/// </summary>
	public override string Name => "InWatchtower";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container (không chứa tham số bổ sung).</param>
	public InWatchtowerConditionDefinition(XContainer container)
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
