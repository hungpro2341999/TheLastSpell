using System.Collections.Generic;
using TheLastStand.Controller;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit.Perk;
using UnityEngine;

namespace TheLastStand.Model;

public interface ISkillCaster : ITileObject
{
	bool CanAffectTargetInFog { get; }

	bool CanAffectTargetInLightFog { get; }

	bool IsExecutingSkill { get; }

	ISkillCasterController SkillCasterController { get; }

	Coroutine SkillExecutionCoroutine { get; set; }

	List<string> PreventedSkillsIds { get; }

	int ComputeStatusDuration(TheLastStand.Model.Status.Status.E_StatusType statusType, int baseValue, PerkDataContainer perkDataContainer = null, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null);
}
