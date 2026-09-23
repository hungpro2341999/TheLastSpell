using System.Collections.Generic;
using System.Linq;
using TPLib.Debugging.Console;
using TheLastStand.Database.Building;

namespace TheLastStand.Definition.Building;

/// <summary>
/// Bộ chuyển đổi (Converter) dùng cho Debug Console để gợi ý / autocompletion các ID định nghĩa hướng sinh công trình ngẫu nhiên từ BuildingDatabase.
/// </summary>
public class StringToRandomBuildingsDirectionsDefinitionIdConverter : StringToStringCollectionEntryConverter
{
	#region Overridden Properties

	/// <summary>
	/// Danh sách tất cả các ID định nghĩa hướng sinh công trình ngẫu nhiên hiện có trong cơ sở dữ liệu.
	/// </summary>
	protected override List<string> Entries => BuildingDatabase.RandomBuildingsDirectionsDefinitions.Keys.ToList();

	#endregion
}

