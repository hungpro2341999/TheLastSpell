using System.Collections.Generic;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.View.Skill.UI;
using UnityEngine;

namespace TheLastStand.Manager;

public class EffectManager : TPSingleton<EffectManager>
{
	[SerializeField]
	private Transform effectDisplaysParent;

	[SerializeField]
	private InvalidSkillDisplay invalidSkillDisplay;

	private HashSet<IEffectTargetSkillActionController> effectTargets = new HashSet<IEffectTargetSkillActionController>();

	public static Transform EffectDisplaysParent => TPSingleton<EffectManager>.Instance.effectDisplaysParent;

	public static InvalidSkillDisplay InvalidSkillDisplay => TPSingleton<EffectManager>.Instance.invalidSkillDisplay;

	public static bool IsDisplayingEffects
	{
		get
		{
			if (TPSingleton<EffectManager>.Instance.effectTargets != null)
			{
				return TPSingleton<EffectManager>.Instance.effectTargets.Count > 0;
			}
			return false;
		}
	}

	public static void DisplayEffects(float delay = 0f)
	{
		foreach (IEffectTargetSkillActionController effectTarget in TPSingleton<EffectManager>.Instance.effectTargets)
		{
			if (effectTarget.GetEffectsCount() > 0)
			{
				effectTarget.DisplayEffects(delay);
			}
		}
	}

	public static void Register(IEffectTargetSkillActionController effectTarget)
	{
		if (effectTarget.GetEffectsCount() > 0)
		{
			TPSingleton<EffectManager>.Instance.effectTargets.Add(effectTarget);
		}
	}

	public static void Register(List<IEffectTargetSkillActionController> effectTargets)
	{
		foreach (IEffectTargetSkillActionController effectTarget in effectTargets)
		{
			Register(effectTarget);
		}
	}

	public static void Unregister(IEffectTargetSkillActionController effectTarget)
	{
		if (effectTarget.GetEffectsCount() == 0)
		{
			TPSingleton<EffectManager>.Instance.effectTargets.Remove(effectTarget);
		}
	}
}
