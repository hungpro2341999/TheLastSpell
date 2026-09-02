using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public abstract class TargetingMethodDefinition : TheLastStand.Framework.Serialization.Definition
{
	public TargetingMethodDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
