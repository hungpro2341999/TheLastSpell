using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class AffixMalusDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_MalusLevel
	{
		Undefined,
		Small,
		Medium,
		Big
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MalusLevelComparer : IEqualityComparer<E_MalusLevel>
	{
		public bool Equals(E_MalusLevel x, E_MalusLevel y)
		{
			return x == y;
		}

		public int GetHashCode(E_MalusLevel obj)
		{
			return (int)obj;
		}
	}

	public static readonly MalusLevelComparer SharedMalusLevelComparer;

	public Dictionary<E_MalusLevel, float> MalusPerLevel { get; private set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public int Weight { get; private set; }

	public AffixMalusDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("StatId");
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(xAttribute.Value, out var result))
		{
			CLoggerManager.Log("Could not parse StatId Attribute value " + xAttribute.Value + " to a valid E_Stat value.");
			return;
		}
		Stat = result;
		XElement xElement2 = xElement.Element("Weight");
		if (!int.TryParse(xElement2.Value, out var result2))
		{
			CLoggerManager.Log("Could not parse AffixMalusDefinition Weight element value " + xElement2.Value + " to a valid int value.");
			return;
		}
		Weight = result2;
		MalusPerLevel = new Dictionary<E_MalusLevel, float>(SharedMalusLevelComparer);
		foreach (XElement item in xElement.Element("Values").Elements())
		{
			string localName = item.Name.LocalName;
			if (!Enum.TryParse<E_MalusLevel>(localName, out var result3))
			{
				CLoggerManager.Log($"Could not parse value Element Name of AffixMalusDefinition with Id {Stat} {localName} to a valid E_MalusLevel value.", LogType.Error);
				break;
			}
			if (!float.TryParse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
			{
				CLoggerManager.Log($"Could not parse Malus value element of AffixMalusDefinition {Stat} {item.Value} to a valid float value.", LogType.Error);
				break;
			}
			MalusPerLevel.Add(result3, result4);
		}
	}

	public bool IsMalusLevelDefined(E_MalusLevel malusLevel)
	{
		return MalusPerLevel.ContainsKey(malusLevel);
	}
}
