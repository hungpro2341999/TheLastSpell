using TheLastStand.Controller.Unit.Perk.PerkEffect;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkEffect;

public class EquipmentSlotModifierEffect : APerkEffect
{
	public EquipmentSlotModifierEffectDefinition EquipmentSlotModifierEffectDefinition => base.APerkEffectDefinition as EquipmentSlotModifierEffectDefinition;

	public EquipmentSlotModifierEffect(EquipmentSlotModifierEffectDefinition aPerkEffectDefinition, EquipmentSlotModifierEffectController aPerkEffectController, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkEffectController, aPerkModule)
	{
	}
}
