using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class BattleModuleDefinition : BuildingModuleDefinition
{
	public BehaviorDefinition Behavior { get; private set; }

	public List<string> GoalsSkillsToDisplayGroupIds { get; } = new List<string>();

	public bool HasDisabledState { get; private set; }

	public int MaximumTrapCharges { get; private set; }

	public Dictionary<string, int> Skills { get; } = new Dictionary<string, int>();

	public List<SkillProgression> SkillProgressions { get; protected set; } = new List<SkillProgression>();

	public BattleModuleDefinition(BuildingDefinition buildingDefinition, XContainer battleDefinition)
		: base(buildingDefinition, battleDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("Skills");
		if (xElement2 != null)
		{
			foreach (XElement item in xElement2.Elements())
			{
				int result = -1;
				XAttribute xAttribute = item.Attribute("NightUsesCount");
				if (xAttribute != null)
				{
					if (!int.TryParse(xAttribute.Value, out result))
					{
						TPDebug.LogError("The skill " + item.Value + " " + HasAnInvalidInt(xAttribute.Value));
						continue;
					}
					if (result == -1)
					{
						CLoggerManager.Log("Night uses count has a value of -1 for skill " + item.Value + "!", LogType.Warning, CLogLevel.MAJOR);
					}
				}
				Skills.Add(item.Value, result);
			}
		}
		XElement xElement3 = xElement.Element("Behavior");
		if (xElement3 == null)
		{
			Behavior = null;
		}
		else
		{
			Behavior = new BehaviorDefinition(xElement3);
		}
		string text = xElement.Element("MaximumTrapUsage")?.Value;
		if (text != null)
		{
			if (int.TryParse(text, out var result2))
			{
				MaximumTrapCharges = result2;
			}
			else
			{
				CLoggerManager.Log("A Trap named : " + BuildingDefinition.Id + " has an invalid value for Element : MaximumTrapUsage. The value should be of type integer ! (Current value : " + text + ")", LogType.Error, CLogLevel.MAJOR);
			}
		}
		IEnumerable<XElement> enumerable = xElement.Element("SkillProgressions")?.Elements("SkillProgression");
		if (enumerable != null)
		{
			foreach (XElement item2 in enumerable)
			{
				SkillProgressions.Add(SkillProgression.Deserialize(item2));
			}
		}
		XElement xElement4 = xElement.Element("GoalsSkillsToDisplay");
		if (xElement4 != null)
		{
			foreach (XElement item3 in xElement4.Elements("SkillGroupId"))
			{
				GoalsSkillsToDisplayGroupIds.Add(item3.Value);
			}
		}
		XElement xElement5 = xElement.Element("HasDisabledState");
		if (xElement5 != null)
		{
			if (!bool.TryParse(xElement5.Value, out var result3))
			{
				CLoggerManager.Log("A Building named : " + BuildingDefinition.Id + " has an invalid value for Element : HasDisabledState. The value should be of type boolean ! (Current value : " + xElement5.Value + ")", LogType.Error, CLogLevel.MAJOR);
			}
			HasDisabledState = result3;
		}
	}
}
