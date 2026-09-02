using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public abstract class APerkActionController
{
	public readonly APerkAction PerkAction;

	public APerkActionController(APerkActionDefinition definition, PerkEvent pEvent)
	{
		PerkAction = CreateModel(definition, pEvent);
	}

	protected abstract APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent);

	public abstract void Trigger(PerkDataContainer data);
}
