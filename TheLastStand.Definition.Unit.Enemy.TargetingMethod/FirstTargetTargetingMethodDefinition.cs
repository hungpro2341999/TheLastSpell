using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class FirstTargetTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "FirstTarget";

	public FirstTargetTargetingMethodDefinition(XContainer container = null)
		: base(container)
	{
	}
}
