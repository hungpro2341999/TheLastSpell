using System.Xml.Linq;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public abstract class HeroesTrophyConditionDefinition : TrophyConditionDefinition
{
	public string Target { get; protected set; }

	public HeroesTrophyConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("Target");
		Target = xAttribute.Value;
	}
}
