using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class EnemiesKilledFromWatchtowerTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "EnemiesKilledFromWatchtower";

	public EnemiesKilledFromWatchtowerTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : EnemiesKilledFromWatchtower in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "EnemiesKilledFromWatchtower";
	}
}
