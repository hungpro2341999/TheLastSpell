using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class InstantiateBuffEffectDisplay : InstantiateEffectDisplay
{
	public InstantiateBuffEffectDisplayDefinition InstantiateBuffEffectDisplayDefinition => PerkActionDefinition as InstantiateBuffEffectDisplayDefinition;

	public InstantiateBuffEffectDisplay(InstantiateBuffEffectDisplayDefinition perkActionDefinition, InstantiateBuffEffectDisplayController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
