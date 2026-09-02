using TPLib;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class ModifyDefensesDamageEffectController : APerkEffectController
{
	public ModifyDefensesDamageEffect ModifyDefensesDamageEffect => base.PerkEffect as ModifyDefensesDamageEffect;

	public ModifyDefensesDamageEffectController(ModifyDefensesDamageEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new ModifyDefensesDamageEffect(aPerkEffectDefinition as ModifyDefensesDamageEffectDefinition, this, aPerkModule);
	}

	public override void OnUnlock(bool onLoad)
	{
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<BuildingManager>.Instance.Buildings[num].BattleModule != null)
			{
				TPSingleton<BuildingManager>.Instance.Buildings[num].BattleModule.ModifyDefensesDamagePerks.Add(ModifyDefensesDamageEffect);
			}
		}
	}

	public override void Lock(bool onLoad)
	{
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<BuildingManager>.Instance.Buildings[num].BattleModule != null)
			{
				TPSingleton<BuildingManager>.Instance.Buildings[num].BattleModule.ModifyDefensesDamagePerks.Remove(ModifyDefensesDamageEffect);
			}
		}
	}
}
