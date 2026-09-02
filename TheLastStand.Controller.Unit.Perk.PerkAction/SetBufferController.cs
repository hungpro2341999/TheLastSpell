using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class SetBufferController : APerkActionController
{
	public SetBuffer SetBuffer => PerkAction as SetBuffer;

	public SetBufferController(SetBufferDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new SetBuffer(definition as SetBufferDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		int num = SetBuffer.SetBufferDefinition.ValueExpression.EvalToInt(PerkAction.PerkEvent.PerkModule.Perk);
		switch (SetBuffer.SetBufferDefinition.BufferIndex)
		{
		case BufferModuleDefinition.BufferIndex.Buffer:
			SetBuffer.BufferModule.Buffer = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer2:
			SetBuffer.BufferModule.Buffer2 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer3:
			SetBuffer.BufferModule.Buffer3 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer4:
			SetBuffer.BufferModule.Buffer4 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer5:
			SetBuffer.BufferModule.Buffer5 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer6:
			SetBuffer.BufferModule.Buffer6 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer7:
			SetBuffer.BufferModule.Buffer7 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer8:
			SetBuffer.BufferModule.Buffer8 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer9:
			SetBuffer.BufferModule.Buffer9 = num;
			break;
		}
	}
}
