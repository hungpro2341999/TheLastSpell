using System.Xml.Linq;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public abstract class ValueIntHeroesTrophyConditionDefinition : HeroesTrophyConditionDefinition
{
	public override object[] DescriptionLocalizationParameters => new object[1] { Value };

	public int Value { get; protected set; }

	protected ValueIntHeroesTrophyConditionDefinition(XContainer container)
		: base(container)
	{
	}
}
