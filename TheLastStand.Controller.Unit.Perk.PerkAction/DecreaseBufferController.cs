using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class DecreaseBufferController : APerkActionController
{
	public DecreaseBuffer DecreaseBuffer => PerkAction as DecreaseBuffer;

	public DecreaseBufferController(DecreaseBufferDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new DecreaseBuffer(definition as DecreaseBufferDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		int num = DecreaseBuffer.DecreaseBufferDefinition.ValueExpression.EvalToInt(PerkAction.PerkEvent.PerkModule.Perk);
		switch (DecreaseBuffer.DecreaseBufferDefinition.BufferIndex)
		{
		case BufferModuleDefinition.BufferIndex.Buffer:
			DecreaseBuffer.BufferModule.Buffer -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer2:
			DecreaseBuffer.BufferModule.Buffer2 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer3:
			DecreaseBuffer.BufferModule.Buffer3 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer4:
			DecreaseBuffer.BufferModule.Buffer4 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer5:
			DecreaseBuffer.BufferModule.Buffer5 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer6:
			DecreaseBuffer.BufferModule.Buffer6 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer7:
			DecreaseBuffer.BufferModule.Buffer7 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer8:
			DecreaseBuffer.BufferModule.Buffer8 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer9:
			DecreaseBuffer.BufferModule.Buffer9 -= num;
			break;
		}
	}
}
