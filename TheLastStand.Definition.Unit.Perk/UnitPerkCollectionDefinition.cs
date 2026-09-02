using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class UnitPerkCollectionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string MiscPerkCollectionId = "Misc";
	}

	public string Id { get; private set; }

	public bool MultipleAllowed { get; private set; }

	public bool DoesCollectionRerollsCompletely { get; private set; }

	public Dictionary<int, List<Tuple<PerkDefinition, int>>> PerksFromTier { get; private set; }

	public UnitPerkCollectionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		DoesCollectionRerollsCompletely = true;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("MultipleAllowed");
		if (bool.TryParse(xAttribute2.Value, out var result))
		{
			MultipleAllowed = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse MultipleAllowed attribute into an int in Perk Collection \"" + Id + "\" : \"" + xAttribute2.Value + "\".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
		}
		PerksFromTier = new Dictionary<int, List<Tuple<PerkDefinition, int>>>();
		foreach (XElement item2 in xElement.Elements("UnitPerkTierDefinition"))
		{
			XAttribute xAttribute3 = item2.Attribute("Tier");
			if (!int.TryParse(xAttribute3.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse Tier attribute into an int in Perk Collection \"" + Id + "\" : \"" + xAttribute3.Value + "\". Skip.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
				continue;
			}
			if (PerksFromTier.ContainsKey(result2))
			{
				CLoggerManager.Log($"Tier \"{result2}\" already exists in Perk Collection \"{Id}\". Skip.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
				continue;
			}
			List<Tuple<PerkDefinition, int>> list = new List<Tuple<PerkDefinition, int>>();
			foreach (XElement item3 in item2.Elements("UnitPerkDefinition"))
			{
				XAttribute xAttribute4 = item3.Attribute("Id");
				if (!PlayableUnitDatabase.PerkDefinitions.TryGetValue(xAttribute4.Value, out var value))
				{
					CLoggerManager.Log("Perk Id \"" + xAttribute4.Value + "\" in Perk Collection \"" + Id + "\" doesn't exist in the database. Skip.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
					continue;
				}
				int item = 1;
				XAttribute xAttribute5 = item3.Attribute("Weight");
				if (xAttribute5 != null)
				{
					if (!int.TryParse(xAttribute5.Value, out var result3))
					{
						CLoggerManager.Log("Weight attribute couldn't be parsed into an int for perk \"" + xAttribute4.Value + "\" in Perk Collection \"" + Id + "\". Set it to 1.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PlayableUnitManager");
					}
					else
					{
						item = result3;
					}
				}
				list.Add(new Tuple<PerkDefinition, int>(value, item));
			}
			if (DoesCollectionRerollsCompletely && list.Count > 1)
			{
				DoesCollectionRerollsCompletely = false;
			}
			PerksFromTier.Add(result2, list);
		}
	}
}
