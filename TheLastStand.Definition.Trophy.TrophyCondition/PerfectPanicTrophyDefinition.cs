using System.Xml.Linq;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class PerfectPanicTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "PerfectPanic";

	public PerfectPanicTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPSingleton<TrophyManager>.Instance.LogError("The Value of an Element: PerfectPanic in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "PerfectPanic";
	}
}
