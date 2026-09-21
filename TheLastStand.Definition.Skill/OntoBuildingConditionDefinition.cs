using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu đơn vị phải đang đứng TRÊN (bên trong) một công trình cụ thể.
/// Khác với NextToBuildingCondition (chỉ cần đứng liền kề) - điều kiện này yêu cầu 
/// đơn vị phải chiếm cùng ô với công trình.
/// Ví dụ: kỹ năng sửa chữa chỉ dùng được khi tướng đứng trên công trình bị hỏng.
/// </summary>
public class OntoBuildingConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string OntoBuildingName = "OntoBuilding";

	#endregion Constants

	#region Properties

	/// <summary>
	/// ID của công trình mà đơn vị phải đứng trên đó.
	/// </summary>
	public string BuildingDefinitionId { get; private set; }

	/// <summary>
	/// Tên điều kiện - trả về "OntoBuilding".
	/// </summary>
	public override string Name => "OntoBuilding";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container chứa BuildingDefinitionId.</param>
	public OntoBuildingConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize dữ liệu XML - đọc ID công trình từ nội dung text.
	/// XML format: &lt;OntoBuilding&gt;WatchtowerId&lt;/OntoBuilding&gt;
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingDefinitionId = xElement.Value;
	}

	#endregion Public Methods
}
