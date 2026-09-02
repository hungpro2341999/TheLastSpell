using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class RefillGaugeController : APerkActionController
{
	public RefillGauge RefillGauge => PerkAction as RefillGauge;

	public RefillGaugeController(RefillGaugeDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new RefillGauge(definition as RefillGaugeDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		RefillGauge.GaugeModule.ValueOffset = 0;
	}
}
