using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class BuffEffectDefinition : StatModifierEffectDefinition
{
	public static class Constants
	{
		public const string Id = "Buff";
	}

	public override string Id => "Buff";

	public override Status.E_StatusType StatusType => Status.E_StatusType.Buff;

	public BuffEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		base.ModifierValueExpression = Parser.Parse(xElement.Element("Bonus")?.Value ?? "0", base.TokenVariables);
	}
}
