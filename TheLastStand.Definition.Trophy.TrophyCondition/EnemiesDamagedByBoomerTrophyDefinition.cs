using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class EnemiesDamagedByBoomerTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "EnemiesDamagedByBoomer";

	public EnemiesDamagedByBoomerTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : EnemiesDamagedByBoomer in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "EnemiesDamagedByBoomer";
	}
}
