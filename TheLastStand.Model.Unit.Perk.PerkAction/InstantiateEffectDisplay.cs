using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public abstract class InstantiateEffectDisplay : APerkAction
{
	public InstantiateEffectDisplayDefinition InstantiateEffectDisplayDefinition => PerkActionDefinition as InstantiateEffectDisplayDefinition;

	public InstantiateEffectDisplay(InstantiateEffectDisplayDefinition perkActionDefinition, InstantiateEffectDisplayController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
