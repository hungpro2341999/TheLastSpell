using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class ForbidSkillUndo : APerkAction
{
	public ForbidSkillUndoDefinition ForbidSkillUndoDefinition => PerkActionDefinition as ForbidSkillUndoDefinition;

	public ForbidSkillUndo(ForbidSkillUndoDefinition perkActionDefinition, ForbidSkillUndoController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
