using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Database;

public class SinkDatabase : Database<SinkDatabase>
{
	public static class Constants
	{
		public const string AttributeId = "Attribute";

		public const string ItemRewardId = "ItemReward";

		public const string PerkId = "Perk";
	}

	[SerializeField]
	private TextAsset sinkDataDefinitionsTextAsset;

	public static Dictionary<string, SinkDataDefinition> SinkDataDefinitions { get; private set; }

	public static SinkDataDefinition AttributeSinkDataDefinition { get; private set; }

	public static SinkDataDefinition ItemRewardSinkDataDefinition { get; private set; }

	public static PerkSinkDataDefinition PerkSinkDataDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (SinkDataDefinitions != null)
		{
			return;
		}
		XElement xElement = XDocument.Parse(sinkDataDefinitionsTextAsset.text, LoadOptions.SetBaseUri).Element("SinkDataDefinitions");
		if (xElement.IsNullOrEmpty())
		{
			CLoggerManager.Log("The document " + sinkDataDefinitionsTextAsset.name + " must have SinkDataDefinitions!", LogType.Error);
			return;
		}
		SinkDataDefinitions = new Dictionary<string, SinkDataDefinition>();
		foreach (XElement item in xElement.Elements("SinkDataDefinition"))
		{
			XAttribute xAttribute = item.Attribute("Id");
			if (xAttribute.IsNullOrEmpty())
			{
				TPDebug.LogError("SinkDataDefinition must have an Id.");
				continue;
			}
			switch (xAttribute.Value)
			{
			case "Attribute":
				AttributeSinkDataDefinition = new SinkDataDefinition(item);
				SinkDataDefinitions.Add(AttributeSinkDataDefinition.Id, AttributeSinkDataDefinition);
				break;
			case "ItemReward":
				ItemRewardSinkDataDefinition = new SinkDataDefinition(item);
				SinkDataDefinitions.Add(ItemRewardSinkDataDefinition.Id, ItemRewardSinkDataDefinition);
				break;
			case "Perk":
				PerkSinkDataDefinition = new PerkSinkDataDefinition(item);
				SinkDataDefinitions.Add(PerkSinkDataDefinition.Id, PerkSinkDataDefinition);
				break;
			}
		}
	}
}
