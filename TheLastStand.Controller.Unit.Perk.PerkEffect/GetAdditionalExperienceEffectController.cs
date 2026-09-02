using TPLib;
using TheLastStand.Definition.Unit.Perk.PerkEffect;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Controller.Unit.Perk.PerkEffect;

public class GetAdditionalExperienceEffectController : APerkEffectController
{
	public GetAdditionalExperienceEffect GetAdditionalExperienceEffect => base.PerkEffect as GetAdditionalExperienceEffect;

	public GetAdditionalExperienceEffectController(GetAdditionalExperienceEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
		: base(aPerkEffectDefinition, aPerkModule)
	{
	}

	protected override APerkEffect CreateModel(APerkEffectDefinition aPerkEffectDefinition, APerkModule aPerkModule)
	{
		return new GetAdditionalExperienceEffect(aPerkEffectDefinition as GetAdditionalExperienceEffectDefinition, this, aPerkModule);
	}

	public override void Trigger(PerkDataContainer data)
	{
		base.Trigger(data);
		TPSingleton<PlayableUnitManager>.Instance.ShouldClearUndoStack = true;
		float num = GetAdditionalExperienceEffect.GetAdditionalExperienceEffectDefinition.ValueExpression.EvalToFloat(base.PerkEffect.APerkModule.Perk);
		base.PerkEffect.APerkModule.Perk.Owner.AdditionalNightExperience += num;
	}
}
