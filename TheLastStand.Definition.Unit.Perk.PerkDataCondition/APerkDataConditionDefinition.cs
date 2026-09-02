using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkDataCondition;

public abstract class APerkDataConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	protected APerkDataConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}
}
