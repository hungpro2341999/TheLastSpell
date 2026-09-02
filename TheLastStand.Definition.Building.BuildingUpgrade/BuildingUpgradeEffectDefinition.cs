using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class BuildingUpgradeEffectDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public BuildingUpgradeEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XAttribute xAttribute = (xContainer as XElement).Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("BuildingUpgradeEffectDefinition must have an Id", LogType.Error);
		}
		else
		{
			Id = xAttribute.Value;
		}
	}
}
