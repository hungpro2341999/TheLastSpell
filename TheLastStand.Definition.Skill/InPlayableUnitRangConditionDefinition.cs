using System.Xml.Linq;

namespace TheLastStand.Definition.Skill;

public class InPlayableUnitRangConditionDefinition : SkillConditionDefinition
{
	public const string InPlayableUnitRangeName = "InPlayableUnitRange";

	public int MaxRange { get; private set; } = 1;

	public override string Name => "InPlayableUnitRange";

	public InPlayableUnitRangConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute xAttribute = (container as XElement).Attribute("MaxRange");
		if (xAttribute != null && int.TryParse(xAttribute.Value, out var result))
		{
			MaxRange = result;
		}
		if (MaxRange <= 0)
		{
			MaxRange = 1;
		}
	}
}
