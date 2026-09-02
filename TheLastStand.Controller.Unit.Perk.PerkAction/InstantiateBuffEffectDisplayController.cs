using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class InstantiateBuffEffectDisplayController : InstantiateEffectDisplayController
{
	public InstantiateBuffEffectDisplay InstantiateBuffEffectDisplay => PerkAction as InstantiateBuffEffectDisplay;

	public InstantiateBuffEffectDisplayController(APerkActionDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new InstantiateBuffEffectDisplay(definition as InstantiateBuffEffectDisplayDefinition, this, pEvent);
	}

	protected override AppearingEffectDisplay AddEffectDisplay(int value)
	{
		BuffDisplay component = ResourcePooler.LoadOnce<BuffDisplay>("Prefab/Displayable Effect/UI Effect Displays/BuffDisplay");
		BuffDisplay pooledComponent = ObjectPooler.GetPooledComponent("BuffDisplay", component, EffectManager.EffectDisplaysParent);
		pooledComponent.Init(InstantiateBuffEffectDisplay.InstantiateBuffEffectDisplayDefinition.Stat, value);
		return pooledComponent;
	}
}
