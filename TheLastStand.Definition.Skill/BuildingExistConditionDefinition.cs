using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu một công trình cụ thể phải tồn tại trên bản đồ.
/// Ví dụ: kỹ năng đặc biệt chỉ khả dụng khi đã xây dựng Forge (Lò rèn) hoặc một công trình nhất định.
/// </summary>
public class BuildingExistConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string BuildingExistName = "BuildingExist";

	#endregion Constants

	#region Properties

	/// <summary>
	/// ID của công trình cần tồn tại trên bản đồ để kỹ năng khả dụng.
	/// Được đọc từ nội dung text của element XML.
	/// </summary>
	public string BuildingDefinitionId { get; private set; }

	/// <summary>
	/// Tên điều kiện - trả về "BuildingExist".
	/// </summary>
	public override string Name => "BuildingExist";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container chứa BuildingDefinitionId.</param>
	public BuildingExistConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize dữ liệu XML - đọc ID công trình từ nội dung text của element.
	/// XML format: &lt;BuildingExist&gt;ForgeId&lt;/BuildingExist&gt;
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		BuildingDefinitionId = xElement.Value;
	}

	#endregion Public Methods
}
