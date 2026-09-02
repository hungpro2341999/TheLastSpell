using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class ModifyEnemiesInjuryStageApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public class ApocalypseInjuryStatModifier
	{
		public int Value;

		public bool OverrideValue;

		public UnitStatDefinition.E_Stat StatType;

		public ApocalypseInjuryStatModifier(UnitStatDefinition.E_Stat statType, int value, bool overrideValue)
		{
			StatType = statType;
			Value = value;
			OverrideValue = overrideValue;
		}
	}

	public int InjuryStage { get; private set; }

	public List<ApocalypseInjuryStatModifier> InjuryStatModifiers { get; } = new List<ApocalypseInjuryStatModifier>();

	public ModifyEnemiesInjuryStageApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("InjuryStage");
		if (!int.TryParse(xAttribute.Value.Replace(base.TokenVariables), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log("The ModifyEnemyInjuryStageEffect has an invalid InjuryStage " + xAttribute.Value.Replace(base.TokenVariables) + "!", LogType.Error);
			result = 1;
		}
		InjuryStage = result;
		foreach (XElement item in obj.Element("StatModifiers").Elements("StatModifier"))
		{
			XAttribute xAttribute2 = item.Attribute("Stat");
			if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute2.Value, out var result2))
			{
				CLoggerManager.Log("An Apocalypse ModifyEnemyInjuryStage Effect's " + HasAnInvalidStat(xAttribute2.Value) + "!", LogType.Error);
				break;
			}
			string text = item.Attribute("Value").Value.Replace(base.TokenVariables);
			if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result3))
			{
				CLoggerManager.Log("An Apocalypse ModifyEnemyInjuryStage Effect's " + HasAnInvalidInt(text), LogType.Error);
				break;
			}
			XAttribute xAttribute3 = item.Attribute("OverrideValue");
			bool result4 = false;
			if (xAttribute3 != null)
			{
				string text2 = xAttribute3.Value.Replace(base.TokenVariables);
				if (!bool.TryParse(text2, out result4))
				{
					CLoggerManager.Log("Could not parse ModifyEnemyInjuryStage effect OverrideValue value " + text2 + " to a valid bool.", LogType.Error);
				}
			}
			InjuryStatModifiers.Add(new ApocalypseInjuryStatModifier(result2, result3, result4));
		}
	}
}
