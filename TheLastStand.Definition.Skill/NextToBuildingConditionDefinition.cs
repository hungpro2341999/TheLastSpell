using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu đơn vị phải đứng LIỀN KỀ (ô kế bên) một công trình cụ thể.
/// Ví dụ: kỹ năng buff chỉ dùng được khi tướng đứng cạnh Barracks hoặc Forge.
/// Khác với OntoBuildingCondition (phải đứng TRÊN công trình).
/// </summary>
public class NextToBuildingConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string NextToBuildingName = "NextToBuilding";

	#endregion Constants

	#region Properties

	/// <summary>
	/// ID của công trình mà đơn vị phải đứng liền kề.
	/// Hệ thống kiểm tra 4 ô xung quanh (trên/dưới/trái/phải) của đơn vị.
	/// </summary>
	public string BuildingDefinitionId { get; private set; }

	/// <summary>
	/// Tên điều kiện - trả về "NextToBuilding".
	/// </summary>
	public override string Name => "NextToBuilding";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container chứa BuildingDefinitionId.</param>
	public NextToBuildingConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize dữ liệu XML - đọc ID công trình từ nội dung text.
	/// XML format: &lt;NextToBuilding&gt;BarracksId&lt;/NextToBuilding&gt;
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingDefinitionId = xElement.Value;
	}

	#endregion Public Methods
}
