using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class TriggerEffectsController : APerkActionController
{
	public TriggerEffectsController(TriggerEffectsDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new TriggerEffects(definition as TriggerEffectsDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		PerkAction.PerkEvent.PerkModule.TryTriggerEffects(data);
	}
}
