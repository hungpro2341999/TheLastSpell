using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class ResourceDatabase : Database<ResourceDatabase>
{
	[SerializeField]
	private TextAsset[] resourceDefinitionsTextAssets;

	public static Dictionary<string, ResourceDefinition> ResourceDefinitions { get; private set; }

	public int FirstRunDamnedSoulsGain { get; private set; } = -1;

	public override void Deserialize(XContainer container = null)
	{
		if (ResourceDefinitions != null)
		{
			return;
		}
		ResourceDefinitions = new Dictionary<string, ResourceDefinition>();
		TextAsset[] array = resourceDefinitionsTextAssets;
		for (int i = 0; i < array.Length; i++)
		{
			XElement xElement = XDocument.Parse(array[i].text, LoadOptions.SetBaseUri).Element("ResourceDefinitions");
			foreach (XElement item in xElement.Elements("ResourceDefinition"))
			{
				ResourceDefinition resourceDefinition = new ResourceDefinition(item);
				ResourceDefinitions.Add(resourceDefinition.Id, resourceDefinition);
			}
			XElement xElement2 = xElement.Element("FirstRunDamnedSoulsGain");
			if (xElement2 != null)
			{
				if (int.TryParse(xElement2.Value, out var result))
				{
					FirstRunDamnedSoulsGain = result;
					continue;
				}
				CLoggerManager.Log("Element FirstRunDamnedSoulsGain could not be parsed into an int. Set value to 0.", LogType.Error);
				FirstRunDamnedSoulsGain = 0;
			}
		}
		if (FirstRunDamnedSoulsGain == -1)
		{
			CLoggerManager.Log("Element FirstRunDamnedSoulsGain wasn't in the given text assets. Set value to 0.", LogType.Error, CLogLevel.NORMAL, forcePrintInUnity: true, "ResourceDatabase");
			FirstRunDamnedSoulsGain = 0;
		}
	}
}
