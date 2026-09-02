using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Database;

public class TooltipDatabase : Database<TooltipDatabase>
{
	[SerializeField]
	private TextAsset compendiumDefinitionTextAsset;

	public static CompendiumDefinition CompendiumDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement xElement = XDocument.Parse(compendiumDefinitionTextAsset.text, LoadOptions.SetBaseUri).Element("CompendiumDefinition");
		if (xElement.IsNullOrEmpty())
		{
			CLoggerManager.Log("The document " + compendiumDefinitionTextAsset.name + " must have an CompendiumDefinition!", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "TooltipDatabase");
		}
		else
		{
			CompendiumDefinition = new CompendiumDefinition(xElement);
		}
	}

	public HashSet<ACompendiumEntryDefinition> GetLinkedEntryDefinitions(string id)
	{
		if (!CompendiumDefinition.CompendiumEntryDefinitions.TryGetValue(id, out var value))
		{
			CLoggerManager.Log("Compendium entry \"" + id + "\" wasn't found in the database.");
			return null;
		}
		return GetLinkedEntryDefinitions(value);
	}

	public HashSet<ACompendiumEntryDefinition> GetLinkedEntryDefinitions(ACompendiumEntryDefinition entryDefinition)
	{
		HashSet<ACompendiumEntryDefinition> hashSet = new HashSet<ACompendiumEntryDefinition>();
		foreach (string linkedEntry in entryDefinition.LinkedEntries)
		{
			if (!CompendiumDefinition.CompendiumEntryDefinitions.TryGetValue(linkedEntry, out var value))
			{
				CLoggerManager.Log("Linked compendium entry \"" + linkedEntry + "\" wasn't found in the database.");
			}
			else
			{
				hashSet.Add(value);
			}
		}
		return hashSet;
	}
}
