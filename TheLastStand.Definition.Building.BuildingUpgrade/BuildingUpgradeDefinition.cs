using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.BuildingPassive;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingUpgrade;

public class BuildingUpgradeDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class LeveledBuildingUpgradeDefinition : TheLastStand.Framework.Serialization.Definition
	{
		private int goldCost;

		private int materialCost;

		public List<BuildingUpgradeEffectDefinition> BuildingUpgradeEffectDefinitions { get; private set; }

		public int DefaultGoldCost => goldCost;

		public int DefaultMaterialsCost => materialCost;

		public int GoldCost
		{
			get
			{
				int num = ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.BuildingUpgrades, ResourceManager.E_ResourceType.Gold);
				return goldCost + Mathf.RoundToInt((float)(goldCost * num) / 100f);
			}
			private set
			{
				goldCost = value;
			}
		}

		public int MaterialCost
		{
			get
			{
				int num = ResourceManager.ComputeExtraPercentageForCost(ResourceManager.E_PriceModifierType.BuildingUpgrades, ResourceManager.E_ResourceType.Materials);
				return materialCost + Mathf.RoundToInt((float)(materialCost * num) / 100f);
			}
			private set
			{
				materialCost = value;
			}
		}

		public CastFxDefinition OverrideCastFxDefinition { get; set; }

		public LeveledBuildingUpgradeDefinition(XContainer container)
			: base(container)
		{
		}

		public override void Deserialize(XContainer container)
		{
			XElement xElement = container as XElement;
			XElement xElement2 = xElement.Element("GoldCost");
			if (!xElement2.IsNullOrEmpty())
			{
				if (!int.TryParse(xElement2.Value, out var result))
				{
					TPDebug.LogError("BuildingUpgradeDefinition UpgradeLevel must have a valid GoldCost (int)");
					return;
				}
				GoldCost = result;
			}
			XElement xElement3 = xElement.Element("MaterialCost");
			if (!xElement3.IsNullOrEmpty())
			{
				if (!int.TryParse(xElement3.Value, out var result2))
				{
					TPDebug.LogError("BuildingUpgradeDefinition UpgradeLevel must have a valid MaterialCost (int)");
					return;
				}
				MaterialCost = result2;
			}
			XElement xElement4 = xElement.Element("UpgradeEffects");
			if (xElement4 == null)
			{
				TPDebug.LogError("BuildingUpgradeDefinition UpgradeLevel must have a UpgradeEffects");
				return;
			}
			BuildingUpgradeEffectDefinitions = new List<BuildingUpgradeEffectDefinition>();
			foreach (XElement item12 in xElement4.Elements("UpgradeEffect"))
			{
				XAttribute xAttribute = item12.Attribute("Id");
				if (xAttribute.IsNullOrEmpty())
				{
					TPDebug.LogError("BuildingUpgradeEffectDefinition must have an Id");
					return;
				}
				switch (xAttribute.Value)
				{
				case "UnlockAction":
				{
					UnlockActionDefinition item11 = new UnlockActionDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item11);
					break;
				}
				case "SwapAction":
				{
					SwapActionDefinition item10 = new SwapActionDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item10);
					break;
				}
				case "ImproveGaugeEffect":
				{
					ImproveGaugeEffectDefinition item9 = new ImproveGaugeEffectDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item9);
					break;
				}
				case "ImproveGuessWho":
				{
					ImproveGuessWhoDefinition item8 = new ImproveGuessWhoDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item8);
					break;
				}
				case "ImprovePassive":
				{
					ImprovePassiveDefinition item7 = new ImprovePassiveDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item7);
					break;
				}
				case "ImproveLevel":
				{
					ImproveLevelDefinition item6 = new ImproveLevelDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item6);
					break;
				}
				case "ImproveUnitLimit":
				{
					ImproveUnitLimitDefinition item5 = new ImproveUnitLimitDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item5);
					break;
				}
				case "ImproveSellRatio":
				{
					ImproveSellRatioDefinition item4 = new ImproveSellRatioDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item4);
					break;
				}
				case "ReplaceBuilding":
				{
					ReplaceBuildingDefinition item3 = new ReplaceBuildingDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item3);
					break;
				}
				case "SwapSkill":
				{
					SwapSkillDefinition item2 = new SwapSkillDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item2);
					break;
				}
				case "IncreasePlaysPerTurn":
				{
					IncreasePlaysPerTurnDefinition item = new IncreasePlaysPerTurnDefinition(item12);
					BuildingUpgradeEffectDefinitions.Add(item);
					break;
				}
				}
			}
			XElement xElement5 = xElement.Element("CastFXs");
			if (xElement5 != null)
			{
				OverrideCastFxDefinition = new CastFxDefinition(xElement5);
			}
		}
	}

	public CastFxDefinition CastFxDefinition { get; set; }

	public string Id { get; private set; }

	public bool IsGlobal { get; private set; }

	public List<string> LinkedUpgradesIds { get; private set; }

	public List<LeveledBuildingUpgradeDefinition> LeveledBuildingUpgradeDefinitions { get; private set; }

	public string LoreDescription => string.Empty;

	public BuildingUpgradeDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer xContainer)
	{
		XElement xElement = xContainer as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			TPDebug.LogError("BuildingUpgradeDefinition must have an Id");
			return;
		}
		Id = xAttribute.Value;
		XAttribute xAttribute2 = xElement.Attribute("IsGlobal");
		if (!xAttribute2.IsNullOrEmpty())
		{
			IsGlobal = bool.Parse(xAttribute2.Value);
		}
		XElement xElement2 = xElement.Element("UpgradeLevels");
		if (xElement2 == null)
		{
			TPDebug.LogError("BuildingUpgradeDefinition " + Id + " must have a UpgradeLevels");
			return;
		}
		LeveledBuildingUpgradeDefinitions = new List<LeveledBuildingUpgradeDefinition>();
		foreach (XElement item2 in xElement2.Elements("UpgradeLevel"))
		{
			LeveledBuildingUpgradeDefinition item = new LeveledBuildingUpgradeDefinition(item2);
			LeveledBuildingUpgradeDefinitions.Add(item);
		}
		LinkedUpgradesIds = new List<string>();
		XElement xElement3 = xElement.Element("LinkedUpgrades");
		if (xElement3 != null)
		{
			foreach (XElement item3 in xElement3.Elements("LinkedUpgrade"))
			{
				if (item3.Attribute("Id").IsNullOrEmpty())
				{
					TPDebug.LogError("BuildingUpgradeDefinition " + Id + " LinkedUpgrades must all have an Id");
					return;
				}
				LinkedUpgradesIds.Add(item3.Attribute("Id").Value);
			}
		}
		XElement xElement4 = xElement.Element("CastFXs");
		if (xElement4 != null)
		{
			CastFxDefinition = new CastFxDefinition(xElement4);
		}
	}

	public string GetDescriptionAtLevel(int level, TheLastStand.Model.Building.Building building, bool isAlreadyActive)
	{
		LeveledBuildingUpgradeDefinition leveledBuildingUpgradeDefinition = LeveledBuildingUpgradeDefinitions[level];
		if (building == null)
		{
			return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level));
		}
		int num = 0;
		switch (leveledBuildingUpgradeDefinition.BuildingUpgradeEffectDefinitions[0].Id)
		{
		case "ImprovePassive":
		{
			ImprovePassiveDefinition improvePassiveDefinition = leveledBuildingUpgradeDefinition.BuildingUpgradeEffectDefinitions[0] as ImprovePassiveDefinition;
			if (building.PassivesModule?.BuildingPassives == null)
			{
				break;
			}
			FillEffectGauge fillEffectGauge = null;
			for (int i = 0; i < building.PassivesModule.BuildingPassives.Count; i++)
			{
				int num3 = 0;
				FillEffectGauge fillEffectGauge2;
				while (num3 < building.PassivesModule.BuildingPassives[i].PassiveEffects.Count)
				{
					fillEffectGauge2 = building.PassivesModule.BuildingPassives[i].PassiveEffects[num3] as FillEffectGauge;
					if (fillEffectGauge2 == null)
					{
						num3++;
						continue;
					}
					goto IL_00d0;
				}
				continue;
				IL_00d0:
				fillEffectGauge = fillEffectGauge2;
				break;
			}
			if (fillEffectGauge == null)
			{
				return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level));
			}
			num = improvePassiveDefinition.Value.EvalToInt();
			int num4 = (isAlreadyActive ? (fillEffectGauge.CurrentValue - num) : fillEffectGauge.CurrentValue);
			num4 /= building.ProductionModule.BuildingGaugeEffect.UnitsThreshold;
			int num5 = (isAlreadyActive ? fillEffectGauge.CurrentValue : (fillEffectGauge.CurrentValue + num));
			num5 /= building.ProductionModule.BuildingGaugeEffect.UnitsThreshold;
			return Localizer.Format(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level), num4, num5);
		}
		case "ImproveGaugeEffect":
		{
			ImproveGaugeEffectDefinition improveGaugeEffectDefinition = leveledBuildingUpgradeDefinition.BuildingUpgradeEffectDefinitions[0] as ImproveGaugeEffectDefinition;
			TheLastStand.Model.Building.BuildingGaugeEffect.BuildingGaugeEffect buildingGaugeEffect = building.ProductionModule?.BuildingGaugeEffect;
			if (buildingGaugeEffect != null)
			{
				num = improveGaugeEffectDefinition.Value;
				float num2 = 0f;
				switch (buildingGaugeEffect.BuildingGaugeEffectDefinition.Id)
				{
				case "GainGold":
				{
					int upgradedBonusValue2 = (building.ProductionModule.BuildingGaugeEffect as GainGold).UpgradedBonusValue;
					num2 = (buildingGaugeEffect.BuildingGaugeEffectDefinition as GainGoldDefinition).GoldGain.EvalToFloat(new FormulaInterpreterContext());
					num2 += (float)upgradedBonusValue2;
					break;
				}
				case "GainMaterials":
				{
					int upgradedBonusValue = (building.ProductionModule.BuildingGaugeEffect as GainMaterials).UpgradedBonusValue;
					num2 = (buildingGaugeEffect.BuildingGaugeEffectDefinition as GainMaterialsDefinition).MaterialsGain.EvalToFloat();
					num2 += (float)upgradedBonusValue;
					break;
				}
				}
				return Localizer.Format(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level), isAlreadyActive ? (num2 - (float)num) : num2, isAlreadyActive ? num2 : (num2 + (float)num));
			}
			break;
		}
		}
		return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipDescription_", Id, level));
	}

	public string GetNameAtLevel(int level)
	{
		return Localizer.Get(string.Format("{0}{1}{2}", "BuildingUpgradeTooltipName_", Id, level));
	}
}
