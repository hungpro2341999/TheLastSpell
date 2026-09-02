using System.Xml.Linq;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class OnExtinguishTriggerDefinition : PassiveTriggerDefinition
{
	public OnExtinguishTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnExtinguish;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
