using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;

namespace TheLastStand.Definition.Building.BuildingPassive.PassiveTrigger;

public abstract class PassiveTriggerDefinition : TheLastStand.Framework.Serialization.Definition
{
	public E_EffectTime EffectTime { get; protected set; }

	public PassiveTriggerDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
