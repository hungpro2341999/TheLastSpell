using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ResetBufferEffectController : APerkEffectController
{
	public ResetBufferEffect ResetBufferEffect => base.PerkEffect as ResetBufferEffect;

	public ResetBufferEffectController(ResetBufferEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ResetBufferEffect(aPerkEffectDefinition as ResetBufferEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		ResetBufferEffect.BufferModule.Buffer = ResetBufferEffect.ResetBufferEffectDefinition.ResetValue ?? ResetBufferEffect.BufferModule.BufferModuleDefinition.DefaultBufferValue;
	}
}
