using System.Collections.Generic;
using System.Linq;
using TPLib.Debugging.Console;
using TheLastStand.Database.Building;

namespace TheLastStand.Definition.Building;

public class StringToRandomBuildingsDirectionsDefinitionIdConverter : StringToStringCollectionEntryConverter
{
	protected override List<string> Entries => BuildingDatabase.RandomBuildingsDirectionsDefinitions.Keys.ToList();
}
