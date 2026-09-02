using System.Collections.Generic;
using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class PlayCutscenePhaseActionDefinition : ABossPhaseActionDefinition
{
	public string CutsceneId { get; private set; }

	public PlayCutscenePhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Id");
		CutsceneId = xAttribute.Value;
	}
}
