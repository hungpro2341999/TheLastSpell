using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Meta;

public class UnitAttributeModifierMetaEffectDefinition : MetaEffectDefinition
{
	public const string Name = "PlayableUnitAttributeModifier";

	public const string AllArchetypesId = "All";

	public string Archetype { get; private set; }

	public bool AllArchetypes => Archetype == "All";

	public Dictionary<UnitStatDefinition.E_Stat, Vector2> StatAndValue { get; } = new Dictionary<UnitStatDefinition.E_Stat, Vector2>();

	public UnitAttributeModifierMetaEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement obj = container as XElement;
		XElement xElement = obj.Element("Archetype");
		if (xElement != null)
		{
			Archetype = xElement.Value;
		}
		foreach (XElement item in obj.Element("StatsBonus").Elements("StatBonus"))
		{
			XElement xElement2 = item.Element("Stat");
			XElement xElement3 = item.Element("Bonus");
			if (xElement2 == null || xElement3 == null)
			{
				Debug.LogError("xStat or xBonus doesn't exist !");
				continue;
			}
			XAttribute xAttribute = xElement2.Attribute("Id");
			XAttribute xAttribute2 = xElement3.Attribute("Min");
			XAttribute xAttribute3 = xElement3.Attribute("Max");
			int result2;
			int result3;
			if (xAttribute == null || !Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
			{
				Debug.LogError("xStatId element as an invalid value! Or xStatId doesn't exist !");
			}
			else if (xAttribute2 == null || !int.TryParse(xAttribute2.Value, out result2))
			{
				Debug.LogError("Min attribute as an invalid value! Or Min doesn't exist !");
			}
			else if (xAttribute3 == null || !int.TryParse(xAttribute3.Value, out result3))
			{
				Debug.LogError("Max attribute as an invalid value! Or Max doesn't exist !");
			}
			else if (StatAndValue.ContainsKey(result))
			{
				StatAndValue[result] += new Vector2(result2, result3);
			}
			else
			{
				StatAndValue.Add(result, new Vector2(result2, result3));
			}
		}
	}

	public override string ToString()
	{
		string text = "PlayableUnitAttributeModifier Archetype : <b>" + Archetype + "</b>\r\n Stats and Value :\r\n";
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, Vector2> item in StatAndValue)
		{
			text += $"\tStat : <b>{item.Key}</b> Value : <b>Min: {item.Value.x} ; Max: {item.Value.y}</b>\r\n";
		}
		return text;
	}
}
