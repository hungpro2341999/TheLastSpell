using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.PlayableUnitGeneration;

public class EquipmentGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class ItemGenerationData
	{
		public string ItemId { get; }

		public string ItemLevelModifiersList { get; }

		public string ItemRaritiesList { get; }

		public string ItemsList { get; }

		public ItemGenerationData(string itemId, string itemLevelModifiersList, string itemRaritiesList, string itemsList)
		{
			ItemId = itemId;
			ItemLevelModifiersList = itemLevelModifiersList;
			ItemRaritiesList = itemRaritiesList;
			ItemsList = itemsList;
		}
	}

	public List<Tuple<int, ItemGenerationData>> ItemsPerWeight { get; private set; } = new List<Tuple<int, ItemGenerationData>>();

	public ItemSlotDefinition.E_ItemSlotId Slot { get; private set; }

	public int TotalWeight { get; private set; }

	public EquipmentGenerationDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Slot");
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("An EquipmentGenerationDefinition has an invalid Id " + xAttribute.Value + "!", LogType.Error);
		}
		Slot = result;
		foreach (XElement item2 in obj.Elements("Items"))
		{
			int num = int.Parse(item2.Attribute("Weight").Value);
			TotalWeight += num;
			XAttribute xAttribute2 = item2.Element("ItemsList")?.Attribute("Id");
			if (xAttribute2 != null)
			{
				XAttribute xAttribute3 = item2.Attribute("Id");
				XAttribute xAttribute4 = item2.Element("ItemLevelModifiersList")?.Attribute("Id");
				XAttribute xAttribute5 = item2.Element("ItemRaritiesList")?.Attribute("Id");
				ItemGenerationData item = new ItemGenerationData(xAttribute3?.Value ?? string.Empty, xAttribute4?.Value, xAttribute5?.Value, xAttribute2?.Value);
				ItemsPerWeight.Add(new Tuple<int, ItemGenerationData>(num, item));
			}
		}
	}
}
