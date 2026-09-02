using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Model.Unit.Stat;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class StatLockerEffectController : APerkEffectController
{
	public StatLockerEffect StatLockerEffect => base.PerkEffect as StatLockerEffect;

	public StatLockerEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new StatLockerEffect(aPerkEffectDefinition as StatLockerEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		PlayableUnitStat stat = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(StatLockerEffect.StatLockerEffectDefinition.Stat);
		stat.PerkLocksBuffer++;
		UnitStatDefinition.E_Stat childStatId = UnitDatabase.UnitStatDefinitions[stat.StatId].ChildStatId;
		if (childStatId != UnitStatDefinition.E_Stat.Undefined)
		{
			base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(childStatId).PerkLocksBuffer++;
		}
	}

	public override void Lock(bool onLoad)
	{
		PlayableUnitStat stat = base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(StatLockerEffect.StatLockerEffectDefinition.Stat);
		stat.PerkLocksBuffer--;
		UnitStatDefinition.E_Stat childStatId = UnitDatabase.UnitStatDefinitions[stat.StatId].ChildStatId;
		if (childStatId != UnitStatDefinition.E_Stat.Undefined)
		{
			base.PerkEffect.APerkModule.Perk.Owner.PlayableUnitStatsController.GetStat(childStatId).PerkLocksBuffer--;
		}
	}
}
