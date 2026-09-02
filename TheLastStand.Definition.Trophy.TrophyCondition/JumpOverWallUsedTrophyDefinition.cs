using System.Xml.Linq;
using TPLib;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class JumpOverWallUsedTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "JumpOverWallUsed";

	public JumpOverWallUsedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPSingleton<TrophyManager>.Instance.LogError("The Value of an Element : JumpOverWallUsed int TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "JumpOverWallUsed";
	}
}
