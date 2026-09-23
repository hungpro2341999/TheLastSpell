using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Lớp cơ sở trừu tượng (Abstract Base Class) định nghĩa cho tất cả các hiệu ứng của hành động công trình (Building Action Effect).
/// </summary>
public abstract class BuildingActionEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Enums

	/// <summary>
	/// Quy định mục tiêu tác động của hành động công trình (Tất cả hoặc 1 mục tiêu đơn).
	/// </summary>
	public enum E_BuildingActionTargeting
	{
		All,
		Single
	}

	#endregion

	#region Properties

	/// <summary>
	/// Định nghĩa hành động công trình chứa hiệu ứng này.
	/// </summary>
	public BuildingActionDefinition BuildingActionDefinitionContainer { get; }

	/// <summary>
	/// Mã ID biểu tượng ước tính hiệu ứng hành động trên giao diện UI.
	/// </summary>
	public virtual string ActionEstimationIconId { get; } = string.Empty;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng hành động công trình từ dữ liệu XML container.
	/// </summary>
	public BuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer)
	{
		BuildingActionDefinitionContainer = buildingActionDefinitionContainer;
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Giải mã dữ liệu XML cho hiệu ứng hành động công trình.
	/// </summary>
	public override void Deserialize(XContainer xContainer)
	{
	}

	#endregion
}

