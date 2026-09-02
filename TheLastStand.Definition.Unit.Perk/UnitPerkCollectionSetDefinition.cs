using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class UnitPerkCollectionSetDefinition : TheLastStand.Framework.Serialization.Definition
{
	public int Index { get; private set; }

	public HashSet<Tuple<string, int, string>> CollectionsPerWeight { get; private set; }

	public UnitPerkCollectionSetDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		if (int.TryParse(obj.Attribute("Index").Value, out var result))
		{
			Index = result - 1;
		}
		else
		{
			CLoggerManager.Log("Index attribute could not be parsed as an int UnitPerkCollectionSetDefinition", TPSingleton<PlayableUnitManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
		}
		CollectionsPerWeight = new HashSet<Tuple<string, int, string>>();
		foreach (XElement item2 in obj.Elements("UnitPerkCollectionDefinition"))
		{
			XAttribute xAttribute = item2.Attribute("Id");
			if (!PlayableUnitDatabase.UnitPerkCollectionDefinitions.TryGetValue(xAttribute.Value, out var value))
			{
				CLoggerManager.Log("Could not find the perk collection \"" + xAttribute.Value + "\" in the database. Skip.", TPSingleton<PlayableUnitManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
				continue;
			}
			XAttribute xAttribute2 = item2.Attribute("Weight");
			if (!int.TryParse(xAttribute2.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse the Weight element into an int : \"" + xAttribute2.Value + "\". Skip.", TPSingleton<PlayableUnitManager>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
				continue;
			}
			XAttribute xAttribute3 = item2.Attribute("RestrictedToRaceId");
			string item = null;
			if (xAttribute3 != null)
			{
				item = xAttribute3.Value;
			}
			CollectionsPerWeight.Add(new Tuple<string, int, string>(value.Id, result2, item));
		}
	}
}
