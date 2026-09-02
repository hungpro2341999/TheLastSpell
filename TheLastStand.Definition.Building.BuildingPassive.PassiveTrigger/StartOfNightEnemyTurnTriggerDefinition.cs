using System.Xml.Linq;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class StartOfNightEnemyTurnTriggerDefinition : PassiveTriggerDefinition
{
	public StartOfNightEnemyTurnTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnStartNightTurnEnemy;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
