using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: yêu cầu mục tiêu phải có mức độ chấn thương (Injury Stage) 
/// tối thiểu đạt một ngưỡng nhất định.
/// Hệ thống Injury chia thương tích thành nhiều cấp (stage) - kỹ năng này chỉ khả dụng 
/// khi mục tiêu đã bị thương nặng đến một mức cụ thể.
/// Ví dụ: kỹ năng chữa thương nặng chỉ dùng được khi đồng đội ở Injury Stage ≥ 2.
/// </summary>
public class MinTargetInjuryStageConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string MinTargetInjuryStageName = "MinTargetInjuryStage";

	#endregion Constants

	#region Properties

	/// <summary>
	/// Mức Injury Stage tối thiểu mà mục tiêu phải đạt (biểu thức toán học).
	/// Kỹ năng chỉ khả dụng khi Injury Stage hiện tại của mục tiêu ≥ giá trị này.
	/// </summary>
	public Node RequiredInjuryStage { get; private set; }

	/// <summary>
	/// Tên điều kiện - trả về "MinTargetInjuryStage".
	/// </summary>
	public override string Name => "MinTargetInjuryStage";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container chứa biểu thức ngưỡng Injury Stage.</param>
	public MinTargetInjuryStageConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize dữ liệu XML - parse nội dung text thành Expression Node.
	/// XML format: &lt;MinTargetInjuryStage&gt;2&lt;/MinTargetInjuryStage&gt;
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		RequiredInjuryStage = Parser.Parse(xElement.Value);
	}

	#endregion Public Methods
}
