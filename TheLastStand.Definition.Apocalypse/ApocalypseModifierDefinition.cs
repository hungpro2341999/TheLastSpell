using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.WorldMap;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypseModifierDefinition : LocalizableDefinition, IFilterable
{
	public static class Constants
	{
		public const string DefenseCostModifierId = "DefenseCostModifier";

		public const string EnemyHealthModifierId = "EnemyHealthModifier";

		public const string FasterEnemiesModifierId = "FasterEnemiesModifier";

		public const string FogSpawnerModifierId = "FogSpawnerModifier";

		public const string ItemsNegativeAffixesModifierId = "ItemsNegativeAffixesModifier";

		public const string ProductionCostModifierId = "ProductionCostModifier";

		public const string WaveSizeModifierId = "WaveSizeModifier";

		public const string CodeSharingIdValidationRegex = "[A-Z]*";
	}

	public string CodeSharingId { get; private set; }

	public HashSet<string> FilterIds { get; private set; }

	public string Id { get; private set; }

	public List<ApocalypseModifierStepDefinition> StepDefinitions { get; } = new List<ApocalypseModifierStepDefinition>();

	public string TierId { get; private set; }

	public ApocalypseModifierDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		XElement xElement2 = xElement.Element("Tier");
		if (!string.IsNullOrEmpty(xElement2?.Value))
		{
			string value = xElement2.Value;
			if (!ApocalypseDatabase.TierDefinitions.ContainsKey(value))
			{
				CLoggerManager.Log("Couldn't find apocalypse tier definition with id '" + value + "' defined in modifier '" + Id + "'", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierDefinition");
			}
			TierId = value;
		}
		else
		{
			CLoggerManager.Log("Apocalypse tier for modifier '" + Id + "' is null or empty !", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierDefinition");
		}
		XElement xTokenVariables = xElement.Element("TokenVariables");
		DeserializeTokenVariables(xTokenVariables);
		XElement container2 = xElement.Element("LocArguments");
		base.Deserialize((XContainer)container2);
		XElement xElement3 = xElement.Element("Steps");
		if (xElement3 != null)
		{
			foreach (XElement item in xElement3.Elements())
			{
				string value2 = item.Value;
				if (!ApocalypseDatabase.ModifierStepDefinitions.TryGetValue(value2, out var value3))
				{
					CLoggerManager.Log("Could not find step id '" + value2 + "' for apocalypse modifier '" + Id + "'", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierDefinition");
				}
				else
				{
					if (!ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.ContainsKey(value3))
					{
						ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.Add(value3, this);
					}
					StepDefinitions.Add(value3);
				}
			}
		}
		FilterIds = new HashSet<string>();
		XElement xElement4 = xElement.Element("FilterIds");
		if (xElement4 != null)
		{
			foreach (XElement item2 in xElement4.Elements("FilterId"))
			{
				if (item2.IsNullOrEmpty())
				{
					CLoggerManager.Log("FilterId is null or empty for modifier: " + Id, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierDefinition");
					continue;
				}
				string value4 = item2.Value;
				if (!GenericDatabase.FilterDefinitions.TryGetValue(value4, out var value5))
				{
					CLoggerManager.Log("Could not find filter id '" + value4 + "' for apocalypse modifier '" + Id + "'", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierDefinition");
				}
				else
				{
					FilterIds.Add(value5.Id);
					ApocalypseDatabase.UsedFilterIds.Add(value5.Id);
				}
			}
		}
		XAttribute xAttribute2 = xElement.Attribute("CodeSharingId");
		if (xAttribute2.IsNullOrEmpty())
		{
			CodeSharingId = Id.GetInitials();
		}
		else
		{
			CodeSharingId = xAttribute2.Value;
		}
		if (!(Regex.Match(CodeSharingId, "[A-Z]*").Value == CodeSharingId))
		{
			CLoggerManager.Log("Invalid CodeSharingId '" + CodeSharingId + "' ! modifier with id '" + Id + "' shouldn't have any lower case characters, number nor space, only uppercase characters are valid.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierDefinition");
			CodeSharingId = string.Empty;
		}
		else if (!ApocalypseDatabase.ModifierDefinitionsFromCodeSharingId.ContainsKey(CodeSharingId))
		{
			ApocalypseDatabase.ModifierDefinitionsFromCodeSharingId.Add(CodeSharingId, this);
		}
		else
		{
			CLoggerManager.Log("Duplicate CodeSharingId '" + CodeSharingId + "' ! modifier with id '" + Id + "' already matches an existing CodeSharingId for modifier '" + ApocalypseDatabase.ModifierDefinitionsFromCodeSharingId[CodeSharingId].Id + "', this id must be unique !", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierDefinition");
			CodeSharingId = string.Empty;
		}
	}

	public string DebugToString()
	{
		return $"Id: {Id}\nApocalypseTier: {TierId}\nTokenVariables nb: {base.TokenVariables?.Count}\nLocArguments nb: {base.LocArguments.Count}\nSteps nb: {StepDefinitions.Count}";
	}

	public int GetHighestCompletedStepIndex()
	{
		int num = -1;
		int count = StepDefinitions.Count;
		foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
		{
			if (city.CompletedApocalypseModifiersStepIndex.ContainsKey(Id))
			{
				int num2 = city.CompletedApocalypseModifiersStepIndex[Id];
				if (num < num2)
				{
					num = num2;
				}
				if (num == count - 1)
				{
					return num;
				}
			}
		}
		return num;
	}

	public int GetModifierStepIndexFromDefinition(ApocalypseModifierStepDefinition modifierStepDefinition)
	{
		return StepDefinitions.IndexOf(modifierStepDefinition);
	}

	public string GetLocalizedDescription(int selectedStepIndex = -1)
	{
		ApocalypseModifierStepDefinition apocalypseModifierStepDefinition = null;
		if (selectedStepIndex == -1)
		{
			if (StepDefinitions.Count > 0)
			{
				apocalypseModifierStepDefinition = StepDefinitions[0];
			}
		}
		else if (selectedStepIndex < StepDefinitions.Count)
		{
			apocalypseModifierStepDefinition = StepDefinitions[selectedStepIndex];
		}
		if (apocalypseModifierStepDefinition != null)
		{
			return apocalypseModifierStepDefinition.GetLocalizedDescription(this);
		}
		return string.Empty;
	}

	public string GetLocalizedTitle()
	{
		return Localizer.Get("ApocalypseModifierTitle_" + Id);
	}
}
