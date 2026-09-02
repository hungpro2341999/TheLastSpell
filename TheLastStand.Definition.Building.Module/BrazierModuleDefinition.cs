using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class BrazierModuleDefinition : BuildingModuleDefinition
{
	private static class Constants
	{
		public const string PointsTotalElement = "PointsTotal";
	}

	public int BrazierPointsTotal { get; private set; }

	public BrazierModuleDefinition(BuildingDefinition buildingDefinition, XContainer constructionDefinition)
		: base(buildingDefinition, constructionDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container is XElement xElement)
		{
			XElement xElement2 = xElement.Element("PointsTotal");
			if (!int.TryParse(xElement2.Value, out var result))
			{
				CLoggerManager.Log("PointsTotal couldn't be parsed into an int : " + xElement2.Value + ".,", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "BrazierModuleDefinition");
			}
			BrazierPointsTotal = result;
		}
	}
}
