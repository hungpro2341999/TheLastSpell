using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SpawnDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> DisallowedEnemies { get; private set; }

	public Dictionary<int, int> DistanceMaxFromCenterPerDays { get; private set; }

	public List<Dictionary<int, Node>> ElitesPerDayDefinitions { get; private set; }

	public string Id { get; private set; }

	public List<int> SpawnsCountMultipliers { get; private set; }

	public Node SpawnsCountPerWave { get; private set; }

	public Dictionary<int, Dictionary<string, int>> SpawnDirectionsPerDayDefinitions { get; private set; }

	public Dictionary<int, List<SpawnDirectionsDefinition.E_Direction>> OverridenForbiddenDirectionsPerDay { get; private set; }

	public Dictionary<int, Dictionary<string, int>> SpawnWavesPerDayDefinitions { get; private set; }

	public int SpawnPointsPerGroup { get; private set; }

	public Vector2Int SpawnPointRect { get; private set; }

	public SpawnDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		SpawnDefinition spawnDefinition = null;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("TemplateId");
		if (xAttribute2 != null)
		{
			spawnDefinition = SpawnWaveDatabase.SpawnDefinitions[xAttribute2.Value];
		}
		XElement xElement2 = xElement.Element("SpawnsCountMultipliers");
		if (xElement2 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log("SpawnDefinition " + Id + " has no SpawnsCountMultipliers and no Template to copy it from!", LogType.Error);
				return;
			}
			SpawnsCountMultipliers = new List<int>(spawnDefinition.SpawnsCountMultipliers);
		}
		else
		{
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			int i = 1;
			foreach (XElement item in xElement2.Elements("SpawnsCountMultiplier"))
			{
				if (!int.TryParse(item.Attribute("StartingNight").Value, out var result))
				{
					CLoggerManager.Log("Could not cast the StartingNight attribute into an int !", LogType.Error);
					return;
				}
				if (!int.TryParse(item.Value, out var result2))
				{
					CLoggerManager.Log("Could not cast the multiplier value into an int !", LogType.Error);
					return;
				}
				if (i == 1)
				{
					if (result != 1)
					{
						CLoggerManager.Log("The SpawnsCountMultiplier for the first night (StartingNight=\"1\") is required and must be placed first !", LogType.Error);
						list.Add(0);
					}
					else
					{
						list.Add(result2);
					}
					i++;
				}
				else if (result < i)
				{
					CLoggerManager.Log("The order of the SpawnsCountMultipliers isn't respected, it might lead to errors !", LogType.Error);
				}
				else
				{
					for (; i < result; i++)
					{
						list2.Add(i);
						list.Add(list[list.Count - 1]);
					}
					list.Add(result2);
					i++;
				}
			}
			if (list2.Count > 0)
			{
				string text = list2[0].ToString();
				while (list2.Count > 1)
				{
					text = text + ", " + list2[1];
					list2.RemoveAt(0);
				}
				CLoggerManager.Log("indexes are missing in the SpawnsCountMultipliers of the SpawnWaveConfig, it might be unintended ! Missing indexes are : " + text, LogType.Warning, CLogLevel.DETAILED);
			}
			SpawnsCountMultipliers = list;
		}
		XElement xElement3 = xElement.Element("SpawnsCountPerWave");
		if (xElement3.IsNullOrEmpty())
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log("SpawnWaveDefinitions " + Id + " has no SpawnsCountPerWave and no Template to copy it from!", LogType.Error);
				return;
			}
			SpawnsCountPerWave = spawnDefinition.SpawnsCountPerWave.Clone();
		}
		else
		{
			SpawnsCountPerWave = Parser.Parse(xElement3.Value);
		}
		SpawnWavesPerDayDefinitions = new Dictionary<int, Dictionary<string, int>>();
		XElement xElement4 = xElement.Element("SpawnWavesPerDayDefinitions");
		if (xElement4 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log("SpawnWaveDefinitions " + Id + " has no SpawnWavesPerDayDefinitions and no Template to copy it from!", LogType.Error);
				return;
			}
			foreach (KeyValuePair<int, Dictionary<string, int>> spawnWavesPerDayDefinition in spawnDefinition.SpawnWavesPerDayDefinitions)
			{
				SpawnWavesPerDayDefinitions.Add(spawnWavesPerDayDefinition.Key, new Dictionary<string, int>(spawnWavesPerDayDefinition.Value));
			}
		}
		else
		{
			foreach (XElement item2 in xElement4.Elements("SpawnWavesPerDayDefinition"))
			{
				XAttribute xAttribute3 = item2.Attribute("StartingNight");
				if (xAttribute3.IsNullOrEmpty())
				{
					CLoggerManager.Log("SpawnWavesPerDayDefinition must have StartingNight!", LogType.Error);
					continue;
				}
				if (!int.TryParse(xAttribute3.Value, out var result3))
				{
					CLoggerManager.Log("StartingDay must be int!", LogType.Error);
					continue;
				}
				SpawnWavesPerDayDefinitions.Add(result3, new Dictionary<string, int>());
				foreach (XElement item3 in item2.Elements("SpawnWaveDefinition"))
				{
					XAttribute xAttribute4 = item3.Attribute("Id");
					if (xAttribute4.IsNullOrEmpty())
					{
						CLoggerManager.Log("SpawnWaveDefinition must have an Id!", LogType.Error);
						continue;
					}
					XAttribute xAttribute5 = item3.Attribute("Weight");
					int result4;
					if (xAttribute5.IsNullOrEmpty())
					{
						CLoggerManager.Log("SpawnWaveDefinition must have a Weight!", LogType.Error);
					}
					else if (!int.TryParse(xAttribute5.Value, out result4))
					{
						CLoggerManager.Log("Weight must be int!", LogType.Error);
					}
					else
					{
						SpawnWavesPerDayDefinitions[result3].Add(xAttribute4.Value, result4);
					}
				}
			}
		}
		SpawnDirectionsPerDayDefinitions = new Dictionary<int, Dictionary<string, int>>();
		XElement xElement5 = xElement.Element("SpawnDirectionsPerDayDefinitions");
		if (xElement5 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log("SpawnWaveDefinitions " + Id + " has no SpawnDirectionsPerDayDefinitions and no Template to copy it from!", LogType.Error);
				return;
			}
			foreach (KeyValuePair<int, Dictionary<string, int>> spawnDirectionsPerDayDefinition in spawnDefinition.SpawnDirectionsPerDayDefinitions)
			{
				SpawnDirectionsPerDayDefinitions.Add(spawnDirectionsPerDayDefinition.Key, new Dictionary<string, int>(spawnDirectionsPerDayDefinition.Value));
			}
		}
		else
		{
			foreach (XElement item4 in xElement5.Elements("SpawnDirectionsPerDayDefinition"))
			{
				XAttribute xAttribute6 = item4.Attribute("StartingNight");
				if (xAttribute6.IsNullOrEmpty())
				{
					CLoggerManager.Log("SpawnDirectionPerDayDefinition must have StartingNight!", LogType.Error);
					continue;
				}
				if (!int.TryParse(xAttribute6.Value, out var result5))
				{
					CLoggerManager.Log("StartingDay must be int!", LogType.Error);
					continue;
				}
				SpawnDirectionsPerDayDefinitions.Add(result5, new Dictionary<string, int>());
				foreach (XElement item5 in item4.Elements("SpawnDirectionDefinition"))
				{
					XAttribute xAttribute7 = item5.Attribute("Id");
					if (xAttribute7.IsNullOrEmpty())
					{
						CLoggerManager.Log("SpawnDirectionDefinition must have an Id!", LogType.Error);
						continue;
					}
					XAttribute xAttribute8 = item5.Attribute("Weight");
					int result6;
					if (xAttribute8.IsNullOrEmpty())
					{
						CLoggerManager.Log("SpawnDirectionDefinition must have a Weight!", LogType.Error);
					}
					else if (!int.TryParse(xAttribute8.Value, out result6))
					{
						CLoggerManager.Log("Weight must be int!", LogType.Error);
					}
					else
					{
						SpawnDirectionsPerDayDefinitions[result5].Add(xAttribute7.Value, result6);
					}
				}
			}
		}
		OverridenForbiddenDirectionsPerDay = new Dictionary<int, List<SpawnDirectionsDefinition.E_Direction>>();
		XElement xElement6 = xElement.Element("OverridenForbiddenDirectionsPerDay");
		if (xElement6.IsNullOrEmpty())
		{
			if (spawnDefinition != null)
			{
				OverridenForbiddenDirectionsPerDay = spawnDefinition.OverridenForbiddenDirectionsPerDay;
			}
		}
		else
		{
			foreach (XElement item6 in xElement6.Elements("OverrideForbiddenDirection"))
			{
				int num = int.Parse(item6.Attribute("StartingNight")?.Value ?? "1");
				if (OverridenForbiddenDirectionsPerDay.ContainsKey(num))
				{
					CLoggerManager.Log($"Forbidden Direction starting day {num} of SpawnDefinition {Id} is set twice!", LogType.Warning);
					continue;
				}
				OverridenForbiddenDirectionsPerDay[num] = new List<SpawnDirectionsDefinition.E_Direction>();
				List<XElement> list3 = item6.Elements("ForbiddenDirection").ToList();
				if (list3.Count == 0)
				{
					continue;
				}
				foreach (XElement item7 in list3)
				{
					if (!Enum.TryParse<SpawnDirectionsDefinition.E_Direction>(item7.Value, out var result7))
					{
						CLoggerManager.Log("Could not parse Forbidden Direction " + item7.Value + " of SpawnDefinition " + Id + " as a valid Direction!", LogType.Error);
					}
					else
					{
						OverridenForbiddenDirectionsPerDay.AddAtKey(num, result7);
					}
				}
			}
		}
		ElitesPerDayDefinitions = new List<Dictionary<int, Node>>();
		XElement xElement7 = xElement.Element("ElitesPerDayDefinitions");
		if (xElement7 != null)
		{
			int j = 0;
			Dictionary<int, Node> dictionary = new Dictionary<int, Node>();
			foreach (XElement item8 in xElement7.Elements("ElitesPerDayDefinition"))
			{
				if (!int.TryParse(item8.Attribute("StartingNight").Value, out var result8))
				{
					CLoggerManager.Log("Could not parse StartingNight attribute into an int (" + Id + "), skipping this one.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
					continue;
				}
				if (result8 <= j)
				{
					CLoggerManager.Log($"StartingNight attribute is inferior or equal to the current index : {result8} <= {j} ({Id}). Please check the order, skipping this one.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
					continue;
				}
				for (; j < result8 - 1; j++)
				{
					ElitesPerDayDefinitions.Add(dictionary);
				}
				dictionary = new Dictionary<int, Node>();
				foreach (XElement item9 in item8.Elements("Elites"))
				{
					if (!int.TryParse(item9.Attribute("Tier").Value, out var result9))
					{
						CLoggerManager.Log($"Could not parse Tier attribute into an int in element ElitesPerDayDefinition ({Id}, starting night : {result8}), skipping this one.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
						continue;
					}
					Node node = Parser.Parse(item9.Value);
					if (node == null)
					{
						CLoggerManager.Log($"Could not parse Elites element into an interpreted expression in element ElitesPerDayDefinition ({Id}, starting night : {result8}), skipping this one.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
					}
					else if (dictionary.ContainsKey(result9))
					{
						CLoggerManager.Log($"This tier ({result9}) is already defined in element ElitesPerDayDefinition ({Id}, starting night : {result8}), skipping this one.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
					}
					else
					{
						dictionary.Add(result9, node);
					}
				}
				ElitesPerDayDefinitions.Add(dictionary);
				j++;
			}
		}
		else
		{
			ElitesPerDayDefinitions.Add(new Dictionary<int, Node>());
		}
		XElement xElement8 = xElement.Element("SpawnPointsPerGroup");
		if (xElement8.IsNullOrEmpty())
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log("SpawnWaveDefinitions " + Id + " has no SpawnPointsPerGroup and no Template to copy it from!", LogType.Error);
				return;
			}
			SpawnPointsPerGroup = spawnDefinition.SpawnPointsPerGroup;
		}
		else
		{
			if (!int.TryParse(xElement8.Value, out var result10))
			{
				CLoggerManager.Log("Invalid SpawnPointsPerGroup", LogType.Error);
				return;
			}
			SpawnPointsPerGroup = result10;
		}
		XElement xElement9 = xElement.Element("SpawnPointRect");
		if (xElement9 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log("SpawnWaveDefinitions " + Id + " has no SpawnPointRect and no Template to copy it from!", LogType.Error);
				return;
			}
			SpawnPointRect = spawnDefinition.SpawnPointRect;
		}
		else
		{
			if (!int.TryParse(xElement9.Attribute("Width").Value, out var result11))
			{
				CLoggerManager.Log("Invalid SpawnPointRect Width", LogType.Error);
				return;
			}
			if (!int.TryParse(xElement9.Attribute("Height").Value, out var result12))
			{
				CLoggerManager.Log("Invalid SpawnPointRect Height", LogType.Error);
				return;
			}
			SpawnPointRect = new Vector2Int(result11, result12);
		}
		XElement xElement10 = xElement.Element("DistanceMaxFromCenterPerDays");
		if (xElement10 == null)
		{
			if (spawnDefinition == null)
			{
				CLoggerManager.Log("SpawnWaveDefinitions " + Id + " has no DistanceMaxFromCenterPerDays and no Template to copy it from!", LogType.Error);
				return;
			}
			DistanceMaxFromCenterPerDays = spawnDefinition.DistanceMaxFromCenterPerDays;
			DistanceMaxFromCenterPerDays = new Dictionary<int, int>(spawnDefinition.DistanceMaxFromCenterPerDays);
		}
		else
		{
			DistanceMaxFromCenterPerDays = new Dictionary<int, int>();
			foreach (XElement item10 in xElement10.Elements("DistanceMaxFromCenterPerDay"))
			{
				XAttribute xAttribute9 = item10.Attribute("StartingNight");
				if (xAttribute9.IsNullOrEmpty())
				{
					CLoggerManager.Log("DistanceMaxFromCenterPerDay must have StartingNight!", LogType.Error);
					continue;
				}
				if (!int.TryParse(xAttribute9.Value, out var result13))
				{
					CLoggerManager.Log("StartingDay must be int!", LogType.Error);
					continue;
				}
				XAttribute xAttribute10 = item10.Attribute("Value");
				int result14;
				if (xAttribute10.IsNullOrEmpty())
				{
					CLoggerManager.Log("DistanceMaxFromCenterPerDay must have Value", LogType.Error);
				}
				else if (!int.TryParse(xAttribute10.Value, out result14))
				{
					CLoggerManager.Log("Invalid DistanceMaxFromCenterPerDay Value " + item10.Value, LogType.Error);
				}
				else
				{
					DistanceMaxFromCenterPerDays.Add(result13, result14);
				}
			}
		}
		XElement xElement11 = xElement.Element("DisallowedEnemies");
		if (xElement11 == null)
		{
			if (spawnDefinition != null && spawnDefinition.DisallowedEnemies != null)
			{
				DisallowedEnemies = new List<string>(spawnDefinition.DisallowedEnemies);
			}
			return;
		}
		DisallowedEnemies = new List<string>();
		foreach (XElement item11 in xElement11.Elements("EnemyId"))
		{
			DisallowedEnemies.Add(item11.Value);
		}
	}

	public List<SpawnDirectionsDefinition.E_Direction> GetOverridenForbiddenDirectionsForDayNumber(int dayNumber)
	{
		Dictionary<int, List<SpawnDirectionsDefinition.E_Direction>> overridenForbiddenDirectionsPerDay = OverridenForbiddenDirectionsPerDay;
		if (overridenForbiddenDirectionsPerDay == null)
		{
			return null;
		}
		List<SpawnDirectionsDefinition.E_Direction> result = null;
		foreach (KeyValuePair<int, List<SpawnDirectionsDefinition.E_Direction>> item in overridenForbiddenDirectionsPerDay)
		{
			if (item.Key <= dayNumber)
			{
				result = item.Value;
				continue;
			}
			break;
		}
		return result;
	}
}
