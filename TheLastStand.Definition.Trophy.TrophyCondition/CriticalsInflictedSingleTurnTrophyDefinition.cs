using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class CriticalsInflictedSingleTurnTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "CriticalsInflictedSingleTurn";

	public CriticalsInflictedSingleTurnTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : CriticalsInflictedSingleTurn in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "CriticalsInflictedSingleTurn";
	}
}
