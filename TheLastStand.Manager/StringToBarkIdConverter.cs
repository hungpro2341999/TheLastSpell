using System;
using System.Collections.Generic;
using TPLib.Debugging.Console;
using TheLastStand.Database;

namespace TheLastStand.Manager;

public class StringToBarkIdConverter : StringToStringCollectionEntryConverter
{
	public static class Constants
	{
		public const string RandomValue = "Random";
	}

	protected override List<string> Entries => new List<string>(BarkDatabase.BarkDefinitions.Keys);

	public override bool TryConvert(string value, out object result)
	{
		if (string.Equals(value, "Random", StringComparison.OrdinalIgnoreCase))
		{
			result = RandomManager.GetRandomElement(this, Entries);
			return true;
		}
		if (!base.TryConvert(value, out result))
		{
			return false;
		}
		return true;
	}

	public override List<string> GetAutoCompleteTexts(string argument)
	{
		List<string> autoCompleteTexts = base.GetAutoCompleteTexts(argument);
		autoCompleteTexts.Insert(0, "Random");
		return autoCompleteTexts;
	}
}
