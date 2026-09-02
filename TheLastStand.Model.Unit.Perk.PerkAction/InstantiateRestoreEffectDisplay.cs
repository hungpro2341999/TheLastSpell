using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class InstantiateRestoreEffectDisplay : InstantiateEffectDisplay
{
	public InstantiateRestoreEffectDisplayDefinition InstantiateRestoreEffectDisplayDefinition => PerkActionDefinition as InstantiateRestoreEffectDisplayDefinition;

	public InstantiateRestoreEffectDisplay(InstantiateRestoreEffectDisplayDefinition perkActionDefinition, InstantiateRestoreEffectDisplayController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
