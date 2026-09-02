using System;
using System.Collections;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.Manager.Unit;

public class EffectTimeEventManager : Manager<EffectTimeEventManager>
{
	private Dictionary<E_EffectTime, Action<PerkDataContainer>> events;

	public Dictionary<E_EffectTime, Action<PerkDataContainer>> Events
	{
		get
		{
			InitEvents();
			return events;
		}
	}

	public static IEnumerator InvokeEventOnAllPlayable(E_EffectTime effectTime, PerkDataContainer perkDataContainer = null)
	{
		List<PlayableUnit> list = new List<PlayableUnit>(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits);
		foreach (PlayableUnit playableUnit in list)
		{
			if (TPSingleton<BossManager>.Instance.ShouldTriggerBossWaveVictory)
			{
				yield break;
			}
			if (!playableUnit.Events.TryGetValue(effectTime, out var value) || value == null || playableUnit.IsDead)
			{
				continue;
			}
			value(perkDataContainer);
			if (playableUnit.IsExecutingSkill)
			{
				ACameraView.MoveTo(playableUnit.DamageableView.GameObject.transform.position, CameraView.AnimationMoveSpeed);
				yield return new WaitUntil(() => !playableUnit.IsExecutingSkill);
			}
			if (TPSingleton<BossManager>.Instance.IsPlayingDeathCutscene)
			{
				yield return TPSingleton<BossManager>.Instance.DeathCutscene.WaitUntilIsOver;
			}
			if (TPSingleton<PlayableUnitManager>.Instance.ShouldWaitUntilDeathSequences)
			{
				yield return TPSingleton<PlayableUnitManager>.Instance.WaitUntilDeathSequences;
			}
		}
	}

	public void InvokeEvent(E_EffectTime effectTime, PerkDataContainer data = null)
	{
		if (Events.TryGetValue(effectTime, out var value))
		{
			value?.Invoke(data);
		}
		else
		{
			LogError($"Tried to invoke an unregistered event: {effectTime}.");
		}
	}

	protected override void Awake()
	{
		base.Awake();
		InitEvents();
	}

	private void InitEvents()
	{
		if (events != null)
		{
			return;
		}
		events = new Dictionary<E_EffectTime, Action<PerkDataContainer>>();
		foreach (E_EffectTime value in Enum.GetValues(typeof(E_EffectTime)))
		{
			if (value != E_EffectTime.OnDeath && value != E_EffectTime.OnTileCrossed && value != E_EffectTime.OnMovementEnd && value != E_EffectTime.OnHitTaken && value != E_EffectTime.OnDodge && value != E_EffectTime.OnTargetHit && value != E_EffectTime.OnTargetDodge && value != E_EffectTime.OnTargetKilled && value != E_EffectTime.OnAttackDataComputed && value != E_EffectTime.OnSkillNextHit && value != E_EffectTime.OnSkillCastBegin && value != E_EffectTime.OnSkillCastEnd && value != E_EffectTime.OnEnemyMovementEnd && value != E_EffectTime.OnSkillStatusApplied && value != E_EffectTime.OnStatusApplied && value != E_EffectTime.OnSkillUndo && value != E_EffectTime.OnDealDamageTargetHit && value != E_EffectTime.OnDealDamageTargetKill && value != E_EffectTime.OnDealDamageExecutionEnd && value != E_EffectTime.OnPerkApplyStatusEnd && value != E_EffectTime.OnRemoveStatus && value != E_EffectTime.OnUnitSpawned && value != E_EffectTime.OnTargetHitBeforeSkillEffectsApplied)
			{
				events.Add(value, null);
			}
		}
	}
}
