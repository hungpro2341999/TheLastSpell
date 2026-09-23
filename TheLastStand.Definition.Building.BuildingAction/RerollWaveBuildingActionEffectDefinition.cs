using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động quay lại/đổi lại đợt tấn công của kẻ địch (Reroll Wave Effect).
/// </summary>
public class RerollWaveBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng Reroll Wave từ dữ liệu XML.
	/// </summary>
	public RerollWaveBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion
}

