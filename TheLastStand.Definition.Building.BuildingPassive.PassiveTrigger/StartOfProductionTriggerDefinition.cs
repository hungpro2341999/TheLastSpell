using System.Xml.Linq;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class StartOfProductionTriggerDefinition : PassiveTriggerDefinition
{
	public StartOfProductionTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnStartProductionTurn;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
