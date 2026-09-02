using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit;

namespace TheLastStand.Model.Status;

public class BuffStatus : StatModifierStatus
{
	public override E_StatusType StatusType => E_StatusType.Buff;

	public BuffStatus(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
		: base(statusController, unit, statusCreationInfo)
	{
	}

	public override string GetStylizedStatus()
	{
		string text = $"{base.ModifierValue}";
		if (base.ModifierValue > 0f)
		{
			text = "+" + text;
		}
		text += (base.Stat.ShownAsPercentage() ? "%" : string.Empty);
		text = "<style=GoodNb>" + text + "</style>";
		string text2 = ((base.Stat != UnitStatDefinition.E_Stat.Undefined) ? Localizer.Get(string.Format("{0}{1}", "UnitStat_Name_", base.Stat)) : string.Empty);
		UnitStatDefinition stat = UnitDatabase.UnitStatDefinitions[base.Stat];
		return $"{text} <style={stat.GetChildStatIfExists()}>{text2}</style> (<style=Time>{((base.RemainingTurnsCount == -1) ? AtlasIcons.InfiniteIcon : $"<style=Number>{base.RemainingTurnsCount}</style>")}</style>)";
	}
}
