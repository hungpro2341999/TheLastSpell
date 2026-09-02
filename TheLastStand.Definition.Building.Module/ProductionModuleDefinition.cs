using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class ProductionModuleDefinition : BuildingModuleDefinition
{
	public static class Constants
	{
		public const int LevelDefaultValue = 1;
	}

	public List<BuildingActionDefinition> BuildingActionDefinitions { get; private set; }

	public BuildingGaugeEffectDefinition BuildingGaugeEffectDefinition { get; private set; }

	public int Level { get; private set; } = 1;

	public ProductionModuleDefinition(BuildingDefinition buildingDefinition, XContainer productionDefinition)
		: base(buildingDefinition, productionDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("Level");
		if (xElement2 != null)
		{
			if (xElement2.IsNullOrEmpty())
			{
				Debug.LogError("Building " + BuildingDefinition.Id + " has an invalid Level !");
				return;
			}
			if (!int.TryParse(xElement2.Value, out var result))
			{
				Debug.LogError("Building " + BuildingDefinition.Id + "'s Level " + HasAnInvalidInt(xElement2.Value));
				return;
			}
			Level = result;
		}
		XElement xElement3 = xElement.Element("BuildingGaugeEffectDefinition");
		if (xElement3 != null)
		{
			using IEnumerator<XElement> enumerator = xElement3.Elements().GetEnumerator();
			if (enumerator.MoveNext())
			{
				XElement current = enumerator.Current;
				if (BuildingDatabase.BuildingGaugeEffectDefinitions.TryGetValue(current.Name.LocalName, out var value))
				{
					BuildingGaugeEffectDefinition = value.Clone();
					XAttribute xAttribute = xElement3.Attribute("TriggeredOnConstruction");
					if (xAttribute != null)
					{
						BuildingGaugeEffectDefinition.TriggeredOnConstruction = bool.Parse(xAttribute.Value);
					}
					switch (BuildingGaugeEffectDefinition.Id)
					{
					case "CreateItem":
						(BuildingGaugeEffectDefinition as CreateItemGaugeEffectDefinition).CreateItemDefinition = new CreateItemDefinition(current);
						break;
					case "GlobalUpgradeStat":
						(BuildingGaugeEffectDefinition as UpgradeStatGaugeEffectDefinition).UpgradeStatDefinition = new UpgradeStatDefinition(current);
						break;
					}
				}
				else
				{
					Debug.LogError("BuildingGaugeEffectDefinition " + current.Name.LocalName + " not found");
				}
			}
		}
		XElement xElement4 = xElement.Element("BuildingActionDefinitions");
		if (xElement4 == null)
		{
			return;
		}
		BuildingActionDefinitions = new List<BuildingActionDefinition>();
		foreach (XElement item2 in xElement4.Elements("BuildingActionDefinition"))
		{
			XAttribute xAttribute2 = item2.Attribute("Id");
			if (xAttribute2.IsNullOrEmpty())
			{
				Debug.LogError("BuildingDefinition " + BuildingDefinition.Id + " BuildingActionDefinition must have a valid string");
			}
			if (BuildingDatabase.BuildingActionDefinitions.TryGetValue(xAttribute2.Value, out var value2))
			{
				BuildingActionDefinition item = value2.Clone();
				BuildingActionDefinitions.Add(item);
				continue;
			}
			Debug.LogError("BuildingActionDefinition " + xAttribute2.Value + " not found");
			break;
		}
	}
}
