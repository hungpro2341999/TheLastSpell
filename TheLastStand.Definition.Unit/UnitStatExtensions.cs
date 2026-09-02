using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public static class UnitStatExtensions
{
	public static bool ShownAsPercentage(this UnitStatDefinition.E_Stat stat)
	{
		if (!UnitStatDefinition.ShownAsPercentageDictionary.TryGetValue(stat, out var value))
		{
			CLoggerManager.Log($"Percentage display has not been specified on deserialization for stat {stat}! Then not showing as a percentage.", LogType.Error);
			return false;
		}
		return value;
	}

	public static string GetValueStylized(this float value, bool outlined = true, bool displaySign = true, bool displayPercentage = false, bool? overridenColorIsGood = null)
	{
		string text = ((!(overridenColorIsGood ?? (value >= 0f))) ? (outlined ? "BadNbOutlined" : "BadNb") : (outlined ? "GoodNbOutlined" : "GoodNb"));
		string text2 = ((!displaySign) ? string.Empty : ((value >= 0f) ? "+" : string.Empty));
		string text3 = (displayPercentage ? "%" : string.Empty);
		return $"<style=\"{text}\">{text2}{value}{text3}</style>";
	}

	public static string GetValueStylized(this float? value, bool outlined = true, bool displaySign = true, bool displayPercentage = false, bool? overridenColorIsGood = null)
	{
		if (!value.HasValue)
		{
			return string.Empty;
		}
		return value.Value.GetValueStylized(outlined, displaySign, displayPercentage, overridenColorIsGood);
	}

	public static string GetValueStylized(this int value, bool outlined = true, bool displaySign = true, bool displayPercentage = false, bool? overridenColorIsGood = null)
	{
		return ((float)value).GetValueStylized(outlined, displaySign, displayPercentage, overridenColorIsGood);
	}

	public static string GetValueStylized(this UnitStatDefinition.E_Stat stat, float value, bool outlined = true, bool displaySign = true, bool? overridenColorIsGood = null)
	{
		return value.GetValueStylized(outlined, displaySign, stat.ShownAsPercentage(), overridenColorIsGood);
	}

	public static string GetLocalizedName(this UnitStatDefinition.E_Stat stat)
	{
		if (stat == UnitStatDefinition.E_Stat.Undefined)
		{
			return string.Empty;
		}
		return Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", stat));
	}

	public static string GetStylizedCustomContent(this UnitStatDefinition.E_Stat stat, string contentToStylize)
	{
		if (contentToStylize == null)
		{
			contentToStylize = string.Empty;
		}
		if (!UnitDatabase.UnitStatDefinitions.TryGetValue(stat, out var value))
		{
			return string.Empty;
		}
		return $"<style={value?.GetChildStatIfExists()}>{contentToStylize}</style>";
	}

	public static string GetStylizedName(this UnitStatDefinition.E_Stat stat)
	{
		return stat.GetStylizedCustomContent(stat.GetLocalizedName());
	}

	public static UnitStatDefinition.E_Stat GetChildStatIfExists(this UnitStatDefinition stat)
	{
		if (stat.ChildStatId == UnitStatDefinition.E_Stat.Undefined)
		{
			return stat.Id;
		}
		return stat.ChildStatId;
	}

	public static UnitStatDefinition GetChildStatDefinitionIfExists(this UnitStatDefinition stat)
	{
		if (stat.ChildStatId == UnitStatDefinition.E_Stat.Undefined)
		{
			return stat;
		}
		return UnitDatabase.UnitStatDefinitions[stat.GetChildStatIfExists()];
	}

	public static UnitStatDefinition.E_Stat GetParentStatIfExists(this UnitStatDefinition stat)
	{
		if (stat.ParentStatId == UnitStatDefinition.E_Stat.Undefined)
		{
			return stat.Id;
		}
		return stat.ParentStatId;
	}

	public static UnitStatDefinition GetParentStatDefinitionIfExists(this UnitStatDefinition stat)
	{
		if (stat.ParentStatId == UnitStatDefinition.E_Stat.Undefined)
		{
			return stat;
		}
		return UnitDatabase.UnitStatDefinitions[stat.GetParentStatIfExists()];
	}
}
