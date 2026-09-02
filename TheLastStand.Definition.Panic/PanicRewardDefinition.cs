using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Panic;

public class PanicRewardDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class DayGenerationDatas
	{
		public int BaseGenerationLevel { get; set; }

		public string ItemGenerationModifiersListId { get; set; }

		public string ItemsListId { get; set; }

		public string ItemRaritiesListId { get; set; }
	}

	public Node Gold { get; private set; }

	public Dictionary<int, DayGenerationDatas> ItemsListsPerDay { get; private set; }

	public Dictionary<int, int> ItemsListsTotalWeightPerDay { get; private set; }

	public Node Materials { get; private set; }

	public PanicRewardDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Element("Gold").Attribute("Value");
		Gold = Parser.Parse(xAttribute.Value);
		XAttribute xAttribute2 = obj.Element("Materials").Attribute("Value");
		Materials = Parser.Parse(xAttribute2.Value);
		XElement xElement = obj.Element("ItemsLists");
		if (xElement == null)
		{
			return;
		}
		ItemsListsPerDay = new Dictionary<int, DayGenerationDatas>();
		ItemsListsTotalWeightPerDay = new Dictionary<int, int>();
		foreach (XElement item in xElement.Elements("Reward"))
		{
			XAttribute xAttribute3 = item.Attribute("Index");
			if (!int.TryParse(xAttribute3.Value, out var result))
			{
				Debug.LogError("Invalid StartingDay " + xAttribute3.Value);
			}
			XElement xElement2 = item.Element("BaseLevel");
			if (!int.TryParse(xElement2.Value, out var result2))
			{
				Debug.LogError("Invalid BaseGenerationLevel " + xElement2.Value);
			}
			string value = item.Element("ItemRaritiesList").Attribute("Id").Value;
			string value2 = item.Element("ItemGenerationModifiersList").Attribute("Id").Value;
			string value3 = item.Element("ItemsList").Attribute("Id").Value;
			DayGenerationDatas value4 = new DayGenerationDatas
			{
				BaseGenerationLevel = result2,
				ItemsListId = value3,
				ItemGenerationModifiersListId = value2,
				ItemRaritiesListId = value
			};
			ItemsListsPerDay.Add(result, value4);
		}
	}
}
