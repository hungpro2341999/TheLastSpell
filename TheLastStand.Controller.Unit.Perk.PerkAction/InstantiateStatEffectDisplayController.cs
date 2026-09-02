using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class InstantiateStatEffectDisplayController : InstantiateEffectDisplayController
{
	public InstantiateStatEffectDisplay InstantiateStatEffectDisplay => PerkAction as InstantiateStatEffectDisplay;

	public InstantiateStatEffectDisplayController(APerkActionDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new InstantiateStatEffectDisplay(definition as InstantiateStatEffectDisplayDefinition, this, pEvent);
	}

	protected override AppearingEffectDisplay AddEffectDisplay(int value)
	{
		AttributeOffsetDisplay component = ResourcePooler.LoadOnce<AttributeOffsetDisplay>("Prefab/Displayable Effect/UI Effect Displays/AttributeOffsetDisplay");
		AttributeOffsetDisplay pooledComponent = ObjectPooler.GetPooledComponent("AttributeOffsetDisplay", component, EffectManager.EffectDisplaysParent);
		pooledComponent.Init(InstantiateStatEffectDisplay.InstantiateStatEffectDisplayDefinition.Stat, value);
		return pooledComponent;
	}
}
