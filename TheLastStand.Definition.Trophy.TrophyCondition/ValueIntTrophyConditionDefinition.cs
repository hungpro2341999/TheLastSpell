using System.Xml.Linq;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public abstract class ValueIntTrophyConditionDefinition : TrophyConditionDefinition
{
	public override object[] DescriptionLocalizationParameters => new object[1] { Value };

	public int Value { get; protected set; }

	protected ValueIntTrophyConditionDefinition(XContainer container)
		: base(container)
	{
	}
}
