using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class IncreaseBufferController : APerkActionController
{
	public IncreaseBuffer IncreaseBuffer => PerkAction as IncreaseBuffer;

	public IncreaseBufferController(IncreaseBufferDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new IncreaseBuffer(definition as IncreaseBufferDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		int num = IncreaseBuffer.IncreaseBufferDefinition.ValueExpression.EvalToInt(PerkAction.PerkEvent.PerkModule.Perk);
		switch (IncreaseBuffer.IncreaseBufferDefinition.BufferIndex)
		{
		case BufferModuleDefinition.BufferIndex.Buffer:
			IncreaseBuffer.BufferModule.Buffer += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer2:
			IncreaseBuffer.BufferModule.Buffer2 += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer3:
			IncreaseBuffer.BufferModule.Buffer3 += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer4:
			IncreaseBuffer.BufferModule.Buffer4 += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer5:
			IncreaseBuffer.BufferModule.Buffer5 += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer6:
			IncreaseBuffer.BufferModule.Buffer6 += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer7:
			IncreaseBuffer.BufferModule.Buffer7 += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer8:
			IncreaseBuffer.BufferModule.Buffer8 += num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer9:
			IncreaseBuffer.BufferModule.Buffer9 += num;
			break;
		}
	}
}
