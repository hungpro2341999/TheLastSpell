using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Building.Module;

public abstract class BuildingModuleDefinition : TheLastStand.Framework.Serialization.Definition
{
	#region Fields & Properties
	/// <summary>
	/// Định nghĩa công trình cha sở hữu module này.
	/// </summary>
	public readonly BuildingDefinition BuildingDefinition;
	#endregion

	#region Initialization
	/// <summary>
	/// Khởi tạo định nghĩa module của công trình và thực hiện đọc dữ liệu XML (Deserialize).
	/// </summary>
	protected BuildingModuleDefinition(BuildingDefinition buildingDefinition, XContainer moduleDefinitionContainer)
		: base(null)
	{
		BuildingDefinition = buildingDefinition;
		Deserialize(moduleDefinitionContainer);
	}
	#endregion
}
