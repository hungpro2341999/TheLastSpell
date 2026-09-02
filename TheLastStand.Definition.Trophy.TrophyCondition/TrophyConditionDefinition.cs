using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public abstract class TrophyConditionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public virtual object[] DescriptionLocalizationParameters { get; }

	public TrophyConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
