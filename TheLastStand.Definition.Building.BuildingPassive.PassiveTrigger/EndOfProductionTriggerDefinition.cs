using System.Xml.Linq;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class EndOfProductionTriggerDefinition : PassiveTriggerDefinition
{
	public EndOfProductionTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnEndProductionTurn;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
