using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class OpportunismDamageInflictedTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "OpportunismDamageInflicted";

	public OpportunismDamageInflictedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : OpportunismDamageInflicted in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "OpportunismDamageInflicted";
	}
}
