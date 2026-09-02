using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Definition.Unit.Enemy.TargetingMethod;

public class ScoreTargetingMethodDefinition : TargetingMethodDefinition
{
	public const string Name = "Score";

	public Node Score { get; private set; }

	public ScoreTargetingMethodDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XAttribute xAttribute = (container as XElement).Attribute("Value");
		Score = Parser.Parse(xAttribute.Value);
	}
}
