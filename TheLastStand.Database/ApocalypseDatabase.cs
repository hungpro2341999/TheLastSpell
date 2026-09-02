using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Apocalypse;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class ApocalypseDatabase : Database<ApocalypseDatabase>
{
	[SerializeField]
	private TextAsset apocalypsesXmlFile;

	[SerializeField]
	private TextAsset apocalypseModifiersXmlFile;

	[SerializeField]
	private TextAsset apocalypseModifierStepsXmlFile;

	[SerializeField]
	private TextAsset apocalypseTiersXmlFile;

	[SerializeField]
	private TextAsset configurationXmlFile;

	public static ApocalypsesDefinition ApocalypsesDefinition { get; private set; }

	public static ApocalypseConfigurationDefinition ConfigurationDefinition { get; private set; }

	public static Dictionary<string, ApocalypseModifierDefinition> ModifierDefinitions { get; private set; }

	public static Dictionary<string, ApocalypseModifierDefinition> ModifierDefinitionsFromCodeSharingId { get; private set; }

	public static Dictionary<ApocalypseModifierStepDefinition, ApocalypseModifierDefinition> ModifierDefinitionsFromStepDefinitions { get; private set; }

	public static Dictionary<string, ApocalypseModifierStepDefinition> ModifierStepDefinitions { get; private set; }

	public static Dictionary<string, XElement> ModifierStepXmlElements { get; private set; }

	public static List<ApocalypseTierDefinition> OrderedTierDefinitions { get; private set; }

	public static Dictionary<string, ApocalypseTierDefinition> TierDefinitions { get; private set; }

	public static HashSet<string> UsedFilterIds { get; private set; }

	public static ApocalypseDefinition[] ComputeApocalypses(int id)
	{
		if (id == 0)
		{
			return new ApocalypseDefinition[1] { ApocalypsesDefinition.ApocalypseDefinitions[id] };
		}
		ApocalypseDefinition[] array = new ApocalypseDefinition[id];
		for (int i = 1; i <= id; i++)
		{
			array[i - 1] = ApocalypsesDefinition.ApocalypseDefinitions[i];
		}
		return array;
	}

	public static bool DoesApocalypseExist(int id)
	{
		return ApocalypsesDefinition.ApocalypseDefinitions.Find((ApocalypseDefinition x) => x.Id == id) != null;
	}

	public static List<int> GetApocalypseLevelsForRewards()
	{
		List<int> list = new List<int>();
		foreach (ApocalypseTierDefinition orderedTierDefinition in OrderedTierDefinitions)
		{
			if (orderedTierDefinition.ApocalypseLevelCompletedToUnlock > 0 && GetModifiersNbForTier(orderedTierDefinition.Id) > 0)
			{
				list.Add(orderedTierDefinition.ApocalypseLevelCompletedToUnlock);
			}
		}
		return list;
	}

	public static List<ApocalypseModifierDefinition> GetModifiersDefinitionsForTier(string tierId)
	{
		List<ApocalypseModifierDefinition> list = new List<ApocalypseModifierDefinition>();
		foreach (ApocalypseModifierDefinition value in ModifierDefinitions.Values)
		{
			if (value.TierId == tierId)
			{
				list.Add(value);
			}
		}
		return list;
	}

	public static int GetModifiersNbForTier(string tierId)
	{
		int num = 0;
		foreach (ApocalypseModifierDefinition value in ModifierDefinitions.Values)
		{
			if (value.TierId == tierId)
			{
				num++;
			}
		}
		return num;
	}

	public override void Deserialize(XContainer container = null)
	{
		DeserializeTiers();
		DeserializeModifierSteps();
		DeserializeModifiers();
		ApocalypsesDefinition = new ApocalypsesDefinition(XDocument.Parse(apocalypsesXmlFile.text, LoadOptions.SetBaseUri).Element("ApocalypseDefinitions"));
		ConfigurationDefinition = new ApocalypseConfigurationDefinition(XDocument.Parse(configurationXmlFile.text, LoadOptions.SetBaseUri).Element("ApocalypseConfigurationDefinitions"));
	}

	private void DeserializeModifiers()
	{
		if (UsedFilterIds == null)
		{
			UsedFilterIds = new HashSet<string>();
		}
		if (ModifierDefinitions != null)
		{
			return;
		}
		ModifierDefinitions = new Dictionary<string, ApocalypseModifierDefinition>();
		ModifierDefinitionsFromCodeSharingId = new Dictionary<string, ApocalypseModifierDefinition>();
		ModifierDefinitionsFromStepDefinitions = new Dictionary<ApocalypseModifierStepDefinition, ApocalypseModifierDefinition>();
		foreach (XElement item in XDocument.Parse(apocalypseModifiersXmlFile.text, LoadOptions.SetBaseUri).Element("ApocalypseModifierDefinitions").Elements("ApocalypseModifierDefinition"))
		{
			ApocalypseModifierDefinition value = new ApocalypseModifierDefinition(item);
			ModifierDefinitions[item.Attribute("Id").Value] = value;
		}
	}

	private void DeserializeModifierSteps()
	{
		if (ModifierStepDefinitions != null)
		{
			return;
		}
		ModifierStepDefinitions = new Dictionary<string, ApocalypseModifierStepDefinition>();
		ModifierStepXmlElements = new Dictionary<string, XElement>();
		foreach (XElement item in XDocument.Parse(apocalypseModifierStepsXmlFile.text, LoadOptions.SetBaseUri).Element("ApocalypseModifierStepDefinitions").Elements("ApocalypseModifierStepDefinition"))
		{
			string value = item.Attribute("Id").Value;
			ModifierStepDefinitions[value] = new ApocalypseModifierStepDefinition(item);
			ModifierStepXmlElements.Add(value, item);
		}
		ModifierStepXmlElements.Clear();
	}

	private void DeserializeTiers()
	{
		if (TierDefinitions != null)
		{
			return;
		}
		TierDefinitions = new Dictionary<string, ApocalypseTierDefinition>();
		OrderedTierDefinitions = new List<ApocalypseTierDefinition>();
		foreach (XElement item in XDocument.Parse(apocalypseTiersXmlFile.text, LoadOptions.SetBaseUri).Element("ApocalypseTierDefinitions").Elements("ApocalypseTierDefinition"))
		{
			ApocalypseTierDefinition apocalypseTierDefinition = new ApocalypseTierDefinition(item);
			if (!string.IsNullOrEmpty(apocalypseTierDefinition.Id))
			{
				TierDefinitions[apocalypseTierDefinition.Id] = apocalypseTierDefinition;
				OrderedTierDefinitions.Add(apocalypseTierDefinition);
			}
		}
	}
}
