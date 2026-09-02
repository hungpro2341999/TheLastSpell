using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkCondition;

public abstract class APerkConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	protected APerkConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}
}
