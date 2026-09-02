using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class UpgradeCityMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "UpgradeCity";

	public string CityId { get; private set; }

	public int Level { get; private set; }

	public UpgradeCityMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XElement xElement = obj.Element("Id");
		CityId = xElement.Value;
		if (!int.TryParse(obj.Element("Level").Value, out var result))
		{
			CLoggerManager.Log("Could not cast the level value into an int !", LogType.Error);
		}
		else
		{
			Level = result;
		}
	}
}
