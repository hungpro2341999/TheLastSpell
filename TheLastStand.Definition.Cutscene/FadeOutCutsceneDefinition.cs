using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Cutscene;

public class FadeOutCutsceneDefinition : AFadeCutsceneDefinition
{
	public static class Constants
	{
		public const string Id = "FadeOut";
	}

	public FadeOutCutsceneDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}
}
