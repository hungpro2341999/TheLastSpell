using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class InstantiateStatEffectDisplay : InstantiateEffectDisplay
{
	public InstantiateStatEffectDisplayDefinition InstantiateStatEffectDisplayDefinition => PerkActionDefinition as InstantiateStatEffectDisplayDefinition;

	public InstantiateStatEffectDisplay(InstantiateStatEffectDisplayDefinition perkActionDefinition, InstantiateStatEffectDisplayController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
