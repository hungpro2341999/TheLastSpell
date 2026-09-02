using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Trophy;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class TrophyDatabase : Database<TrophyDatabase>
{
	[SerializeField]
	private TextAsset trophiesDefinitions;

	[SerializeField]
	private TextAsset trophyConfig;

	public static DefaultTrophyDefinition DefaultTrophyDefinition { get; private set; }

	public static TrophyConfigDefinition TrophyConfigDefinition { get; private set; }

	public static List<TrophyDefinition> TrophyDefinitions { get; private set; }

	public static TrophyConfigDefinition.GemStageData GetGemStageData(uint damnedSoulsValue)
	{
		foreach (TrophyConfigDefinition.GemStageData gemStageData in TrophyConfigDefinition.GemStageDatas)
		{
			if (damnedSoulsValue >= gemStageData.Min && damnedSoulsValue <= gemStageData.Max)
			{
				return gemStageData;
			}
		}
		return TrophyConfigDefinition.GemStageDatas.Last();
	}

	public override void Deserialize(XContainer container = null)
	{
		DeserializeTrophyDefinitions();
		DeserializeTrophyConfig();
	}

	private void DeserializeTrophyDefinitions()
	{
		if (TrophyDefinitions != null)
		{
			return;
		}
		TrophyDefinitions = new List<TrophyDefinition>();
		XElement xElement = XDocument.Parse(trophiesDefinitions.text, LoadOptions.SetBaseUri).Element("TrophiesDefinitions");
		if (xElement == null)
		{
			CLoggerManager.Log("TrophiesDefinitions.xml doesn't contains an element named : TrophiesDefinitions", LogType.Error);
			return;
		}
		foreach (XElement item2 in xElement.Elements("TrophyDefinition"))
		{
			TrophyDefinition item = new TrophyDefinition(item2);
			TrophyDefinitions.Add(item);
		}
		XElement xElement2 = xElement.Element("DefaultTrophyDefinition");
		if (xElement2 != null)
		{
			DefaultTrophyDefinition = new DefaultTrophyDefinition(xElement2);
		}
	}

	private void DeserializeTrophyConfig()
	{
		if (TrophyConfigDefinition == null)
		{
			XElement xElement = XDocument.Parse(trophyConfig.text, LoadOptions.SetBaseUri).Element("TrophyConfig");
			if (xElement == null)
			{
				CLoggerManager.Log("There is no TrophyConfig Element in " + trophyConfig.name + " text file.", LogType.Error);
			}
			else
			{
				TrophyConfigDefinition = new TrophyConfigDefinition(xElement);
			}
		}
	}
}
