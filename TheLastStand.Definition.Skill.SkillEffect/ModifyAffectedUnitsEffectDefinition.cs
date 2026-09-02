using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillEffect;

public class ModifyAffectedUnitsEffectDefinition : SkillEffectDefinition
{
	public static class Constants
	{
		public const string Id = "ModifyAffectedUnits";
	}

	public AffectingUnitSkillEffectDefinition.E_SkillUnitAffect AffectedUnitsToAdd { get; private set; }

	public AffectingUnitSkillEffectDefinition.E_SkillUnitAffect AffectedUnitsToRemove { get; private set; }

	public List<string> IdsListsToExclude { get; } = new List<string>();

	public List<string> IdsListsToInclude { get; } = new List<string>();

	public override bool DisplayCompendiumEntry => false;

	public override string Id => "ModifyAffectedUnits";

	public override bool ShouldBeDisplayed => false;

	public ModifyAffectedUnitsEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xEffectElement = container as XElement;
		ParseAddRemoveAffectedUnits(xEffectElement, "AddAffectedUnits", isAddAffectedUnits: true);
		ParseAddRemoveAffectedUnits(xEffectElement, "RemoveAffectedUnits", isAddAffectedUnits: false);
		ParseIncludeExcludeAffectedUnitsIds(xEffectElement, "IncludeAffectedUnits", isIncludeAffectedUnits: true);
		ParseIncludeExcludeAffectedUnitsIds(xEffectElement, "ExcludeAffectedUnits", isIncludeAffectedUnits: false);
	}

	private void LogError(string message)
	{
		CLoggerManager.Log(message, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SkillManager");
	}

	private void ParseAddRemoveAffectedUnits(XElement xEffectElement, string elementsName, bool isAddAffectedUnits)
	{
		foreach (XElement item in xEffectElement.Elements(elementsName))
		{
			XAttribute xAttribute = item.Attribute("Type");
			AffectingUnitSkillEffectDefinition.E_SkillUnitAffect result;
			if (xAttribute == null)
			{
				LogError("Type wasn't defined in effect 'ModifyAffectedUnits' effect in '" + elementsName + "' element !");
			}
			else if (!Enum.TryParse<AffectingUnitSkillEffectDefinition.E_SkillUnitAffect>(xAttribute.Value, out result))
			{
				LogError("Type has an incorrect value: " + xAttribute.Value);
			}
			else if (isAddAffectedUnits)
			{
				AffectedUnitsToAdd |= result;
			}
			else
			{
				AffectedUnitsToRemove |= result;
			}
		}
	}

	private void ParseIncludeExcludeAffectedUnitsIds(XElement xEffectElement, string elementsName, bool isIncludeAffectedUnits)
	{
		foreach (XElement item in xEffectElement.Elements(elementsName))
		{
			XAttribute xAttribute = item.Attribute("IdsList");
			if (xAttribute == null)
			{
				LogError("IdsList wasn't defined in effect 'ModifyAffectedUnits' effect in '" + elementsName + "' element !");
				continue;
			}
			string text = xAttribute.Value.Replace(base.TokenVariables);
			if (!GenericDatabase.IdsListDefinitions.ContainsKey(text))
			{
				LogError("Couldn't find IdsList with id: " + text);
			}
			else if (isIncludeAffectedUnits)
			{
				IdsListsToInclude.Add(text);
			}
			else
			{
				IdsListsToExclude.Add(text);
			}
		}
	}
}
