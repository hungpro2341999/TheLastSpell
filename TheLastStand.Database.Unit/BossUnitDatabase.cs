using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class BossUnitDatabase : Database<BossUnitDatabase>
{
	[SerializeField]
	private TextAsset bossStagingDefinitionsTextAsset;

	[SerializeField]
	private IEnumerable<TextAsset> individualBossUnitTemplateDefinitionsTextAssets;

	[SerializeField]
	private IEnumerable<TextAsset> groupBossUnitTemplateDefinitionsTextAssets;

	public static Dictionary<string, BossUnitTemplateDefinition> BossUnitTemplateDefinitions { get; private set; }

	public static Dictionary<string, BossStagingDefinition> BossStagingDefinitions { get; private set; }

	public static Dictionary<string, EnemySkillSoundDefinition> BossSkillSoundDefinitions { get; private set; }

	public static Dictionary<string, HitByEnemySoundDefinition> HitByEnemySoundDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (BossUnitTemplateDefinitions != null)
		{
			return;
		}
		BossUnitTemplateDefinitions = new Dictionary<string, BossUnitTemplateDefinition>();
		Queue<XElement> elements = GatherElements(groupBossUnitTemplateDefinitionsTextAssets, individualBossUnitTemplateDefinitionsTextAssets, "BossUnitTemplateDefinition");
		foreach (XElement item in SortElementsByDependencies(elements))
		{
			BossUnitTemplateDefinition bossUnitTemplateDefinition = new BossUnitTemplateDefinition(item);
			try
			{
				BossUnitTemplateDefinitions.Add(bossUnitTemplateDefinition.Id, bossUnitTemplateDefinition);
			}
			catch (ArgumentException)
			{
				Debug.LogWarning("Duplicate boss definition found for ID " + bossUnitTemplateDefinition.Id + ": the individual template files will have PRIORITY over the all-in-one template file.");
			}
		}
		BossStagingDefinitions = new Dictionary<string, BossStagingDefinition>();
		XDocument xDocument = XDocument.Parse(bossStagingDefinitionsTextAsset.text, LoadOptions.SetBaseUri);
		ConcurrentQueue<XElement> bossStagingDefinitions = new ConcurrentQueue<XElement>();
		xDocument.Element("BossStagingDefinitions").Elements("BossStagingDefinition").All(delegate(XElement o)
		{
			bossStagingDefinitions.Enqueue(o);
			return true;
		});
		XElement result;
		while (bossStagingDefinitions.TryDequeue(out result))
		{
			string value = result.Attribute("BossId").Value;
			BossStagingDefinitions.Add(value, new BossStagingDefinition(result));
		}
	}
}
