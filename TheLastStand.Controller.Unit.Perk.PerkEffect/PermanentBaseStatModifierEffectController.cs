using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class PermanentBaseStatModifierEffectController : APerkEffectController
{
	public PermanentBaseStatModifierEffect PermanentBaseStatModifierEffect => base.PerkEffect as PermanentBaseStatModifierEffect;

	public PermanentBaseStatModifierEffectController(PermanentBaseStatModifierEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new PermanentBaseStatModifierEffect(aPerkEffectDefinition as PermanentBaseStatModifierEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		UnitStatDefinition.E_Stat stat = PermanentBaseStatModifierEffect.PermanentBaseStatModifierEffectDefinition.Stat;
		PlayableUnit owner = base.PerkEffect.APerkModule.Perk.Owner;
		float value = PermanentBaseStatModifierEffect.Value;
		if (value > 0f)
		{
			owner.PlayableUnitStatsController.IncreaseBaseStat(stat, value, PermanentBaseStatModifierEffect.PermanentBaseStatModifierEffectDefinition.ChildStatFollows);
		}
		else if (value < 0f)
		{
			owner.PlayableUnitStatsController.DecreaseBaseStat(stat, 0f - value, PermanentBaseStatModifierEffect.PermanentBaseStatModifierEffectDefinition.ChildStatFollows);
		}
	}
}
