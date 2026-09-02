using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.WorldMap;

public class CityDatabase : Database<CityDatabase>
{
	[SerializeField]
	private TextAsset[] cityDefinitionsTextAssets;

	public static Dictionary<string, CityDefinition> CityDefinitions { get; set; }

	public override void Deserialize(XContainer container = null)
	{
		if (CityDefinitions != null)
		{
			return;
		}
		Queue<XElement> elements = GatherElements(cityDefinitionsTextAssets, null, "CityDefinition");
		IEnumerable<XElement> enumerable = SortElementsByDependencies(elements);
		CityDefinitions = new Dictionary<string, CityDefinition>();
		foreach (XElement item in enumerable)
		{
			CityDefinition cityDefinition = new CityDefinition(item);
			CityDefinitions.Add(cityDefinition.Id, cityDefinition);
		}
	}
}
