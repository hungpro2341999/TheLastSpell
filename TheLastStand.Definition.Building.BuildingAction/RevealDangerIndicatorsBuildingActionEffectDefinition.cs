using System.Xml.Linq;

namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Định nghĩa hiệu ứng hành động hiển thị các cảnh báo mối đe dọa / nguy hiểm từ kẻ địch (Reveal Danger Indicators Effect).
/// </summary>
public class RevealDangerIndicatorsBuildingActionEffectDefinition : BuildingActionEffectDefinition
{
	#region Constructors

	/// <summary>
	/// Khởi tạo định nghĩa hiệu ứng hiển thị chỉ số nguy hiểm từ dữ liệu XML.
	/// </summary>
	public RevealDangerIndicatorsBuildingActionEffectDefinition(XContainer xContainer, BuildingActionDefinition buildingActionDefinitionContainer)
		: base(xContainer, buildingActionDefinitionContainer)
	{
	}

	#endregion
}

