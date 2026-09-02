using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class RunCompletedInCityConditionDefinition : ApocalypseUnlockConditionDefinition
{
	public const string RunCompletedInCityName = "RunCompletedInCity";

	public string CityDefinitionId { get; private set; }

	public override string Name => "RunCompletedInCity";

	public int RunCompleted { get; private set; }

	public RunCompletedInCityConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		XAttribute xAttribute2 = obj.Attribute("Value");
		if (!int.TryParse(xAttribute2.Value, out var result))
		{
			Debug.LogError("RunCompletedInCity " + xAttribute2.Value + " " + HasAnInvalidInt(xAttribute2.Value));
		}
		CityDefinitionId = xAttribute.Value;
		RunCompleted = result;
	}
}
