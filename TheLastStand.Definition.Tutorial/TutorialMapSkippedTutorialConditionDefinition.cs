using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Tutorial;

public class TutorialMapSkippedTutorialConditionDefinition : TutorialConditionDefinition
{
	public static class Constants
	{
		public const string Name = "TutorialMapSkipped";
	}

	public TutorialMapSkippedTutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
