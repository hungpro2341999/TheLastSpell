using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class TriggerEffects : APerkAction
{
	public TriggerEffects(TriggerEffectsDefinition perkActionDefinition, TriggerEffectsController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent pPerkEvent)
		: base(perkActionDefinition, perkActionController, pPerkEvent)
	{
	}
}
