using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class RefreshPerkActivationFeedbackController : APerkActionController
{
	public RefreshPerkActivationFeedback RefreshPerkActivationFeedback => PerkAction as RefreshPerkActivationFeedback;

	public RefreshPerkActivationFeedbackController(APerkActionDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new RefreshPerkActivationFeedback(definition as RefreshPerkActivationFeedbackDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		bool flag = PerkAction.PerkEvent.PerkModule.Perk.FeedbackActivationConditions.IsValid(data);
		if (PerkAction.PerkEvent.PerkModule.WasActiveOnLastRefresh != flag)
		{
			if (RefreshPerkActivationFeedback.RefreshPerkActivationFeedbackDefinition.RefreshView)
			{
				PlayableUnit owner = PerkAction.PerkEvent.PerkModule.Perk.Owner;
				PerkActivationDisplay component = ResourcePooler.LoadOnce<PerkActivationDisplay>("Prefab/Displayable Effect/UI Effect Displays/PerkActivationDisplay");
				PerkActivationDisplay pooledComponent = ObjectPooler.GetPooledComponent("PerkActivationDisplay", component, EffectManager.EffectDisplaysParent);
				pooledComponent.Init(PerkAction.PerkEvent.PerkModule.Perk.PerkDefinition.Name, flag);
				owner.UnitController.AddEffectDisplay(pooledComponent);
				owner.UnitController.DisplayEffects();
			}
			PerkAction.PerkEvent.PerkModule.WasActiveOnLastRefresh = flag;
		}
	}
}
