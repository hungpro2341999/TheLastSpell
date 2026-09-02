using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class CreateItemDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static int All => -1;

	public Node Count { get; private set; }

	public bool HasID => !string.IsNullOrEmpty(Id);

	public string Id { get; private set; }

	public string LevelModifierListId { get; private set; }

	public int ItemMinLevel { get; private set; }

	public ItemsListDefinition ItemsListDefinition { get; private set; }

	public ProbabilityTreeEntriesDefinition ItemRaritiesListDefinition { get; private set; }

	public CreateItemDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Id = xElement.Attribute("Id")?.Value;
		XAttribute xAttribute = xElement.Attribute("MinLevel");
		ItemMinLevel = -1;
		if (xAttribute != null)
		{
			if (!int.TryParse(xAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				TPDebug.LogError("CreateItemDefinition " + Id + "'s MinLevel " + HasAnInvalidInt(xAttribute.Value));
				return;
			}
			if (result >= 0)
			{
				ItemMinLevel = result;
			}
		}
		XAttribute xAttribute2 = xElement.Element("ItemsList").Attribute("Id");
		if (!ItemDatabase.ItemsListDefinitions.TryGetValue(xAttribute2.Value, out var value))
		{
			CLoggerManager.Log(xAttribute2.Value + " items list not found!", LogType.Error);
			return;
		}
		ItemsListDefinition = value;
		XElement xElement2 = xElement.Element("BuildingLevelModifiersList");
		if (xElement2 != null)
		{
			LevelModifierListId = xElement2.Attribute("Id")?.Value;
		}
		XElement xElement3 = xElement.Element("ItemRaritiesList");
		if (xElement3 == null)
		{
			CLoggerManager.Log("CreateItem levels missing!", LogType.Assert);
			return;
		}
		XAttribute xAttribute3 = xElement3.Attribute("Id");
		if (xAttribute3.IsNullOrEmpty() || !ItemDatabase.ItemRaritiesListDefinitions.ContainsKey(xAttribute3.Value))
		{
			CLoggerManager.Log("CreateItem ItemRarities Id is not valid or does not exist in ItemRaritiesListDefinitions", LogType.Error);
			return;
		}
		if (!ItemDatabase.ItemRaritiesListDefinitions.TryGetValue(xAttribute3.Value, out var value2))
		{
			CLoggerManager.Log(xAttribute3.Value + " items rarities list not found!", LogType.Error);
			return;
		}
		ItemRaritiesListDefinition = value2;
		XElement xElement4 = xElement.Element("Count");
		Count = (xElement4.IsNullOrEmpty() ? Parser.Parse("1") : Parser.Parse(xElement4.Value));
	}
}
