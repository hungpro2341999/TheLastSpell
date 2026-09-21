using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Skill;

/// <summary>
/// Điều kiện sử dụng kỹ năng: giới hạn kỹ năng chỉ khả dụng trong một hoặc nhiều pha cụ thể của lượt chơi.
/// Các pha bao gồm: Night (pha đêm - chiến đấu), Production (pha sản xuất - ban ngày),
/// Deployment (pha triển khai - bố trí tướng).
/// Khác với AllowDuringPhase trên SkillDefinition (áp dụng cố định), điều kiện này 
/// được đánh giá động theo ngữ cảnh (contextual condition).
/// </summary>
public class OnlyDuringPhaseConditionDefinition : SkillConditionDefinition
{
	#region Constants

	/// <summary>Tên định danh của điều kiện dùng cho so khớp XML.</summary>
	public const string OnlyDuringPhaseName = "OnlyDuringPhase";

	#endregion Constants

	#region Properties

	/// <summary>
	/// True nếu kỹ năng khả dụng trong pha Triển khai (Deployment).
	/// </summary>
	public bool DuringDeployment { get; set; }

	/// <summary>
	/// True nếu kỹ năng khả dụng trong pha Đêm (Night) - pha chiến đấu chống quái.
	/// </summary>
	public bool DuringNight { get; set; }

	/// <summary>
	/// True nếu kỹ năng khả dụng trong pha Sản xuất (Production) - pha ban ngày.
	/// </summary>
	public bool DuringProduction { get; set; }

	/// <summary>
	/// Tên điều kiện - trả về "OnlyDuringPhase".
	/// </summary>
	public override string Name => "OnlyDuringPhase";

	#endregion Properties

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa điều kiện từ dữ liệu XML.
	/// </summary>
	/// <param name="container">XML container chứa danh sách các pha được phép.</param>
	public OnlyDuringPhaseConditionDefinition(XContainer container)
		: base(container)
	{
	}

	#endregion Constructors

	#region Public Methods

	/// <summary>
	/// Deserialize dữ liệu XML - duyệt các element con để xác định pha nào được phép.
	/// XML format:
	/// &lt;OnlyDuringPhase&gt;
	///   &lt;Night/&gt;
	///   &lt;Production/&gt;
	/// &lt;/OnlyDuringPhase&gt;
	/// </summary>
	/// <param name="container">XML container.</param>
	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in (container as XElement).Elements())
		{
			if (item.Name == "Night")
			{
				DuringNight = true;
			}
			else if (item.Name == "Production")
			{
				DuringProduction = true;
			}
			else if (item.Name == "Deployment")
			{
				DuringDeployment = true;
			}
			else
			{
				TPDebug.LogError($"{item.Name} is not a valid phase name");
			}
		}
	}

	#endregion Public Methods
}
