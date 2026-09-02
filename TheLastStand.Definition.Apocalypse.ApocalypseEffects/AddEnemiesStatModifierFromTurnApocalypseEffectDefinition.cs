using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class AddEnemiesStatModifierFromTurnApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public List<Tuple<UnitStatDefinition.E_Stat, int>> StatModifiers { get; } = new List<Tuple<UnitStatDefinition.E_Stat, int>>();

	public int Turn { get; private set; }

	public AddEnemiesStatModifierFromTurnApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		string text = xElement.Attribute("Turn").Value.Replace(base.TokenVariables);
		if (!int.TryParse(text, out var result))
		{
			CLoggerManager.Log("An Apocalypse AddEnemiesStatModifierFromTurn Effect's " + HasAnInvalidInt(text), LogType.Error);
			return;
		}
		Turn = result;
		foreach (XElement item in xElement.Elements("StatModifier"))
		{
			XAttribute xAttribute = item.Attribute("Stat");
			if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result2))
			{
				CLoggerManager.Log("An Apocalypse AddEnemiesStatModifierFromTurn Effect's " + HasAnInvalidStat(xAttribute.Value) + "!", LogType.Error);
				break;
			}
			string text2 = item.Attribute("Value").Value.Replace(base.TokenVariables);
			if (!int.TryParse(text2, out var result3))
			{
				CLoggerManager.Log("An Apocalypse AddEnemiesStatModifierFromTurn Effect's " + HasAnInvalidInt(text2), LogType.Error);
				break;
			}
			StatModifiers.Add(new Tuple<UnitStatDefinition.E_Stat, int>(result2, result3));
		}
	}
}
