using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class RestoreUsesEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "RestoreUses";
	}

	public ItemSlotDefinition.E_ItemSlotId SlotType { get; private set; }

	public Node ValueExpression { get; private set; }

	public RestoreUsesEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Value");
		ValueExpression = Parser.Parse(xAttribute.Value, base.TokenVariables);
		XAttribute xAttribute2 = obj.Attribute("SlotId");
		if (Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(xAttribute2.Value, out var result))
		{
			SlotType = result;
			return;
		}
		SlotType = ItemSlotDefinition.E_ItemSlotId.None;
		CLoggerManager.Log("Could not parse SlotId attribute into a E_ItemSlotId. Value is \"" + xAttribute2.Value + "\"", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "RestoreUsesEffectDefinition");
	}
}
