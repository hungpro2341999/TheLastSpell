using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Apocalypse.LightFogSpawner;
using TheLastStand.Definition.Hazard;
using UnityEngine;

namespace TheLastStand.Definition.Fog;

public class LightFogDefinition : HazardDefinition
{
	public struct RepelInfo
	{
		public int Range;

		public bool CheckDiagonals;
	}

	public static class Constants
	{
		public const char EmptySymbol = '_';

		public const char OriginSymbol = 'O';

		public const char AffectedSymbol = 'X';
	}

	public Dictionary<string, LightFogSpawnersGenerationDefinition> LightFogSpawnersGenerationDefinitions { get; private set; }

	public Dictionary<string, float> LightFogSpawnersMultipliers { get; private set; }

	public RepelInfo Repel { get; private set; }

	public override E_HazardType HazardType => E_HazardType.LightFog;

	public Dictionary<string, List<Vector2Int>> Patterns { get; private set; }

	public LightFogDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("Repel");
		int.TryParse(xElement.Element("Range").Value, out var result);
		Repel = new RepelInfo
		{
			Range = result,
			CheckDiagonals = (xElement.Element("CheckDiagonals") != null)
		};
		LightFogSpawnersGenerationDefinitions = new Dictionary<string, LightFogSpawnersGenerationDefinition>();
		LightFogSpawnersMultipliers = new Dictionary<string, float>();
		XElement xElement2 = container.Element("LightFogSpawnersGenerations");
		foreach (XElement item in xElement2.Elements("LightFogSpawnersGeneration"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			LightFogSpawnersGenerationDefinitions.Add(xAttribute.Value, new LightFogSpawnersGenerationDefinition(item, base.TokenVariables));
		}
		foreach (XElement item2 in xElement2.Element("LightFogSpawnersMultipliers").Elements("LightFogSpawnersMultiplier"))
		{
			XAttribute xAttribute2 = item2.Attribute("FogDensity");
			float.TryParse(item2.Attribute("Multiplier").Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2);
			LightFogSpawnersMultipliers.Add(xAttribute2.Value, result2);
		}
		Patterns = new Dictionary<string, List<Vector2Int>>();
		foreach (XElement item3 in container.Element("Patterns").Elements("Pattern"))
		{
			string value = item3.Attribute("Id").Value;
			Vector2Int? vector2Int = null;
			List<Vector2Int> list = new List<Vector2Int>();
			string[] array = item3.Value.Split('\n');
			int num = array.Length - 1;
			int num2 = 0;
			while (num >= 0)
			{
				array[num] = array[num].RemoveWhitespace();
				for (int i = 0; i < array[num].Length; i++)
				{
					if (array[num][i] == '_')
					{
						continue;
					}
					if (array[num][i] == 'O')
					{
						if (!vector2Int.HasValue)
						{
							vector2Int = new Vector2Int(i, num2);
						}
						else
						{
							CLoggerManager.Log("origin tile is already set for pattern " + value + " !");
						}
					}
					else if (array[num][i] == 'X')
					{
						list.Add(new Vector2Int(i, num2));
					}
				}
				num--;
				num2++;
			}
			if (!vector2Int.HasValue)
			{
				CLoggerManager.Log("origin tile must be set for pattern " + value + " !");
			}
			else
			{
				for (int j = 0; j < list.Count; j++)
				{
					list[j] -= vector2Int.Value;
				}
			}
			Patterns.Add(value, list);
		}
	}
}
