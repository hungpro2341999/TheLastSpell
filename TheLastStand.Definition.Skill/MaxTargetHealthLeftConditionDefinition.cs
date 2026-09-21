using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu máu (Health) còn lại của mục tiêu phải dưới một ngưỡng tối đa.
/// Dùng cho các kỹ năng "hành quyết" (execute/finish) chỉ kích hoạt khi kẻ địch còn ít máu.
/// Ví dụ: kỹ năng chỉ dùng được khi mục tiêu còn ≤ 30% HP.
/// Ngưỡng máu là biểu thức toán học (Expression Node) có thể tính toán động.
/// </summary>
public class MaxTargetHealthLeftConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string MaxTargetHealthLeftName = "MaxTargetHealthLeft";

	#endregion Constants

	#region Properties

	/// <summary>
	/// Ngưỡng máu tối đa mà mục tiêu phải còn lại (biểu thức toán học).
	/// Kỹ năng chỉ khả dụng khi HP hiện tại của mục tiêu ≤ giá trị này.
	/// Dùng Node (Expression Tree) để hỗ trợ cả giá trị cố định lẫn công thức.
	/// </summary>
	public Node HealthThreshold { get; private set; }

	/// <summary>
	/// Tên điều kiện - trả về "MaxTargetHealthLeft".
	/// </summary>
	public override string Name => "MaxTargetHealthLeft";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container chứa biểu thức ngưỡng máu.</param>
	public MaxTargetHealthLeftConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize dữ liệu XML - parse nội dung text thành Expression Node cho ngưỡng máu.
	/// XML format: &lt;MaxTargetHealthLeft&gt;0.3 * TargetMaxHealth&lt;/MaxTargetHealthLeft&gt;
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		HealthThreshold = Parser.Parse(xElement.Value);
	}

	#endregion Public Methods
}
