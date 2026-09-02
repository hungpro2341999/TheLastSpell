using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class RandomBuildingsGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string AllowingRandomBuildingsIdsList = "AllowingRandomBuildings";
	}

	public struct BuildingInfo
	{
		public string Id;

		public TileFlagDefinition.E_TileFlagTag TileFlag;

		public int Count;
	}

	public string Id { get; private set; }

	public List<BuildingInfo> BuildingsInfo { get; } = new List<BuildingInfo>();

	public RandomBuildingsGenerationDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Id");
		Id = xAttribute.Value;
		foreach (XElement item2 in obj.Elements("Building"))
		{
			XAttribute xAttribute2 = item2.Attribute("Id");
			XAttribute xAttribute3 = item2.Attribute("TileFlag");
			if (!Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(xAttribute3.Value, out var result))
			{
				CLoggerManager.Log("Could not parse TileFlag attribute value " + xAttribute3.Value + " of Building " + xAttribute2.Value + " in RandomBuildingsGenerationDefinition " + Id + "!", LogType.Error);
				break;
			}
			XAttribute xAttribute4 = item2.Attribute("Count");
			if (!int.TryParse(xAttribute4.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse Count attribute value " + xAttribute4.Value + " of Building " + xAttribute2.Value + " in RandomBuildingsGenerationDefinition " + Id + "!", LogType.Error);
				break;
			}
			BuildingInfo item = new BuildingInfo
			{
				Id = xAttribute2.Value,
				TileFlag = result,
				Count = result2
			};
			BuildingsInfo.Add(item);
		}
	}
}
