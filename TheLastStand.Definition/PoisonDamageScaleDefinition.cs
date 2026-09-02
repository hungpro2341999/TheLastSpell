using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Skill;
using UnityEngine;

namespace TheLastStand.Definition;

public class PoisonDamageScaleDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, float> MultipliersPerLevel { get; private set; }

	public PoisonDamageScaleDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container.Element("PoisonDamageScaleDefinition");
		MultipliersPerLevel = new Dictionary<int, float>();
		foreach (XElement item in xElement.Elements("Multiplier"))
		{
			XAttribute xAttribute = item.Attribute("Level");
			if (!int.TryParse(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("Invalid int value " + xAttribute.Value + " for a PoisonDamageMultiplier element.", LogType.Error);
				break;
			}
			if (!float.TryParse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				CLoggerManager.Log("Invalid float value " + item.Value + " for a PoisonDamageMultiplier element.", LogType.Error);
				break;
			}
			MultipliersPerLevel.Add(result, result2);
		}
	}

	public float GetMultiplierAtLevel(int level)
	{
		if (MultipliersPerLevel.TryGetValue(level, out var value))
		{
			return value;
		}
		TPSingleton<SkillManager>.Instance.LogWarning($"PoisonDamageScaleDefinition does not define {level} multiplier. Getting the largest multiplier below level {level} instead.");
		for (int num = level - 1; num > -1; num--)
		{
			if (MultipliersPerLevel.TryGetValue(num, out value))
			{
				return value;
			}
		}
		TPSingleton<SkillManager>.Instance.LogWarning("PoisonDamageScaleDefinition seem to define no multiplier, returning 1.");
		return 1f;
	}
}
