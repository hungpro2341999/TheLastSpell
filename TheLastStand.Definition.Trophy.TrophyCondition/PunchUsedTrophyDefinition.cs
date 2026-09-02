using System.Xml.Linq;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class PunchUsedTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "PunchUsed";

	public PunchUsedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPSingleton<TrophyManager>.Instance.LogError("The Value of an Element : PunchUsed in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "PunchUsed";
	}
}
