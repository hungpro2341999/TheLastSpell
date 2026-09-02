using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class RefillGauge : APerkAction
{
	public RefillGaugeDefinition RefillGaugeDefinition => PerkActionDefinition as RefillGaugeDefinition;

	public GaugeModule GaugeModule => base.PerkEvent.PerkModule as GaugeModule;

	public RefillGauge(RefillGaugeDefinition perkActionDefinition, RefillGaugeController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
