using TPLib;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class ForbidSkillUndoController : APerkActionController
{
	public ForbidSkillUndo ForbidSkillUndo => PerkAction as ForbidSkillUndo;

	public ForbidSkillUndoController(APerkActionDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new ForbidSkillUndo(definition as ForbidSkillUndoDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		TPSingleton<PlayableUnitManager>.Instance.ShouldClearUndoStack = true;
	}
}
