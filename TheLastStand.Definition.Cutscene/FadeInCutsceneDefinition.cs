using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Cutscene;

public class FadeInCutsceneDefinition : AFadeCutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "FadeIn";
	}

	public FadeInCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
