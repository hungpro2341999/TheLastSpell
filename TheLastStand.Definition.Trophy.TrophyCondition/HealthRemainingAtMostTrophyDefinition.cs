using System.Xml.Linq;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class HealthRemainingAtMostTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "HealthRemainingAtMost";

	public HealthRemainingAtMostTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPSingleton<TrophyManager>.Instance.LogError("The Value of an Element : HealthRemainingAtMost in TrophiesDefinitions isn't a valid float");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "HealthRemainingAtMost";
	}
}
