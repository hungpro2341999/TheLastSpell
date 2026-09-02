using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class IncreasePricesApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public ResourceManager.E_ResourceType ResourceCostType { get; private set; }

	public ResourceManager.E_PriceModifierType Type { get; private set; }

	public int Value { get; private set; }

	public IncreasePricesApocalypseEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
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
		ResourceCostType = ResourceManager.E_ResourceType.GoldAndMaterials;
		XAttribute xAttribute = obj.Attribute("ResourceCostType");
		if (xAttribute != null)
		{
			string text = xAttribute.Value.Replace(base.TokenVariables);
			if (!Enum.TryParse<ResourceManager.E_ResourceType>(text, out var result))
			{
				CLoggerManager.Log("An Apocalypse's IncreasePrices Effect " + HasAnInvalid("E_ResourceType", text), LogType.Error, CLogLevel.MAJOR);
			}
			ResourceCostType = result;
		}
		string text2 = obj.Attribute("Type").Value.Replace(base.TokenVariables);
		if (!Enum.TryParse<ResourceManager.E_PriceModifierType>(text2, out var result2))
		{
			CLoggerManager.Log("An Apocalypse's IncreasePrices Effect " + HasAnInvalid("E_PriceModifierType", text2), LogType.Error, CLogLevel.MAJOR);
		}
		Type = result2;
		XAttribute xAttribute2 = obj.Attribute("Value");
		Value = Parser.Parse(xAttribute2.Value, base.TokenVariables).EvalToInt();
	}
}
