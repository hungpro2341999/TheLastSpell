using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class LogPerkAction : APerkAction
{
	public LogPerkActionDefinition LogPerkActionDefinition => PerkActionDefinition as LogPerkActionDefinition;

	public LogPerkAction(LogPerkActionDefinition perkActionDefinition, LogPerkActionController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
