using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.DLC;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Maths;
using UnityEngine;

namespace TheLastStand.Database;

public class GenericDatabase : Database<GenericDatabase>
{
	[SerializeField]
	private TextAsset filterDefinitionsTextAssets;

	[SerializeField]
	private TextAsset[] idsListsDefinitionsTextAssets;

	public static Dictionary<string, FilterDefinition> FilterDefinitions { get; private set; }

	public static Dictionary<string, IdsListDefinition> IdsListDefinitions { get; private set; }

	public static List<TextAsset> GetDLCTextAssets(DLCTextAssetDefinition[] dlcTextAssetDefinitions, bool forceGetAllTextAssetDefinitions = false)
	{
		List<TextAsset> list = new List<TextAsset>();
		foreach (DLCTextAssetDefinition dLCTextAssetDefinition in dlcTextAssetDefinitions)
		{
			if (!dLCTextAssetDefinition.IsLinkedToDLC || dLCTextAssetDefinition.IsDLCOwned() || forceGetAllTextAssetDefinitions)
			{
				list.Add(dLCTextAssetDefinition.TextAsset);
			}
		}
		return list;
	}

	public static List<IdsListDefinition> GetIdListDefinitionForEntity(string entityId)
	{
		List<IdsListDefinition> list = new List<IdsListDefinition>();
		foreach (IdsListDefinition value in IdsListDefinitions.Values)
		{
			if (value.Ids.Contains(entityId))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public static List<string> GetIdListIdsForEntity(string entityId)
	{
		return (from x in GetIdListDefinitionForEntity(entityId)
			select x.Id).ToList();
	}

	public static bool TryGetIdListDefinitionForEntity(string entityId, out List<IdsListDefinition> foundDefinitions)
	{
		foundDefinitions = GetIdListDefinitionForEntity(entityId);
		return foundDefinitions.Count > 0;
	}

	public static bool TryGetIdListIdsForEntity(string entityId, out List<string> foundDefinitions)
	{
		foundDefinitions = GetIdListIdsForEntity(entityId);
		return foundDefinitions.Count > 0;
	}

	public override void Deserialize(XContainer container = null)
	{
		DeserializeIdsListDefinitions();
		DeserializeFiltersDefinitions();
	}

	private void DeserializeFiltersDefinitions()
	{
		FilterDefinitions = new Dictionary<string, FilterDefinition>();
		XElement xElement = XDocument.Parse(filterDefinitionsTextAssets.text, LoadOptions.SetBaseUri).Element("FilterDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document must have FiltersDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item in xElement.Elements("FilterDefinition"))
		{
			item.Attribute("Id");
			FilterDefinition filterDefinition = new FilterDefinition(item);
			if (!string.IsNullOrEmpty(filterDefinition.Id))
			{
				FilterDefinitions.Add(filterDefinition.Id, filterDefinition);
			}
		}
		foreach (FilterDefinition value in FilterDefinitions.Values)
		{
			value.ComputeIncludedFilters();
		}
	}

	private void DeserializeIdsListDefinitions()
	{
		IdsListDefinitions = new Dictionary<string, IdsListDefinition>();
		foreach (XElement item in GatherElements(idsListsDefinitionsTextAssets, null, "IdsListDefinition", "IdsListsDefinitions"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			IdsListDefinitions.Add(xAttribute.Value, new IdsListDefinition(item));
		}
		foreach (IdsListDefinition item2 in TopologicSorter.Sort(IdsListDefinitions.Values).ToList())
		{
			item2.DeserializeAfterDependencySorting();
		}
	}

	[ContextMenu("Log Ids Lists content")]
	private void LogIdsListsContent()
	{
		foreach (KeyValuePair<string, IdsListDefinition> idsListDefinition in IdsListDefinitions)
		{
			CLoggerManager.Log(idsListDefinition);
			idsListDefinition.Value.Ids.ForEach(delegate(string o)
			{
				CLoggerManager.Log(o);
			});
		}
	}
}
