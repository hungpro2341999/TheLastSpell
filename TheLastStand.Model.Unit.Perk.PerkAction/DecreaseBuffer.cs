using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class DecreaseBuffer : APerkAction
{
	public DecreaseBufferDefinition DecreaseBufferDefinition => PerkActionDefinition as DecreaseBufferDefinition;

	public BufferModule BufferModule => base.PerkEvent.PerkModule as BufferModule;

	public DecreaseBuffer(DecreaseBufferDefinition perkActionDefinition, DecreaseBufferController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent perkEvent)
		: base(perkActionDefinition, perkActionController, perkEvent)
	{
	}
}
