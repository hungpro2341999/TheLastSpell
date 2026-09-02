using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Definition.Modding.ModuleConfig.Perks;

public class PerksModuleConfigDefinition : ModuleConfigDefinition
{
	public new static class Constants
	{
		public static class ElementName
		{
			public const string PerkDefinitions = "PerkDefinitions";

			public const string UnitPerkCollectionDefinitions = "UnitPerkCollectionDefinitions";

			public const string UnitPerkCollectionSetDefinitions = "UnitPerkCollectionSetDefinitions";

			public const string PerkIconsFolderRelativePath = "PerkIconsFolderRelativePath";

			public const string RelativePath = "RelativePath";
		}
	}

	private readonly List<string> perkDefinitionsRelativePaths = new List<string>();

	private readonly List<string> perkCollectionsRelativePaths = new List<string>();

	private readonly List<string> perkCollectionsSetsRelativePaths = new List<string>();

	private string perkIconsFolderRelativePaths = string.Empty;

	public List<ModdedPerksDefinition> ModdedPerksDefinitions { get; private set; } = new List<ModdedPerksDefinition>();

	public List<ModdedPerkCollectionsDefinition> ModdedPerkCollectionsDefinitions { get; private set; } = new List<ModdedPerkCollectionsDefinition>();

	public List<ModdedPerkCollectionsSetsDefinition> ModdedPerkCollectionsSetsDefinitions { get; private set; } = new List<ModdedPerkCollectionsSetsDefinition>();

	public PerksModuleConfigDefinition(XContainer container, DirectoryInfo directory)
		: base(container)
	{
		TryDeserializeFiles(directory);
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XElement xElement = obj.Element("PerkDefinitions");
		if (xElement != null)
		{
			IEnumerable<string> enumerable = from xE in xElement.Elements("RelativePath")
				select xE.Value;
			if (enumerable.Any())
			{
				perkDefinitionsRelativePaths.AddRange(enumerable);
			}
			else
			{
				CLoggerManager.Log("RelativePath Element was missing in PerkDefinitions child nodes. At least one is needed!", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
			}
		}
		XElement xElement2 = obj.Element("UnitPerkCollectionDefinitions");
		if (xElement2 != null)
		{
			IEnumerable<string> enumerable2 = from xE in xElement2.Elements("RelativePath")
				select xE.Value;
			if (enumerable2.Any())
			{
				perkCollectionsRelativePaths.AddRange(enumerable2);
			}
			else
			{
				CLoggerManager.Log("RelativePath Element was missing in UnitPerkCollectionDefinitions child nodes. At least one is needed!", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
			}
		}
		XElement xElement3 = obj.Element("UnitPerkCollectionSetDefinitions");
		if (xElement3 != null)
		{
			IEnumerable<string> enumerable3 = from xE in xElement3.Elements("RelativePath")
				select xE.Value;
			if (enumerable3.Any())
			{
				perkCollectionsSetsRelativePaths.AddRange(enumerable3);
			}
			else
			{
				CLoggerManager.Log("RelativePath Element was missing in UnitPerkCollectionSetDefinitions child nodes. At least one is needed!", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
			}
		}
		XElement xElement4 = obj.Element("PerkIconsFolderRelativePath");
		if (xElement4 != null && xElement4.Value != null)
		{
			perkIconsFolderRelativePaths = xElement4.Value;
		}
	}

	public void TryDeserializeFiles(DirectoryInfo directory)
	{
		if (perkDefinitionsRelativePaths.Count > 0)
		{
			foreach (string perkDefinitionsRelativePath in perkDefinitionsRelativePaths)
			{
				string text = Path.Combine(directory.FullName, perkDefinitionsRelativePath);
				if (TryGetXDocument(text, out var xDocument))
				{
					ModdedPerksDefinition moddedPerksDefinition = new ModdedPerksDefinition(xDocument);
					foreach (KeyValuePair<string, PerkDefinition> perkDefinition in moddedPerksDefinition.PerkDefinitions)
					{
						perkDefinition.Value.ModdingData = new PerkDefinition.ModdedPerkDefinitionData(Path.Combine(directory.FullName, perkIconsFolderRelativePaths), perkDefinition.Key + ".png");
					}
					ModdedPerksDefinitions.Add(moddedPerksDefinition);
				}
				else
				{
					CLoggerManager.Log("Could not find " + text + ", skipping.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
				}
			}
		}
		if (perkCollectionsRelativePaths.Count > 0)
		{
			foreach (string perkCollectionsRelativePath in perkCollectionsRelativePaths)
			{
				string text2 = Path.Combine(directory.FullName, perkCollectionsRelativePath);
				if (TryGetXDocument(text2, out var xDocument2))
				{
					ModdedPerkCollectionsDefinitions.Add(new ModdedPerkCollectionsDefinition(xDocument2));
				}
				else
				{
					CLoggerManager.Log("Could not find " + text2 + ", skipping.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
				}
			}
		}
		if (perkCollectionsSetsRelativePaths.Count <= 0)
		{
			return;
		}
		foreach (string perkCollectionsSetsRelativePath in perkCollectionsSetsRelativePaths)
		{
			string text3 = Path.Combine(directory.FullName, perkCollectionsSetsRelativePath);
			if (TryGetXDocument(text3, out var xDocument3))
			{
				ModdedPerkCollectionsSetsDefinitions.Add(new ModdedPerkCollectionsSetsDefinition(xDocument3));
			}
			else
			{
				CLoggerManager.Log("Could not find " + text3 + ", skipping.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
			}
		}
	}

	public bool TryGetXDocument(string filePath, out XDocument xDocument)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		if (fileInfo.Exists)
		{
			string text = string.Empty;
			using (StreamReader streamReader = fileInfo.OpenText())
			{
				text = streamReader.ReadToEnd();
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					xDocument = XDocument.Parse(text, LoadOptions.SetBaseUri);
					return true;
				}
				catch (Exception arg)
				{
					CLoggerManager.Log($"Can't Read this config file : {fileInfo.FullName} ! (Exception : {arg})", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ModManager");
				}
			}
		}
		xDocument = null;
		return false;
	}
}
