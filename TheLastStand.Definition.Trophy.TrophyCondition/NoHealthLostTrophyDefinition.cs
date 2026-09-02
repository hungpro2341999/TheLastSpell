using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class NoHealthLostTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "NoHealthLost";

	public NoHealthLostTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse((container as XElement).Attribute("NightToExceed").Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : NoHealthLost in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "NoHealthLost";
	}
}
