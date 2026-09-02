using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ComputationStatLockerEffectController : APerkEffectController
{
	public ComputationStatLockerEffect ComputationStatLockerEffect => base.PerkEffect as ComputationStatLockerEffect;

	public ComputationStatLockerEffectController(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ComputationStatLockerEffect(aPerkEffectDefinition as ComputationStatLockerEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		PlayableUnit owner = base.PerkEffect.APerkModule.Perk.Owner;
		TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat = ComputationStatLockerEffect.ComputationStatLockerEffectDefinition.ComputationStat;
		if (owner.PerkComputationStatsLocksBuffer.ContainsKey(computationStat) && !owner.PerkComputationStatsLocksBuffer[computationStat].Contains(ComputationStatLockerEffect))
		{
			owner.PerkComputationStatsLocksBuffer[computationStat].Add(ComputationStatLockerEffect);
			return;
		}
		owner.PerkComputationStatsLocksBuffer.Add(computationStat, new List<ComputationStatLockerEffect> { ComputationStatLockerEffect });
	}

	public override void Lock(bool onLoad)
	{
		PlayableUnit owner = base.PerkEffect.APerkModule.Perk.Owner;
		TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat = ComputationStatLockerEffect.ComputationStatLockerEffectDefinition.ComputationStat;
		if (owner.PerkComputationStatsLocksBuffer.ContainsKey(computationStat) && owner.PerkComputationStatsLocksBuffer[computationStat].Contains(ComputationStatLockerEffect))
		{
			owner.PerkComputationStatsLocksBuffer[computationStat].Remove(ComputationStatLockerEffect);
		}
		else
		{
			TPSingleton<PlayableUnitManager>.Instance.LogWarning($"Tried to unlock a computation stat that didn't register any locks. that's weird ... computation stat : {computationStat} ; perk : {base.PerkEffect.APerkModule.Perk.PerkDefinition.Id}", CLogLevel.MAJOR);
		}
	}
}
