using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using UnityEngine;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class RestoreUsesEffectController : APerkEffectController
{
	public RestoreUsesEffect RestoreUsesEffect => base.PerkEffect as RestoreUsesEffect;

	public RestoreUsesEffectController(RestoreUsesEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new RestoreUsesEffect(aPerkEffectDefinition as RestoreUsesEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		if (!base.PerkEffect.APerkModule.Perk.Owner.EquipmentSlots.TryGetValue(RestoreUsesEffect.RestoreUsablesUsesEffectDefinition.SlotType, out var value))
		{
			return;
		}
		foreach (EquipmentSlot item in value)
		{
			if (item.Item == null)
			{
				continue;
			}
			foreach (TheLastStand.Model.Skill.Skill skill in item.Item.Skills)
			{
				if (skill.OverallUsesRemaining != -1)
				{
					skill.OverallUsesRemaining = Mathf.Min(skill.ComputeTotalUses(), skill.OverallUsesRemaining + RestoreUsesEffect.Value);
				}
			}
		}
	}
}
