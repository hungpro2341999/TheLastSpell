using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class BossPhasesDatabase : Database<BossPhasesDatabase>
{
	[SerializeField]
	private TextAsset bossPhasesDefinitionsTextAsset;

	public static Dictionary<string, BossPhasesDefinition> BossPhasesDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (BossPhasesDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(TPSingleton<BossPhasesDatabase>.Instance.bossPhasesDefinitionsTextAsset.text, LoadOptions.SetBaseUri).Element("BossPhasesDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("The document has no BossPhasesDefinitions!", LogType.Error);
			return;
		}
		BossPhasesDefinitions = new Dictionary<string, BossPhasesDefinition>();
		foreach (XElement item in xElement.Elements("BossPhasesDefinition"))
		{
			BossPhasesDefinition bossPhasesDefinition = new BossPhasesDefinition(item);
			BossPhasesDefinitions.Add(bossPhasesDefinition.Id, bossPhasesDefinition);
		}
	}
}
