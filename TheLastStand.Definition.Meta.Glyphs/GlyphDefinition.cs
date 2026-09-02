using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs;

public class GlyphDefinition : LocalizableDefinition
{
	private static class Constants
	{
		public const string GlyphTitleLocaPrefix = "GlyphName_";

		public const string GlyphDescriptionLocaPrefix = "GlyphDescription_";
	}

	public int Cost { get; private set; }

	public List<GlyphEffectDefinition> GlyphEffectDefinitions { get; private set; }

	public string Id { get; private set; }

	public bool IsCustom { get; private set; }

	public PerkDefinition PerkToShow { get; private set; }

	public GlyphDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public static void AssertIsTrue(bool value, string message)
	{
		if (!value)
		{
			CLoggerManager.Log(message, LogType.Assert, CLogLevel.MAJOR, forcePrintInUnity: true, "GlyphDefinition");
		}
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		AssertIsTrue(xElement != null, "GlyphDefinition received null element in Deserialize.");
		XAttribute xAttribute = xElement.Attribute("Id");
		AssertIsTrue(xAttribute != null, "Id attribute is missing in GlyphDefinition.");
		Id = xAttribute.Value;
		DeserializeTokenVariables(xElement.Element("TokenVariables"));
		base.Deserialize((XContainer)xElement.Element("LocArguments"));
		XAttribute xAttribute2 = xElement.Attribute("Cost");
		AssertIsTrue(xAttribute2 != null, "Cost attribute is missing in GlyphDefinition.");
		AssertIsTrue(int.TryParse(xAttribute2.Value, out var result), "Cost attribute can't be parsed into an int : \"" + xAttribute2.Value + "\".");
		Cost = result;
		XAttribute xAttribute3 = xElement.Attribute("IsCustom");
		if (xAttribute3 != null && bool.TryParse(xAttribute3.Value, out var result2))
		{
			IsCustom = result2;
		}
		XElement xElement2 = xElement.Element("GlyphEffects");
		AssertIsTrue(xElement2 != null, "GlyphEffects element is missing in GlyphDefinition (" + Id + ")");
		IEnumerable<XElement> enumerable = xElement2.Elements();
		GlyphEffectDefinitions = new List<GlyphEffectDefinition>(enumerable.Count());
		foreach (XElement item in enumerable)
		{
			AddGlyphEffect(item);
		}
	}

	public string GetDescription(InterpreterContext interpreterContext)
	{
		if (base.LocArguments == null)
		{
			return Localizer.Get("GlyphDescription_" + Id);
		}
		return Localizer.Format("GlyphDescription_" + Id, GetArguments(interpreterContext));
	}

	public string GetName()
	{
		return Localizer.Get("GlyphName_" + Id);
	}

	private void AddGlyphEffect(XElement xGlyphEffect)
	{
		GlyphEffectDefinition glyphEffectDefinitionFromName = GetGlyphEffectDefinitionFromName(xGlyphEffect.Name.LocalName, xGlyphEffect);
		if (glyphEffectDefinitionFromName != null)
		{
			if (glyphEffectDefinitionFromName is GlyphNativePerkEffectDefinition { ForceHideTooltip: false } glyphNativePerkEffectDefinition)
			{
				PerkToShow = glyphNativePerkEffectDefinition.PerkDefinition;
			}
			GlyphEffectDefinitions.Add(glyphEffectDefinitionFromName);
		}
		else
		{
			CLoggerManager.Log("GlyphEffectDefinition " + xGlyphEffect.Name.LocalName + " not found.", LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "GlyphManager");
		}
	}

	private GlyphEffectDefinition GetGlyphEffectDefinitionFromName(string name, XElement xGlyphEffect)
	{
		return name switch
		{
			"AddBuildingPassive" => new GlyphAddBuildingPassiveEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"AdditionalRewardReroll" => new GlyphAdditionalRewardRerollEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"BonusUnits" => new GlyphBonusUnitsEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"BonusSellingRatio" => new GlyphBonusSellingRatioEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"DamnedSoulsPercentageModifier" => new GlyphDamnedSoulsPercentageModifierEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"DamnedSoulsScavengingPercentageModifier" => new GlyphDamnedSoulsScavengingPercentageModifierEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"DecreaseEnemiesCount" => new GlyphDecreaseEnemiesCountEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"FreeTrapUsageChances" => new GlyphFreeTrapUsageChancesEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"GoldScavengingPercentageModifier" => new GlyphGoldScavengingPercentageModifierEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"IncreaseBuildingHealth" => new GlyphIncreaseBuildingHealthEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"IncreaseDefensesDamages" => new GlyphIncreaseDefensesDamagesEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"IncreaseLevelupRerolls" => new GlyphIncreaseLevelupRerollsEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"IncreaseStartingGearLevel" => new GlyphIncreaseStartingGearLevelEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"IncreaseStartingResources" => new GlyphIncreaseStartingResourcesEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"MaterialScavengingPercentageModifier" => new GlyphMaterialScavengingPercentageModifierEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyBuildingActionsCost" => new GlyphModifyBuildingActionsCostEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyBuildLimit" => new GlyphModifyBuildLimitEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyCosts" => new GlyphModifyCostsEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyItemRarity" => new GlyphModifyItemRarityEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyLevelProbabilityTree" => new GlyphModifyLevelProbabilityTreeEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyPlayableUnitsStats" => new GlyphModifyPlayableUnitsStatsEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyRarityProbabilityTree" => new GlyphModifyRarityProbabilityTreeEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ModifyRewardsCount" => new GlyphModifyRewardsCountEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"MultiplyItemWeight" => new GlyphMultiplyItemWeightEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"NativePerk" => new GlyphNativePerkEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"NativePerkPointsBonus" => new GlyphNativePerkPointsBonusEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"SetFogCap" => new GlyphSetFogCapEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"ToggleSkillProgressionFlag" => new GlyphToggleSkillProgressionFlagEffectDefinition(xGlyphEffect, base.TokenVariables), 
			"AddStartingGearGenerationPriority" => new GlyphAddStartingGearGenerationPriorityEffectDefinition(xGlyphEffect, base.TokenVariables), 
			_ => null, 
		};
	}
}
