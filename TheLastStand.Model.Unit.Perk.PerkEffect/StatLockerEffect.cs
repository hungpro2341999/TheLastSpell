using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class StatLockerEffect : APerkEffect
{
	public StatLockerEffectDefinition StatLockerEffectDefinition => base.APerkEffectDefinition as StatLockerEffectDefinition;

	public StatLockerEffect(StatLockerEffectDefinition aPerkEffectDefinition, StatLockerEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
