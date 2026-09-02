using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Apocalypse.ApocalypseEffects;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class ApocalypseModifierStepDefinition : LocalizableDefinition
{
	private static class Constants
	{
		public const string ViewDropdownValueName = "DropdownValue";

		public const string ViewTooltipStepValueName = "TooltipStepValue";
	}

	private class StepViewDropdownValue
	{
		public string Prefix { get; }

		public Node ValueExpression { get; }

		public string Suffix { get; }

		public StepViewDropdownValue(string prefix, Node valueExpression, string suffix)
		{
			Prefix = prefix;
			ValueExpression = valueExpression;
			Suffix = suffix;
		}

		public string GetValue()
		{
			return $"{Prefix}{ValueExpression.Eval(null)}{Suffix}";
		}
	}

	private class StepViewTooltipStepValue
	{
		public bool UseDropdownValue { get; private set; }

		public bool UseLocArgument { get; private set; }

		public int LocArgumentIndex { get; private set; }

		public StepViewTooltipStepValue(bool useDropdownValue, bool useLocArgument, int locArgumentIndex)
		{
			UseDropdownValue = useDropdownValue;
			UseLocArgument = useLocArgument;
			LocArgumentIndex = locArgumentIndex;
		}
	}

	private string cachedCodeSharing;

	private StepViewDropdownValue stepViewDropdownValue;

	private StepViewTooltipStepValue stepViewTooltipStepValue;

	public int ApocalypseLevel { get; private set; }

	public List<ApocalypseEffectDefinition> Effects { get; private set; } = new List<ApocalypseEffectDefinition>();

	public string Id { get; private set; }

	public string TemplateId { get; protected set; }

	public string ViewDropdownValue
	{
		get
		{
			if (stepViewDropdownValue == null)
			{
				return string.Empty;
			}
			return stepViewDropdownValue.GetValue();
		}
	}

	public string ViewTooltipStepValue
	{
		get
		{
			if (stepViewTooltipStepValue == null)
			{
				return string.Empty;
			}
			return GetTooltipStepValue();
		}
	}

	public ApocalypseModifierStepDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		ApocalypseModifierStepDefinition value = null;
		XElement value2 = null;
		XAttribute xAttribute = xElement.Attribute("TemplateId");
		if (xAttribute != null)
		{
			TemplateId = xAttribute.Value;
			if (!ApocalypseDatabase.ModifierStepDefinitions.TryGetValue(TemplateId, out value))
			{
				LogError("ApocalypseModifierStep couldn't find a template definition with id: " + TemplateId + "!");
			}
			if (!ApocalypseDatabase.ModifierStepXmlElements.TryGetValue(TemplateId, out value2))
			{
				LogError("ApocalypseModifierStep couldn't find a template xmlElement with id: " + TemplateId + "!");
			}
		}
		XElement xElement2 = xElement.Element("ApocalypseLevel");
		if (xElement2 != null)
		{
			if (!int.TryParse(xElement2.Value, out var result))
			{
				LogError("Could not parse ApocalypseLevel attribute into an int");
				ApocalypseLevel = 1;
			}
			ApocalypseLevel = result;
		}
		else if (value != null)
		{
			ApocalypseLevel = value.ApocalypseLevel;
		}
		XAttribute xAttribute2 = xElement.Attribute("Id");
		Id = xAttribute2.Value;
		XElement xElement3 = xElement.Element("TokenVariables");
		if (xElement3 != null)
		{
			DeserializeTokenVariables(xElement3);
		}
		else if (value2 != null)
		{
			DeserializeTokenVariables(value2.Element("TokenVariables"));
		}
		XElement xElement4 = xElement.Element("LocArguments");
		if (xElement4 != null)
		{
			base.Deserialize((XContainer)xElement4);
		}
		else if (value2 != null)
		{
			base.Deserialize((XContainer)value2.Element("LocArguments"));
		}
		XElement xElement5 = xElement.Element("Effects");
		if (xElement5 == null && value2 != null)
		{
			xElement5 = value2.Element("Effects");
		}
		if (xElement5 != null)
		{
			foreach (XElement item in xElement5.Elements())
			{
				switch (item.Name.LocalName)
				{
				case "AddAffixFlag":
					Effects.Add(new AddAffixFlagApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "AddEnemiesStatModifierFromTurn":
					Effects.Add(new AddEnemiesStatModifierFromTurnApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "AddSkillProgressionFlag":
					Effects.Add(new AddSkillProgressionFlagApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "EnemiesStatBaseValueModifier":
					Effects.Add(new EnemiesStatBaseValueModifierApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "ForbidBuildingCategoryAroundMagicCircle":
					Effects.Add(new ForbidBuildingCategoryAroundMagicCircleApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "GenerateFogSpawners":
					Effects.Add(new GenerateFogSpawnersApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "GenerateMalusAffixes":
					Effects.Add(new GenerateMalusAffixesApocalypseEffectDefinition(item));
					break;
				case "IncreaseDailyFogUpdateFrequency":
					Effects.Add(new IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "IncreaseEnemiesNumber":
					Effects.Add(new IncreaseEnemiesNumberApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "IncreaseEnemiesProgressionOffset":
					Effects.Add(new IncreaseEnemiesProgressionOffsetApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "IncreasePrices":
					Effects.Add(new IncreasePricesApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "IncreaseStartingFogDensity":
					Effects.Add(new IncreaseStartingFogDensityApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "ModifyBuildingsDeadZoneRange":
					Effects.Add(new ModifyBuildingsDeadZoneRangeApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "ModifyEnemiesInjuryStage":
					Effects.Add(new ModifyEnemiesInjuryStageApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "ModifyInnSlotRecruitmentLevel":
					Effects.Add(new ModifyInnSlotRecruitmentLevelApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "ModifyMagicCircleStartingHealth":
					Effects.Add(new ModifyMagicCircleStartingHealthApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "MultiplyEnemyUnitSpawnWaveWeight":
					Effects.Add(new MultiplyEnemyUnitSpawnWaveWeightApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "MultiplyPanicGain":
					Effects.Add(new MultiplyPanicGainApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "PlayableUnitBlockLineOfSight":
					Effects.Add(new PlayableUnitBlockLineOfSightApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "RemoveStartingPlayableUnit":
					Effects.Add(new RemoveStartingPlayableUnitApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				case "SetBuildingsNotDemolishable":
					Effects.Add(new SetBuildingsNotDemolishableApocalypseEffectDefinition(item, base.TokenVariables));
					break;
				default:
					LogError("Unhandled ApocalypseModifierStep effect name " + item.Name.LocalName + "!");
					break;
				}
			}
		}
		XElement xElement6 = xElement.Element("View");
		if (xElement6 == null && value2 != null)
		{
			xElement6 = value2.Element("View");
		}
		if (xElement6 == null)
		{
			return;
		}
		foreach (XElement item2 in xElement6.Elements())
		{
			switch (item2.Name.LocalName)
			{
			case "DropdownValue":
				DeserializeViewDropdownValue(item2);
				break;
			case "TooltipStepValue":
				DeserializeViewTooltipStepValue(item2);
				break;
			default:
				LogError("Unhandled ApocalypseModifierStep View element name " + item2.Name.LocalName + "!");
				break;
			}
		}
	}

	public string DebugToString()
	{
		return $"Id: {Id}\nApocalypseLevel: {ApocalypseLevel}\nTokenVariables nb: {base.TokenVariables?.Count}\nLocArguments nb: {base.LocArguments?.Count}\nEffects nb: {Effects?.Count}";
	}

	public string GetLocalizedDescription(ApocalypseModifierDefinition apocalypseModifierDefinition = null)
	{
		if (apocalypseModifierDefinition == null && ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.ContainsKey(this))
		{
			apocalypseModifierDefinition = ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions[this];
			if (apocalypseModifierDefinition == null)
			{
				LogError("Couldn't find parent ApocalypseModifierDefinition for step with id: " + Id + " !");
				return string.Empty;
			}
		}
		if (base.LocArguments != null)
		{
			return Localizer.Format("ApocalypseModifierDescription_" + apocalypseModifierDefinition.Id, GetArguments(null));
		}
		return Localizer.Get("ApocalypseModifierDescription_" + apocalypseModifierDefinition.Id);
	}

	public (string, int) GetModifierIdAndStepIndex()
	{
		if (ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.ContainsKey(this))
		{
			ApocalypseModifierDefinition apocalypseModifierDefinition = ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions[this];
			return (apocalypseModifierDefinition.Id, apocalypseModifierDefinition.GetModifierStepIndexFromDefinition(this));
		}
		return (null, -1);
	}

	public string ToCodeSharingFormat()
	{
		if (!string.IsNullOrEmpty(cachedCodeSharing))
		{
			return cachedCodeSharing;
		}
		if (!ApocalypseDatabase.ModifierDefinitionsFromStepDefinitions.TryGetValue(this, out var value) || string.IsNullOrEmpty(value.CodeSharingId))
		{
			return string.Empty;
		}
		cachedCodeSharing = $"{value.CodeSharingId}{value.GetModifierStepIndexFromDefinition(this)}";
		return cachedCodeSharing;
	}

	private void DeserializeViewDropdownValue(XElement xDropdownValueElement)
	{
		XAttribute xAttribute = xDropdownValueElement.Attribute("Prefix");
		XAttribute xAttribute2 = xDropdownValueElement.Attribute("Value");
		XAttribute xAttribute3 = xDropdownValueElement.Attribute("Suffix");
		if (xAttribute2 == null)
		{
			LogError("Attribute Value in element DropdownValue in ApocalypseModifierStep " + Id + " should not be null !");
		}
		stepViewDropdownValue = new StepViewDropdownValue(xAttribute?.Value, Parser.Parse(xAttribute2?.Value, base.TokenVariables), xAttribute3?.Value);
	}

	private void DeserializeViewTooltipStepValue(XElement xTooltipStepValueElement)
	{
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		if (xTooltipStepValueElement.Element("UseDropdownValue") != null)
		{
			flag = true;
			if (stepViewDropdownValue == null)
			{
				LogError("UseDropdownValue is set but there's not dropdown value defined ! (in ApocalypseModifierStep " + Id + ")");
			}
		}
		XElement xElement = xTooltipStepValueElement.Element("UseLocArgument");
		if (xElement != null)
		{
			flag2 = true;
			XAttribute xAttribute = xElement.Attribute("Index");
			if (xAttribute == null)
			{
				LogError("UseLocArgument as a missing attribute 'Index' in ApocalypseModifierStep " + Id + ")");
			}
			else
			{
				if (!int.TryParse(xAttribute.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
				{
					LogError("ApocalypseModifierStep " + Id + "'s UseLocArgument Index " + HasAnInvalidInt(xAttribute.Value));
					return;
				}
				num = result;
				if (base.LocArguments == null || num >= base.LocArguments.Count)
				{
					LogError($"UseLocArgument has a wrong Index '{num}' in ApocalypseModifierStep {Id}, there is no matching LocArgument !");
					return;
				}
			}
		}
		if ((flag && flag2) || (!flag && !flag2))
		{
			LogError("TooltipStepValue has either defined both UseDropdownValue and UseDropdownValue or neither, this shouldn't happen, one element must be set ! (in ApocalypseModifierStep " + Id + ")");
		}
		else
		{
			stepViewTooltipStepValue = new StepViewTooltipStepValue(flag, flag2, num);
		}
	}

	private string GetTooltipStepValue()
	{
		if (stepViewTooltipStepValue == null)
		{
			return string.Empty;
		}
		if (stepViewTooltipStepValue.UseDropdownValue)
		{
			return ViewDropdownValue;
		}
		if (stepViewTooltipStepValue.UseLocArgument && base.LocArguments != null && stepViewTooltipStepValue.LocArgumentIndex < base.LocArguments.Count)
		{
			return base.LocArguments[stepViewTooltipStepValue.LocArgumentIndex].GetFinalValue(null);
		}
		return string.Empty;
	}

	private void LogError(string message)
	{
		CLoggerManager.Log(message, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "ApocalypseModifierStepDefinition");
	}
}
