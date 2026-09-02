using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Definition.Night;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.Database;

public class GameDatabase : Database<GameDatabase>
{
	[SerializeField]
	private TextAsset nightReportRankDefinitionTextAsset;

	[FormerlySerializedAs("victoryCutsceneDefinitionsTextAssets")]
	[SerializeField]
	private TextAsset[] cutsceneDefinitionsTextAssets;

	public static List<NightReportRankDefinition> NightReportRankDefinitions { get; private set; }

	public static Dictionary<string, CutsceneDefinition> CutsceneDefinitions { get; private set; } = new Dictionary<string, CutsceneDefinition>();

	public override void Deserialize(XContainer container = null)
	{
		DeserializeNightReportRankDefinition();
		DeserializeVictorySequenceDefinitions();
	}

	private void DeserializeVictorySequenceDefinitions()
	{
		Queue<XElement> queue = GatherElements(cutsceneDefinitionsTextAssets, null, "CutsceneDefinition");
		Queue<XElement> queue2 = GatherElements(cutsceneDefinitionsTextAssets, null, "UnitCutsceneDefinition", "CutsceneDefinitions");
		while (queue2.Count > 0)
		{
			queue.Enqueue(queue2.Dequeue());
		}
		foreach (XElement item in SortElementsByDependencies(queue))
		{
			CutsceneDefinition cutsceneDefinition = new CutsceneDefinition(item);
			CutsceneDefinitions.Add(cutsceneDefinition.Id, cutsceneDefinition);
		}
	}

	private void DeserializeNightReportRankDefinition()
	{
		XElement xElement = XDocument.Parse(nightReportRankDefinitionTextAsset.text, LoadOptions.SetBaseUri).Element("NightReportRankDefinitions");
		if (xElement.IsNullOrEmpty())
		{
			CLoggerManager.Log("The document " + nightReportRankDefinitionTextAsset.name + " must have a NightReportRankDefinitions element!", LogType.Error);
			return;
		}
		NightReportRankDefinitions = new List<NightReportRankDefinition>();
		foreach (XElement item in xElement.Elements("NightReportRankDefinition"))
		{
			NightReportRankDefinitions.Add(new NightReportRankDefinition(item));
		}
	}
}
