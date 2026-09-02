using System.Xml.Linq;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class OnDeathTriggerDefinition : PassiveTriggerDefinition
{
	public OnDeathTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnDeath;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
