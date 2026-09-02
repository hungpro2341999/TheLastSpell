using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class ActorDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string ActorId { get; private set; }

	public DamageableType ActorType { get; private set; } = DamageableType.Other;

	public TileFlagDefinition.E_TileFlagTag TileFlagTag { get; private set; }

	public string UnitId { get; private set; }

	public ActorDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		ActorId = xAttribute.Value;
		XAttribute xAttribute2 = obj.Attribute("UnitId");
		UnitId = xAttribute2.Value;
		XAttribute xAttribute3 = obj.Attribute("ActorType");
		if (Enum.TryParse<DamageableType>(xAttribute3.Value, out var result))
		{
			ActorType = result;
		}
		else
		{
			CLoggerManager.Log("Unable to parse " + xAttribute3.Value + " into DamageableType", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute4 = obj.Attribute("TileFlag");
		if (xAttribute4 != null)
		{
			if (Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(xAttribute4.Value, out var result2))
			{
				TileFlagTag = result2;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute4.Value + " into E_TileFlagTag", LogType.Error, CLogLevel.MAJOR);
			}
		}
		else
		{
			TileFlagTag = TileFlagDefinition.E_TileFlagTag.None;
		}
	}
}
