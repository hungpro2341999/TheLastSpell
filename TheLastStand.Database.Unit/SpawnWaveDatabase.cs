using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class SpawnWaveDatabase : Database<SpawnWaveDatabase>
{
	[SerializeField]
	private TextAsset[] spawnDefinitionsTextAssets;

	[SerializeField]
	private TextAsset[] waveDefinitionsTextAssets;

	[SerializeField]
	private TextAsset spawnDirectionDefinitionsTextAsset;

	public static Dictionary<string, SpawnDirectionsDefinition> SpawnDirectionDefinitions { get; private set; }

	public static Dictionary<string, SpawnWaveDefinition> WaveDefinitions { get; private set; }

	public static Dictionary<string, SpawnDefinition> SpawnDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		DeserializeSpawnDirectionsDefinition();
		DeserializeSpawnDefinitions();
		DeserializeWaveDefinitions();
	}

	private void DeserializeSpawnDefinitions()
	{
		if (SpawnDefinitions != null)
		{
			return;
		}
		Queue<XElement> elements = GatherElements(spawnDefinitionsTextAssets, null, "SpawnDefinition");
		IEnumerable<XElement> enumerable = SortElementsByDependencies(elements);
		SpawnDefinitions = new Dictionary<string, SpawnDefinition>();
		foreach (XElement item in enumerable)
		{
			SpawnDefinition spawnDefinition = new SpawnDefinition(item);
			SpawnDefinitions.Add(spawnDefinition.Id, spawnDefinition);
		}
	}

	private void DeserializeSpawnDirectionsDefinition()
	{
		if (SpawnDirectionDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(spawnDirectionDefinitionsTextAsset.text, LoadOptions.SetBaseUri).Element("SpawnDirectionsDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("SpawnWaveDefinitions must have a SpawnDirectionDefinitions!", LogType.Error);
			return;
		}
		SpawnDirectionDefinitions = new Dictionary<string, SpawnDirectionsDefinition>();
		foreach (XElement item in xElement.Elements("SpawnDirectionsDefinition"))
		{
			SpawnDirectionsDefinition spawnDirectionsDefinition = new SpawnDirectionsDefinition(item);
			SpawnDirectionDefinitions.Add(spawnDirectionsDefinition.Id, spawnDirectionsDefinition);
		}
	}

	private void DeserializeWaveDefinitions()
	{
		if (WaveDefinitions == null)
		{
			Queue<XElement> queue = GatherElements(waveDefinitionsTextAssets, null, "SpawnWaveDefinition");
			WaveDefinitions = new Dictionary<string, SpawnWaveDefinition>();
			while (queue.Count > 0)
			{
				SpawnWaveDefinition spawnWaveDefinition = new SpawnWaveDefinition(queue.Dequeue());
				WaveDefinitions.Add(spawnWaveDefinition.Id, spawnWaveDefinition);
			}
		}
	}
}
