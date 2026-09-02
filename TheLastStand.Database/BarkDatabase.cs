using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Database;

public class BarkDatabase : Database<BarkDatabase>
{
	[SerializeField]
	private TextAsset barkDefinitionsTextAsset;

	public static Dictionary<string, BarkDefinition> BarkDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (BarkDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(barkDefinitionsTextAsset.text, LoadOptions.SetBaseUri).Element("BarkDefinitions");
		if (xElement.IsNullOrEmpty())
		{
			CLoggerManager.Log("The document " + barkDefinitionsTextAsset.name + " must have BarkDefinitions!", LogType.Error);
			return;
		}
		BarkDefinitions = new Dictionary<string, BarkDefinition>();
		foreach (XElement item in xElement.Elements("BarkDefinition"))
		{
			BarkDefinition barkDefinition = new BarkDefinition(item);
			BarkDefinitions.Add(barkDefinition.Id, barkDefinition);
		}
	}
}
