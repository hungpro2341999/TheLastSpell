using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss.PhaseAction;

public class ReplaceActorsPhaseActionDefinition : ASpawnActorPhaseActionDefinition
{
	private List<string> replacementIds;

	public int Amount { get; private set; }

	public bool HasMultipleReplacementId
	{
		get
		{
			List<string> list = replacementIds;
			if (list == null)
			{
				return false;
			}
			return list.Count > 1;
		}
	}

	public bool IncludeNonActor { get; private set; }

	public bool WaitDeathAnim { get; private set; }

	public ReplaceActorsPhaseActionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement xElement = container as XElement;
		replacementIds = new List<string>();
		XAttribute xAttribute = xElement.Attribute("ReplacementId");
		replacementIds = xAttribute.Value.RemoveWhitespace().Split(',').ToList();
		XAttribute xAttribute2 = xElement.Attribute("Amount");
		if (xAttribute2 != null)
		{
			Amount = int.Parse(xAttribute2.Value);
		}
		XAttribute xAttribute3 = xElement.Attribute("IncludeNonActor");
		if (xAttribute3 != null)
		{
			if (bool.TryParse(xAttribute3.Value, out var result))
			{
				IncludeNonActor = result;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute3.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		XAttribute xAttribute4 = xElement.Attribute("WaitDeathAnim");
		if (xAttribute4 != null)
		{
			if (bool.TryParse(xAttribute4.Value, out var result2))
			{
				WaitDeathAnim = result2;
			}
			else
			{
				CLoggerManager.Log("Unable to parse " + xAttribute4.Value + " into bool.", LogType.Error, CLogLevel.MAJOR);
			}
		}
		unitCreationSettingsTemplate = new UnitCreationSettings(string.Empty, unitCreationSettingsTemplate.CastSpawnSkill, unitCreationSettingsTemplate.PlaySpawnAnim, unitCreationSettingsTemplate.PlaySpawnCutscene, unitCreationSettingsTemplate.WaitSpawnAnim, -1, null, unitCreationSettingsTemplate.IsGuardian);
	}

	public override UnitCreationSettings GetUnitCreationSettings()
	{
		int index = 0;
		if (HasMultipleReplacementId)
		{
			index = RandomManager.GetRandomRange(this, 0, replacementIds.Count);
		}
		return new UnitCreationSettings(replacementIds[index], unitCreationSettingsTemplate.CastSpawnSkill, unitCreationSettingsTemplate.PlaySpawnAnim, unitCreationSettingsTemplate.PlaySpawnCutscene, unitCreationSettingsTemplate.WaitSpawnAnim, -1, null, unitCreationSettingsTemplate.IsGuardian);
	}
}
