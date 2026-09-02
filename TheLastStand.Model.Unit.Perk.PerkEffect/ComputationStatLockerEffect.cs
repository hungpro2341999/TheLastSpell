using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class ComputationStatLockerEffect : APerkEffect
{
	public ComputationStatLockerEffectDefinition ComputationStatLockerEffectDefinition => base.APerkEffectDefinition as ComputationStatLockerEffectDefinition;

	public ComputationStatLockerEffect(ComputationStatLockerEffectDefinition aPerkEffectDefinition, ComputationStatLockerEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
