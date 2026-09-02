using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Fog;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Fog;

public class FogDatabase : Database<FogDatabase>
{
	[SerializeField]
	private TextAsset[] fogDefinitionsTextAssets;

	[SerializeField]
	private TextAsset lightFogDefinition;

	public static Dictionary<string, FogDefinition> FogsDefinitions { get; private set; }

	public static LightFogDefinition LightFogDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (FogsDefinitions != null)
		{
			return;
		}
		Queue<XElement> elements = GatherElements(fogDefinitionsTextAssets, null, "FogDefinition");
		IEnumerable<XElement> enumerable = SortElementsByDependencies(elements);
		FogsDefinitions = new Dictionary<string, FogDefinition>();
		foreach (XElement item in enumerable)
		{
			FogDefinition fogDefinition = new FogDefinition(item);
			FogsDefinitions.Add(fogDefinition.Id, fogDefinition);
		}
		XElement xElement = XDocument.Parse(lightFogDefinition.text, LoadOptions.SetBaseUri).Element("LightFogDefinition");
		if (xElement.IsEmpty)
		{
			CLoggerManager.Log("The document " + lightFogDefinition.name + " must have LightFogDefinition!", LogType.Error);
		}
		else
		{
			LightFogDefinition = new LightFogDefinition(xElement);
		}
	}
}
