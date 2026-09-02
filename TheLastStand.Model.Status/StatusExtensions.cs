using TPLib.Localization;
using TheLastStand.Definition.Unit;

namespace TheLastStand.Model.Status;

public static class StatusExtensions
{
	public static string GetFormattedStyle(this Status.E_StatusType eStatusType, float? value, int? turnsCount, int? chance, UnitStatDefinition.E_Stat eStat = UnitStatDefinition.E_Stat.Undefined, string valueStylizedOverride = null)
	{
		switch (eStatusType)
		{
		case Status.E_StatusType.AllNegative:
			return "<style=NegativeAlterations>" + Localizer.Get(string.Format("{0}{1}", "StatusName_", eStatusType)) + "</style>";
		case Status.E_StatusType.AllPositive:
			return "<style=PositiveAlterations>" + Localizer.Get(string.Format("{0}{1}", "StatusName_", eStatusType)) + "</style>";
		default:
		{
			string text = string.Format("<style={0}>{1}</style>", eStatusType, Localizer.Get(string.Format("{0}{1}", "SkillEffectName_", eStatusType)));
			string text2 = $"<style={eStatusType}></style>";
			string stylizedName = eStat.GetStylizedName();
			string text3 = (turnsCount.HasValue ? ("(<style=Time>" + ((turnsCount == -1) ? AtlasIcons.InfiniteIcon : $"<style=Number>{turnsCount}</style>") + "</style>)") : string.Empty);
			string text4 = (value.HasValue ? $"<style=BadNb>{value}</style>" : string.Empty);
			string text5 = ((!value.HasValue || eStat == UnitStatDefinition.E_Stat.Undefined) ? string.Empty : (string.IsNullOrEmpty(valueStylizedOverride) ? eStat.GetValueStylized(value.Value) : valueStylizedOverride));
			string text6 = $"<color=white>(<style=Number>{chance}%</style>)</color> ";
			string text7 = ((value.HasValue && eStat != UnitStatDefinition.E_Stat.Undefined) ? (text2 + " " + text5 + " " + stylizedName) : text);
			switch (eStatusType)
			{
			case Status.E_StatusType.Buff:
			case Status.E_StatusType.Debuff:
				return text7 + " " + text3;
			case Status.E_StatusType.Poison:
				return text4 + " " + text + " " + text3;
			case Status.E_StatusType.Stun:
				return text + " " + text6 + text3;
			default:
				return text + " " + text3;
			}
		}
		}
	}
}
