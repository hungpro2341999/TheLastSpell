using System.Xml.Linq;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class FriendlyFireTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "FriendlyFire";

	public FriendlyFireTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		base.Value = 0;
	}

	public override string ToString()
	{
		return "FriendlyFire";
	}
}
