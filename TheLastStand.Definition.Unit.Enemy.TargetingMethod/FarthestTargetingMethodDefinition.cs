using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class FarthestTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "Farthest";

	public FarthestTargetingMethodDefinition(XContainer container = null)
		: base(container)
	{
	}
}
