using System.Xml.Linq;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class SetPhasePhaseActionDefinition : ABossPhaseActionDefinition
{
	public string PhaseId { get; private set; }

	public SetPhasePhaseActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Id");
		PhaseId = xAttribute.Value;
	}
}
