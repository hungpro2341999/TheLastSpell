using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class UnitDatabase : Database<UnitDatabase>
{
	[SerializeField]
	private TextAsset unitStatDefinitions;

	[SerializeField]
	private TextAsset pathfindingDefinitionTextAsset;

	public static PathfindingDefinition PathfindingDefinition { get; private set; }

	public static Dictionary<UnitStatDefinition.E_Stat, UnitStatDefinition> UnitStatDefinitions { get; private set; }

	public static float MagicDamagePercentageResistanceReduction { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement xElement = XDocument.Parse(unitStatDefinitions.text, LoadOptions.SetBaseUri).Element("UnitStatDefinitions");
		UnitStatDefinitions = new Dictionary<UnitStatDefinition.E_Stat, UnitStatDefinition>(UnitStatDefinition.SharedStatComparer);
		foreach (XElement item in xElement.Elements("UnitStatDefinition"))
		{
			UnitStatDefinition unitStatDefinition = new UnitStatDefinition(item);
			UnitStatDefinitions.Add(unitStatDefinition.Id, unitStatDefinition);
		}
		if (float.TryParse(xElement.Element("MagicDamagePercentageResistanceReduction").Attribute("Value").Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			MagicDamagePercentageResistanceReduction = result;
		}
		else
		{
			CLoggerManager.Log("Could not parse Value attribute into a float for MagicDamagePercentageResistanceReduction in UnitDatabase", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "UnitDatabase");
		}
		PathfindingDefinition = new PathfindingDefinition(XDocument.Parse(pathfindingDefinitionTextAsset.text, LoadOptions.SetBaseUri).Element("PathfindingDefinition"));
	}
}
