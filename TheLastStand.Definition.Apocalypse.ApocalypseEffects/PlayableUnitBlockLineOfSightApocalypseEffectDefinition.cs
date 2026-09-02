using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class PlayableUnitBlockLineOfSightApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public PlayableUnitBlockLineOfSightApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
