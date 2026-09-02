using System.Xml.Linq;
using TPLib;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class NightCompletedTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "NightCompleted";

	public string CityId { get; private set; }

	public NightCompletedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("CityId");
		CityId = xAttribute.Value;
		if (!int.TryParse(obj.Value, out var result))
		{
			TPDebug.LogError("The Value of an Element : NightCompleted in TrophiesDefinitions isn't a valid int");
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "NightCompleted";
	}
}
