using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class ClosestTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "Closest";

	public ClosestTargetingMethodDefinition(XContainer container = null)
		: base(container)
	{
	}
}
