using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public abstract class APerkActionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public APerkActionDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}
}
