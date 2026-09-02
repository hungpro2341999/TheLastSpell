using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class TriggerEffectsOnAllAttackDataController : APerkActionController
{
	public TriggerEffectsOnAllAttackDataController(TriggerEffectsOnAllAttackDataDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new TriggerEffectsOnAllAttackData(definition as TriggerEffectsOnAllAttackDataDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		if (data.AllAttackData == null)
		{
			return;
		}
		foreach (AttackSkillActionExecutionTileData allAttackDatum in data.AllAttackData)
		{
			data.TargetDamageable = allAttackDatum.Damageable;
			data.AttackData = allAttackDatum;
			PerkAction.PerkEvent.PerkModule.TryTriggerEffects(data);
		}
	}
}
