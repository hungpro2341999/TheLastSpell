using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using UnityEngine;

namespace TheLastStand.Definition.Brazier;

public class BrazierToSpawnPerNight : NightIndexedItem
{
	private static class Constants
	{
		public const string BrazierToSpawnElement = "BrazierToSpawn";

		public const string BuildingIdAttribute = "BuildingId";
	}

	public BuildingDefinition BrazierDefinition;

	public override void Init(int nightIndex, XElement xElement)
	{
		base.Init(nightIndex, xElement);
		XAttribute xAttribute = xElement.Attribute("BuildingId");
		if (!BuildingDatabase.BuildingDefinitions.TryGetValue(xAttribute.Value, out var value))
		{
			CLoggerManager.Log("BuildingId attribute could not be found in the buildings database (" + xAttribute.Value + "). Skipped.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "BrazierDefinition");
		}
		else
		{
			BrazierDefinition = value;
		}
	}
}
