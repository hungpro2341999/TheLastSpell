using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class ResetBufferEffect : APerkEffect
{
	public BufferModule BufferModule => base.APerkModule as BufferModule;

	public ResetBufferEffectDefinition ResetBufferEffectDefinition => base.APerkEffectDefinition as ResetBufferEffectDefinition;

	public ResetBufferEffect(ResetBufferEffectDefinition aPerkEffectDefinition, ResetBufferEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
