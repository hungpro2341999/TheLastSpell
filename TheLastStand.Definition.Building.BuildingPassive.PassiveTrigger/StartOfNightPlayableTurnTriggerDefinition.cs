using System.Xml.Linq;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public class StartOfNightPlayableTurnTriggerDefinition : PassiveTriggerDefinition
{
	public StartOfNightPlayableTurnTriggerDefinition(XContainer container)
		: base(container)
	{
		base.EffectTime = E_EffectTime.OnStartNightTurnPlayable;
	}

	public override void Deserialize(XContainer container)
	{
	}
}
