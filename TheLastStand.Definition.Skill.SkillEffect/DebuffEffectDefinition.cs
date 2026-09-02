using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class DebuffEffectDefinition : StatModifierEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Debuff";
	}

	public override string Id => "Debuff";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Debuff;

	public DebuffEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		AffectedUnits = E_SkillUnitAffect.IgnoreCaster;
		base.Deserialize(container);
		XElement xElement = container as XElement;
		base.ModifierValueExpression = Parser.Parse(xElement.Element("Malus")?.Value ?? "0", base.TokenVariables);
	}
}
