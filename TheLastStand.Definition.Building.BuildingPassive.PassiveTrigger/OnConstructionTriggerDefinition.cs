using System.Xml.Linq;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class OnConstructionTriggerDefinition : PassiveTriggerDefinition
{
	public OnConstructionTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnCreation;
	}
}
