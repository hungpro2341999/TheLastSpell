using System.Collections.Generic;
using System.Linq;
using TPLib.Debugging.Console;
using TheLastStand.Database.WorldMap;

namespace TheLastStand.Model.WorldMap;

public class StringToCityIdConverter : StringToStringCollectionEntryConverter
{
	protected override List<string> Entries => CityDatabase.CityDefinitions.Keys.ToList();
}
