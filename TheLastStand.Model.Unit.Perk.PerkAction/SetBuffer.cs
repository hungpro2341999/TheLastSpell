using TheLastStand.Controller.Unit.Perk.PerkAction;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkAction;

public class SetBuffer : APerkAction
{
	public SetBufferDefinition SetBufferDefinition => PerkActionDefinition as SetBufferDefinition;

	public BufferModule BufferModule => base.PerkEvent.PerkModule as BufferModule;

	public SetBuffer(APerkActionDefinition perkActionDefinition, APerkActionController perkActionController, TheLastStand.Model.Unit.Perk.PerkEvent.PerkEvent pPerkEvent)
		: base(perkActionDefinition, perkActionController, pPerkEvent)
	{
	}
}
