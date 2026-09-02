using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class EquipmentSlotModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "EquipmentSlotModifier";
	}

	public ItemSlotDefinition.E_ItemSlotId SlotId { get; set; }

	public Node ValueExpression { get; set; }

	public EquipmentSlotModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("SlotId");
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("An EquipmentSlotModifierEffectDefinition has an invalid Id " + xAttribute.Value + "!", LogType.Error);
		}
		SlotId = result;
		XAttribute xAttribute2 = obj.Attribute("Value");
		if (xAttribute2 != null)
		{
			ValueExpression = Parser.Parse(xAttribute2.Value, base.TokenVariables);
		}
	}
}
