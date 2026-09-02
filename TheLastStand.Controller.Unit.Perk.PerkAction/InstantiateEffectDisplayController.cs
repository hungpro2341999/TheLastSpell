using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public abstract class InstantiateEffectDisplayController : APerkActionController
{
	public InstantiateEffectDisplay InstantiateEffectDisplay => PerkAction as InstantiateEffectDisplay;

	public InstantiateEffectDisplayController(APerkActionDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	public override void Trigger(PerkDataContainer data)
	{
		int num = (int)InstantiateEffectDisplay.InstantiateEffectDisplayDefinition.ValueExpression.EvalToFloat(PerkAction.PerkEvent.PerkModule.Perk);
		if (num != 0)
		{
			PlayableUnit owner = PerkAction.PerkEvent.PerkModule.Perk.Owner;
			owner.UnitController.AddEffectDisplay(AddEffectDisplay(num));
			owner.UnitController.DisplayEffects();
		}
	}

	protected abstract AppearingEffectDisplay AddEffectDisplay(int value);
}
