using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class IncreaseBuffer : APerkAction
{
	public IncreaseBufferDefinition IncreaseBufferDefinition => PerkActionDefinition as IncreaseBufferDefinition;

	public BufferModule BufferModule => base.PerkEvent.PerkModule as BufferModule;

	public IncreaseBuffer(IncreaseBufferDefinition perkActionDefinition, IncreaseBufferController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
