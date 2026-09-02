using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class InstantiateRestoreEffectDisplayController : InstantiateEffectDisplayController
{
	public InstantiateRestoreEffectDisplay InstantiateRestoreEffectDisplay => PerkAction as InstantiateRestoreEffectDisplay;

	public InstantiateRestoreEffectDisplayController(APerkActionDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new InstantiateRestoreEffectDisplay(definition as InstantiateRestoreEffectDisplayDefinition, this, pEvent);
	}

	protected override AppearingEffectDisplay AddEffectDisplay(int value)
	{
		RestoreStatDisplay component = ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay");
		RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent("RestoreStatDisplay", component, EffectManager.EffectDisplaysParent);
		pooledComponent.Init(InstantiateRestoreEffectDisplay.InstantiateRestoreEffectDisplayDefinition.Stat, value);
		return pooledComponent;
	}
}
