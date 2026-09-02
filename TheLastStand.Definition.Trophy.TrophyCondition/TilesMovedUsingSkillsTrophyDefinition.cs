using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class TilesMovedUsingSkillsTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "TilesMovedUsingSkills";

	public TilesMovedUsingSkillsTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : TilesMovedUsingSkills in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "TilesMovedUsingSkills";
	}
}
