using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class EnemiesStatBaseValueModifierApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public List<string> AffectedEnemies { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public int Value { get; private set; }

	public EnemiesStatBaseValueModifierApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Stat");
		XAttribute xAttribute2 = xElement.Attribute("Value");
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("An Apocalypse EnemyStatModifier Effect's " + HasAnInvalidStat(xAttribute.Value) + "!", LogType.Error);
			return;
		}
		Value = Parser.Parse(xAttribute2.Value, base.TokenVariables).EvalToInt();
		AffectedEnemies = new List<string>();
		foreach (XElement item in xElement.Elements("TriggeredList"))
		{
			string key = item.Value.Replace(base.TokenVariables);
			foreach (string id in GenericDatabase.IdsListDefinitions[key].Ids)
			{
				AddAffectedEnemy(id);
			}
		}
		foreach (XElement item2 in xElement.Elements("TriggeredId"))
		{
			AddAffectedEnemy(item2.Value.Replace(base.TokenVariables));
		}
		Stat = result;
	}

	private void AddAffectedEnemy(string id)
	{
		if (!AffectedEnemies.Contains(id))
		{
			AffectedEnemies.Add(id);
		}
	}
}
