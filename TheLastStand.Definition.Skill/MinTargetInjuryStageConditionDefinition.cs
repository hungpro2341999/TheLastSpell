using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Skill;

public class MinTargetInjuryStageConditionDefinition : SkillConditionDefinition
{
	public const string MinTargetInjuryStageName = "MinTargetInjuryStage";

	public Node RequiredInjuryStage { get; private set; }

	public override string Name => "MinTargetInjuryStage";

	public MinTargetInjuryStageConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		RequiredInjuryStage = Parser.Parse(xElement.Value);
	}
}
