using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class HealthLostTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "HealthLost";

	public HealthLostTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : HealthLost in TrophiesDefinitions isn't a valid float");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "HealthLost";
	}
}
