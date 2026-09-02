using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Modding.ModuleConfig.Perks;

public class ModdedPerksDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<string, PerkDefinition> PerkDefinitions { get; } = new Dictionary<string, PerkDefinition>();

	public ModdedPerksDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		IEnumerable<XElement> enumerable = (container as XDocument).Element("PerkDefinitions")?.Elements("PerkDefinition");
		if (enumerable == null)
		{
			return;
		}
		foreach (XElement item in enumerable)
		{
			PerkDefinition value = new PerkDefinition(item);
			string text = item.Attribute("Id")?.Value;
			if (!string.IsNullOrEmpty(text))
			{
				PerkDefinitions[text] = value;
				PlayableUnitDatabase.PerkDefinitions[text] = value;
			}
			else
			{
				CLoggerManager.Log("Missing or Empty Id attribute for a PerkDefinition Element, skipping.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
			}
		}
	}
}
