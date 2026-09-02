using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class RandomTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "Random";

	public RandomTargetingMethodDefinition(XContainer container = null)
		: base(container)
	{
	}
}
