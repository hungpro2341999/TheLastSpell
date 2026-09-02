using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public class SpawnSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "Spawn";

	public List<string> BuildingIdsToDestroy = new List<string>();

	public float Delay { get; private set; }

	public List<EnemySpawnData> EnemiesByAmount { get; } = new List<EnemySpawnData>();

	public List<EnemySpawnData> EnemiesByWeight { get; } = new List<EnemySpawnData>();

	public List<EnemySpawnData> RandomEnemies { get; } = new List<EnemySpawnData>();

	public bool IsByAmount { get; private set; }

	public Node RandomEnemiesAmount { get; private set; }

	public SpawnSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = (container as XElement).Element("Spawn");
		XAttribute xAttribute = xElement.Attribute("Delay");
		if (xAttribute != null)
		{
			if (!float.TryParse(xAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("Could not parse SpawnSkillAction Delay attribute value " + xAttribute.Value + " to a valid float!", LogType.Error);
			}
			else
			{
				Delay = result;
			}
		}
		XElement xElement2 = xElement.Element("ByWeight");
		if (xElement2 != null)
		{
			foreach (XElement item in xElement2.Elements("Enemy"))
			{
				EnemiesByWeight.Add(DeserializeEnemyElement(item));
			}
		}
		XElement xElement3 = xElement.Element("ByAmount");
		if (xElement3 != null)
		{
			foreach (XElement item2 in xElement3.Elements("Enemy"))
			{
				EnemiesByAmount.Add(DeserializeEnemyElement(item2));
			}
			XElement xElement4 = xElement3.Element("RandomEnemies");
			if (xElement4 != null)
			{
				XAttribute xAttribute2 = xElement4.Attribute("Amount");
				if (xAttribute2 != null)
				{
					RandomEnemiesAmount = Parser.Parse(xAttribute2.Value);
				}
				foreach (XElement item3 in xElement4.Elements("Enemy"))
				{
					RandomEnemies.Add(DeserializeEnemyElement(item3));
				}
			}
			if (RandomEnemiesAmount == null)
			{
				Node node = (RandomEnemiesAmount = Parser.Parse("0"));
			}
		}
		IsByAmount = EnemiesByAmount.Count > 0 || RandomEnemies.Count > 0;
		XElement xElement5 = xElement.Element("BuildingDestructionRule");
		if (xElement5 == null)
		{
			return;
		}
		foreach (XElement item4 in xElement5.Elements("BuildingsList"))
		{
			XAttribute xAttribute3 = item4.Attribute("Id");
			if (xAttribute3 == null)
			{
				CLoggerManager.Log("SpawnSkillAction has an empty Id of a BuildingDestructionRule/BuildingsList !", LogType.Error);
				continue;
			}
			if (!GenericDatabase.IdsListDefinitions.TryGetValue(xAttribute3.Value, out var value))
			{
				CLoggerManager.Log("Trying to get Ids List " + xAttribute3.Value + " but it does not exist.", LogType.Error);
				continue;
			}
			foreach (string id in value.Ids)
			{
				if (!BuildingIdsToDestroy.Contains(id))
				{
					BuildingIdsToDestroy.Add(id);
				}
			}
		}
	}

	private EnemySpawnData DeserializeEnemyElement(XElement enemyElement)
	{
		XAttribute xAttribute = enemyElement.Attribute("Id");
		int result = -1;
		TileFlagDefinition.E_TileFlagTag result2 = TileFlagDefinition.E_TileFlagTag.None;
		string bossPhaseActorId = null;
		XAttribute xAttribute2 = enemyElement.Attribute("Weight");
		if (xAttribute2 != null && !int.TryParse(xAttribute2.Value, out result))
		{
			CLoggerManager.Log($"Could not parse Weight \"{xAttribute2.Value}\" of {GetType().Name} to a valid int! (Enemy to spawn Id: {xAttribute})", LogType.Error);
		}
		XAttribute xAttribute3 = enemyElement.Attribute("Amount");
		Node amount = ((xAttribute3 == null) ? Parser.Parse("1") : Parser.Parse(xAttribute3.Value));
		XAttribute xAttribute4 = enemyElement.Attribute("FlagTag");
		if (xAttribute4 != null && !Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(xAttribute4.Value, out result2))
		{
			CLoggerManager.Log($"Could not parse FlagTag \"{xAttribute4.Value}\" of {GetType().Name} to a valid TileFlag! (Enemy to spawn Id: {xAttribute})", LogType.Error);
		}
		XAttribute xAttribute5 = enemyElement.Attribute("BossActorId");
		if (xAttribute5 != null)
		{
			bossPhaseActorId = xAttribute5.Value;
		}
		bool result3 = false;
		XAttribute xAttribute6 = enemyElement.Attribute("IsGuardian");
		if (xAttribute6 != null && !bool.TryParse(xAttribute6.Value, out result3))
		{
			CLoggerManager.Log("Unable to parse " + xAttribute6.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
		}
		return new EnemySpawnData(xAttribute.Value, result, amount, result2, new UnitCreationSettings(bossPhaseActorId, castSpawnSkill: true, playSpawnAnim: true, playSpawnCutscene: true, waitSpawnAnim: false, -1, null, result3));
	}
}
