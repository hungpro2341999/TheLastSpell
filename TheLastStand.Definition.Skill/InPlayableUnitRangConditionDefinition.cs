using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu phải có đơn vị đồng minh (Playable Unit) 
/// trong phạm vi nhất định (MaxRange) tính từ vị trí đơn vị đang sử dụng kỹ năng.
/// Ví dụ: kỹ năng hỗ trợ chỉ dùng được khi có đồng đội ở gần (1-2 ô).
/// </summary>
public class InPlayableUnitRangConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string InPlayableUnitRangeName = "InPlayableUnitRange";

	#endregion Constants

	#region Properties

	/// <summary>
	/// Phạm vi tối đa (tính bằng số ô) để kiểm tra sự hiện diện của đơn vị đồng minh.
	/// Mặc định = 1 (đơn vị đồng minh phải ở ô liền kề). Tối thiểu là 1.
	/// </summary>
	public int MaxRange { get; private set; } = 1;

	/// <summary>
	/// Tên điều kiện - trả về "InPlayableUnitRange".
	/// </summary>
	public override string Name => "InPlayableUnitRange";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container chứa attribute MaxRange.</param>
	public InPlayableUnitRangConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize dữ liệu XML - đọc thuộc tính MaxRange.
	/// Nếu MaxRange không được cung cấp hoặc &lt;= 0, mặc định sẽ là 1.
	/// XML format: &lt;InPlayableUnitRange MaxRange="2"/&gt;
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("MaxRange");
		if (xAttribute != null && int.TryParse(xAttribute.Value, out var result))
		{
			MaxRange = result;
		}
		if (MaxRange <= 0)
		{
			MaxRange = 1;
		}
	}

	#endregion Public Methods
}
