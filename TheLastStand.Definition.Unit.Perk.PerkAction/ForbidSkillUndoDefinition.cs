using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class ForbidSkillUndoDefinition : APerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "ForbidSkillUndo";
	}

	public ForbidSkillUndoDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
