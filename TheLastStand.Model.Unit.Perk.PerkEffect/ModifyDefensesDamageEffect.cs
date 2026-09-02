using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class ModifyDefensesDamageEffect : APerkEffect
{
	public ModifyDefensesDamageEffectDefinition ModifyDefensesDamageEffectDefinition => base.APerkEffectDefinition as ModifyDefensesDamageEffectDefinition;

	public ModifyDefensesDamageEffect(ModifyDefensesDamageEffectDefinition aPerkEffectDefinition, ModifyDefensesDamageEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
