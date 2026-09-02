using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class NightCompletedXTurnsAfterSpawnEndTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "NightCompletedXTurnsAfterSpawnEnd";

	public NightCompletedXTurnsAfterSpawnEndTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : NightCompletedXTurnsAfterSpawnEnd in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "NightCompletedXTurnsAfterSpawnEnd";
	}
}
