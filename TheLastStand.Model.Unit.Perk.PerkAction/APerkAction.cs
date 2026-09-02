using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public abstract class APerkAction
{
	public APerkActionController PerkActionController;

	public APerkActionDefinition PerkActionDefinition;

	public TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent PerkEvent { get; private set; }

	public APerkAction(APerkActionDefinition perkActionDefinition, APerkActionController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
	{
		PerkActionDefinition = perkActionDefinition;
		PerkActionController = perkActionController;
		PerkEvent = perkEvent;
	}
}
