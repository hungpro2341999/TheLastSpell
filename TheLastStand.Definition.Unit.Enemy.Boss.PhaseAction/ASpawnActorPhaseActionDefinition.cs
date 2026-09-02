using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public abstract class ASpawnActorPhaseActionDefinition : ABossPhaseActionDefinition
{
	protected UnitCreationSettings unitCreationSettingsTemplate;

	public List<string> ActorsIds { get; protected set; }

	public bool CameraFocus { get; private set; }

	public bool HasMultipleActorId
	{
		get
		{
			List<string> actorsIds = ActorsIds;
			if (actorsIds == null)
			{
				return false;
			}
			return actorsIds.Count > 1;
		}
	}

	protected ASpawnActorPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		ActorsIds = new List<string>();
		XAttribute xAttribute = xElement.Attribute("ActorId");
		ActorsIds = xAttribute.Value.RemoveWhitespace().Split(',').ToList();
		XAttribute xAttribute2 = xElement.Attribute("CameraFocus");
		if (xAttribute2 != null)
		{
			if (bool.TryParse(xAttribute2.Value, out var result))
			{
				CameraFocus = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute2.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		XAttribute xAttribute3 = xElement.Attribute("CastSpawnSkill");
		bool result2 = true;
		if (xAttribute3 != null && !bool.TryParse(xAttribute3.Value, out result2))
		{
			CLoggerManager.Log("Could not parse SpawnActorPhaseActionDefinition castSpawnSkill value " + xAttribute3.Value + " as a valid bool value.", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute4 = xElement.Attribute("PlaySpawnAnim");
		bool result3 = true;
		if (xAttribute4 != null && !bool.TryParse(xAttribute4.Value, out result3))
		{
			CLoggerManager.Log("Could not parse SpawnActorPhaseActionDefinition playSpawnAnim value " + xAttribute4.Value + " as a valid bool value.", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute5 = xElement.Attribute("PlaySpawnCutscene");
		bool result4 = !(this is ReplaceActorsPhaseActionDefinition);
		if (xAttribute5 != null && xAttribute5.Value != null && !bool.TryParse(xAttribute5.Value, out result4))
		{
			CLoggerManager.Log("Unable to parse " + xAttribute5.Value + " into bool", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute6 = xElement.Attribute("WaitSpawnAnim");
		bool result5 = false;
		if (xAttribute6 != null && !bool.TryParse(xAttribute6.Value, out result5))
		{
			CLoggerManager.Log("Unable to parse " + xAttribute6.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute7 = xElement.Attribute("IsGuardian");
		bool result6 = false;
		if (xAttribute7 != null && !bool.TryParse(xAttribute7.Value, out result6))
		{
			CLoggerManager.Log("Unable to parse " + xAttribute7.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
		}
		XAttribute xAttribute8 = xElement.Attribute("IgnoreFromEnemyUnitsCount");
		bool result7 = false;
		if (xAttribute8 != null && !bool.TryParse(xAttribute8.Value, out result7))
		{
			CLoggerManager.Log("Unable to parse " + xAttribute8.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
		}
		unitCreationSettingsTemplate = new UnitCreationSettings(string.Empty, result2, result3, result4, result5, -1, null, result6, result7);
	}

	public string GetRandomActorId()
	{
		int index = 0;
		if (HasMultipleActorId)
		{
			index = RandomManager.GetRandomRange(this, 0, ActorsIds.Count);
		}
		return ActorsIds[index];
	}

	public virtual UnitCreationSettings GetUnitCreationSettings()
	{
		return new UnitCreationSettings(GetRandomActorId(), unitCreationSettingsTemplate.CastSpawnSkill, unitCreationSettingsTemplate.PlaySpawnAnim, unitCreationSettingsTemplate.PlaySpawnCutscene, unitCreationSettingsTemplate.WaitSpawnAnim, -1, null, unitCreationSettingsTemplate.IsGuardian, unitCreationSettingsTemplate.IgnoreFromEnemyUnitsCount);
	}
}
