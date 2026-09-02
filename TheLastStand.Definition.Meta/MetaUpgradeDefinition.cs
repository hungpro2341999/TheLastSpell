using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class MetaUpgradeDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_MetaUpgradeCategory
	{
		None = 0,
		Misc = 1,
		City = 2,
		Building = 4,
		Weapon = 8,
		Equipment = 0x10,
		Glyph = 0x20,
		Hero = 0x40,
		All = 0x7F
	}

	[Flags]
	public enum E_MetaUpgradeFilter
	{
		None = 0,
		Acquired = 1,
		Locked = 2,
		NotAcquiredYet = 4,
		New = 8
	}

	public class ConditionsGroup
	{
		public int GroupIndex;

		public bool CheckOnce;

		public List<MetaConditionDefinition> Conditions = new List<MetaConditionDefinition>();
	}

	public E_MetaUpgradeCategory Category { get; private set; }

	public bool DamnedSoulsShop => Price != 0;

	public int DeserializationIndex { get; private set; }

	public string DLCId { get; private set; }

	public bool MandatoryUnlock { get; private set; }

	public bool Hidden { get; private set; }

	public string IconName { get; private set; } = string.Empty;

	public string Id { get; private set; }

	public bool IsLinkedToDLC => !string.IsNullOrEmpty(DLCId);

	public uint Price { get; private set; }

	public List<ConditionsGroup> ActivationConditionsDefinitions { get; } = new List<ConditionsGroup>();

	public List<ConditionsGroup> UnlockConditionsDefinitions { get; } = new List<ConditionsGroup>();

	public List<MetaEffectDefinition> UpgradeEffectDefinitions { get; } = new List<MetaEffectDefinition>();

	public List<string> BuildingActionsToShow { get; private set; } = new List<string>();

	public List<string> BuildingsToShow { get; private set; } = new List<string>();

	public List<string> BuildingUpgradesToShow { get; private set; } = new List<string>();

	public List<string> GlyphsToShow { get; private set; } = new List<string>();

	public List<string> ItemsToShow { get; private set; } = new List<string>();

	public MetaUpgradeDefinition(XContainer container, int deserializationIndex)
		: base(container)
	{
		DeserializationIndex = deserializationIndex;
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Id = xElement.Attribute("Id").Value;
		IconName = xElement.Element("IconName")?.Value ?? Id;
		Hidden = xElement.Element("Hidden") != null;
		Price = uint.Parse(xElement.Attribute("Price")?.Value ?? Price.ToString());
		MandatoryUnlock = xElement.Element("MandatoryUnlock") != null;
		XAttribute xAttribute = xElement.Attribute("DLCId");
		if (xAttribute != null)
		{
			DLCId = xAttribute.Value;
		}
		XElement xElement2 = xElement.Element("UnlockConditions");
		if (xElement2 != null)
		{
			DeserializeConditions(xElement2, UnlockConditionsDefinitions);
		}
		XElement xElement3 = xElement.Element("ActivationConditions");
		if (xElement3 != null)
		{
			DeserializeConditions(xElement3, ActivationConditionsDefinitions);
		}
		XElement xElement4 = xElement.Element("UpgradeEffects");
		if (xElement4 != null)
		{
			foreach (XElement item in xElement4.Elements())
			{
				switch (item.Name.LocalName)
				{
				case "AdditionalInitMages":
					UpgradeEffectDefinitions.Add(new AdditionalInitMagesMetaEffectDefinition(item));
					break;
				case "AdditionalRerollReward":
					UpgradeEffectDefinitions.Add(new AdditionalRerollRewardMetaEffectDefinition(item));
					break;
				case "BuildingModifier":
					UpgradeEffectDefinitions.Add(new BuildingModifierMetaEffectDefinition(item));
					break;
				case "CreateItemModifier":
					UpgradeEffectDefinitions.Add(new CreateItemModifierMetaEffectDefinition(item));
					break;
				case "FogModifier":
					UpgradeEffectDefinitions.Add(new FogModifierMetaEffectDefinition(item));
					break;
				case "InitResourcesBonus":
					UpgradeEffectDefinitions.Add(new InitResourcesBonusMetaEffectDefinition(item));
					break;
				case "ItemLevelProbabilityModifier":
					UpgradeEffectDefinitions.Add(new ItemLevelProbabilityMetaEffectDefinition(item));
					break;
				case "ItemRaritiesModifier":
					UpgradeEffectDefinitions.Add(new ItemRaritiesMetaEffectDefinition(item));
					break;
				case "LockItems":
					UpgradeEffectDefinitions.Add(new LockItemsMetaEffectDefinition(item));
					break;
				case "NewEnemy":
					UpgradeEffectDefinitions.Add(new NewEnemyMetaEffectDefinition(item));
					break;
				case "TraitsParameters":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new TraitsParametersMetaEffectDefinition(item));
					break;
				case "PlayableUnitAttributeModifier":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnitAttributeModifierMetaEffectDefinition(item));
					break;
				case "UnlockAffixes":
					UpgradeEffectDefinitions.Add(new UnlockAffixesMetaEffectDefinition(item));
					break;
				case "UnlockBuildingAction":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingActionMetaEffectDefinition unlockBuildingActionMetaEffectDefinition = new UnlockBuildingActionMetaEffectDefinition(item);
					BuildingActionsToShow.Add(unlockBuildingActionMetaEffectDefinition.BuildingActionId);
					UpgradeEffectDefinitions.Add(unlockBuildingActionMetaEffectDefinition);
					break;
				}
				case "UnlockBuilding":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingMetaEffectDefinition unlockBuildingMetaEffectDefinition = new UnlockBuildingMetaEffectDefinition(item);
					BuildingsToShow.Add(unlockBuildingMetaEffectDefinition.BuildingId);
					UpgradeEffectDefinitions.Add(unlockBuildingMetaEffectDefinition);
					break;
				}
				case "UnlockBuildingUpgrade":
				{
					Category |= E_MetaUpgradeCategory.Building;
					UnlockBuildingUpgradeMetaEffectDefinition unlockBuildingUpgradeMetaEffectDefinition = new UnlockBuildingUpgradeMetaEffectDefinition(item);
					BuildingUpgradesToShow.Add(unlockBuildingUpgradeMetaEffectDefinition.UpgradeId);
					UpgradeEffectDefinitions.Add(unlockBuildingUpgradeMetaEffectDefinition);
					break;
				}
				case "UnlockEquipmentGeneration":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockEquipmentGenerationMetaEffectDefinition(item));
					break;
				case "UnlockItems":
				{
					UnlockItemsMetaEffectDefinition unlockItemsMetaEffectDefinition = new UnlockItemsMetaEffectDefinition(item);
					foreach (string item2 in unlockItemsMetaEffectDefinition.ItemsToUnlock)
					{
						if (ItemDatabase.ItemDefinitions.TryGetValue(item2, out var value))
						{
							if (value.IsWeapon)
							{
								Category |= E_MetaUpgradeCategory.Weapon;
							}
							else
							{
								Category |= E_MetaUpgradeCategory.Equipment;
							}
						}
					}
					ItemsToShow.AddRange(unlockItemsMetaEffectDefinition.ItemsToUnlock);
					UpgradeEffectDefinitions.Add(unlockItemsMetaEffectDefinition);
					break;
				}
				case "UnlockCities":
					Category |= E_MetaUpgradeCategory.City;
					UpgradeEffectDefinitions.Add(new UnlockCitiesMetaEffectDefinition(item));
					break;
				case "UnlockGlyphs":
				{
					Category |= E_MetaUpgradeCategory.Glyph;
					UnlockGlyphsMetaEffectDefinition unlockGlyphsMetaEffectDefinition = new UnlockGlyphsMetaEffectDefinition(item);
					GlyphsToShow.AddRange(unlockGlyphsMetaEffectDefinition.GlyphIds);
					UpgradeEffectDefinitions.Add(unlockGlyphsMetaEffectDefinition);
					break;
				}
				case "UnlockPerkCollectionSlots":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockPerkCollectionSlotsMetaEffectDefinition(item));
					break;
				case "UnlockRaces":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockRacesMetaEffectDefinition(item));
					break;
				case "UnlockRerollReward":
					UpgradeEffectDefinitions.Add(new UnlockRerollRewardMetaEffectDefinition(item));
					break;
				case "UnlockShopReroll":
					UpgradeEffectDefinitions.Add(new UnlockShopRerollMetaEffectDefinition(item));
					break;
				case "UnlockSink":
					UpgradeEffectDefinitions.Add(new UnlockSinkMetaEffectDefinition(item));
					break;
				case "UnlockTraits":
					Category |= E_MetaUpgradeCategory.Hero;
					UpgradeEffectDefinitions.Add(new UnlockTraitsMetaEffectDefinition(item));
					break;
				case "UnlockWaves":
					UpgradeEffectDefinitions.Add(new UnlockWavesMetaEffectDefinition(item));
					break;
				case "UpgradeCity":
					UpgradeEffectDefinitions.Add(new UpgradeCityMetaEffectDefinition(item));
					break;
				case "WavesParameters":
					UpgradeEffectDefinitions.Add(new WavesParametersMetaEffectDefinition(item));
					break;
				default:
					CLoggerManager.Log("MetaUpgrade effect " + item.Name.LocalName + " is not handled to be parsed as a valid definition!", LogType.Error);
					break;
				}
			}
			XElement xElement5 = xElement.Element("ForceDisplayTooltips");
			if (xElement5 != null)
			{
				foreach (XElement item3 in xElement5.Elements())
				{
					XAttribute xAttribute2 = item3.Attribute("Id");
					switch (item3.Name.LocalName)
					{
					case "ItemTooltip":
						ItemsToShow.Add(xAttribute2.Value);
						break;
					case "GlyphTooltip":
						GlyphsToShow.Add(xAttribute2.Value);
						break;
					case "BuildingTooltip":
						BuildingsToShow.Add(xAttribute2.Value);
						break;
					case "BuildingActionTooltip":
						BuildingActionsToShow.Add(xAttribute2.Value);
						break;
					case "BuildingUpgradeTooltip":
						BuildingUpgradesToShow.Add(xAttribute2.Value);
						break;
					}
				}
			}
			XElement xElement6 = xElement.Element("ForceHideTooltips");
			if (xElement6 != null)
			{
				foreach (XElement item4 in xElement6.Elements())
				{
					XAttribute xAttribute3 = item4.Attribute("Id");
					switch (item4.Name.LocalName)
					{
					case "ItemTooltip":
						ItemsToShow.Remove(xAttribute3.Value);
						break;
					case "GlyphTooltip":
						GlyphsToShow.Remove(xAttribute3.Value);
						break;
					case "BuildingTooltip":
						BuildingsToShow.Remove(xAttribute3.Value);
						break;
					case "BuildingActionTooltip":
						BuildingActionsToShow.Remove(xAttribute3.Value);
						break;
					case "BuildingUpgradeTooltip":
						BuildingUpgradesToShow.Remove(xAttribute3.Value);
						break;
					}
				}
			}
			XElement xElement7 = xElement.Element("Categories");
			if (xElement7 != null)
			{
				XAttribute xAttribute4 = xElement7.Attribute("OverrideAutomaticCategories");
				bool result = default(bool);
				if (xAttribute4 != null && bool.TryParse(xAttribute4.Value, out result) && result)
				{
					Category = E_MetaUpgradeCategory.None;
				}
				foreach (XElement item5 in xElement7.Elements("Category"))
				{
					XAttribute xAttribute5 = item5.Attribute("Value");
					if (Enum.TryParse<E_MetaUpgradeCategory>(xAttribute5.Value, out var result2))
					{
						Category |= result2;
					}
					else
					{
						CLoggerManager.Log("Could not parse Category attribute into a meta upgrade category in meta upgrade " + Id + " : " + xAttribute5.Value);
					}
				}
			}
			if (Category == E_MetaUpgradeCategory.None)
			{
				Category = E_MetaUpgradeCategory.Misc;
			}
			if (Category == E_MetaUpgradeCategory.Misc)
			{
				CLoggerManager.Log("Meta upgrade doesn't have a category except Misc. This shouldn't happen ! (" + Id + ").");
			}
		}
		else
		{
			CLoggerManager.Log("MetaUpgrade " + Id + " doesn't have UpgradeEffects element!", LogType.Error);
		}
	}

	public override string ToString()
	{
		string log = "<b>#--- " + Id + (Hidden ? "(Hidden)" : "") + " ---#</b>\n";
		log += "Unlock Conditions Groups :\n";
		for (int i = 0; i < UnlockConditionsDefinitions.Count; i++)
		{
			log += $"Group {i + 1}:\n";
			UnlockConditionsDefinitions[i].Conditions.ForEach(delegate(MetaConditionDefinition o)
			{
				log += $"- {o}\n";
			});
		}
		log += "Activation Conditions Groups :\n";
		for (int num = 0; num < ActivationConditionsDefinitions.Count; num++)
		{
			log += $"Group {num + 1}:\n";
			ActivationConditionsDefinitions[num].Conditions.ForEach(delegate(MetaConditionDefinition o)
			{
				log += $"- {o}\n";
			});
		}
		log += "Effects :\n";
		UpgradeEffectDefinitions.ForEach(delegate(MetaEffectDefinition o)
		{
			log += $"- {o}\n";
		});
		return log;
	}

	private void DeserializeConditions(XElement conditionsElement, List<ConditionsGroup> conditionsDefinitions)
	{
		int num = 0;
		int num2 = 0;
		foreach (XElement item in conditionsElement.Elements("ConditionsGroup"))
		{
			bool flag = item.Element("Hidden") != null;
			if (!flag)
			{
				num2++;
			}
			bool checkOnce = item.Element("CheckOnce") != null;
			ConditionsGroup conditionsGroup = new ConditionsGroup
			{
				CheckOnce = checkOnce,
				GroupIndex = num
			};
			foreach (XElement item2 in item.Elements())
			{
				if (!(item2.Name.LocalName == "Hidden") && !(item2.Name.LocalName == "CheckOnce"))
				{
					try
					{
						conditionsGroup.Conditions.Add(new MetaConditionDefinition(item2, flag, num));
					}
					catch (Exception arg)
					{
						CLoggerManager.Log($"Caught and skipped invalid or obsolete condition definition in MetaUpgrade {Id}:\n{arg}", LogType.Error);
					}
				}
			}
			conditionsDefinitions.Add(conditionsGroup);
			num++;
		}
		if (num2 >= 2)
		{
			CLoggerManager.Log("More than one unlock/activation conditions groups are shown in MetaUpgrade " + Id + ". At most ONE must be visible.", LogType.Error);
		}
	}
}
