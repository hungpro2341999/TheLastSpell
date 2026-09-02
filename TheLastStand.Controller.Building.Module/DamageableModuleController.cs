using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Definition.Building.Module;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Building;
using TheLastStand.View.Camera;
using TheLastStand.View.Skill.SkillAction;
using UnityEngine;

namespace TheLastStand.Controller.Building.Module;

public class DamageableModuleController : BuildingModuleController, IDamageableController, IEffectTargetSkillActionController
{
	public bool CanPrepareForDeath { get; set; } = true;

	public DamageableModule DamageableModule { get; }

	public IDamageable Damageable => DamageableModule;

	public DamageableModuleController(BuildingController buildingControllerParent, DamageableModuleDefinition damageableModuleDefinition)
		: base(buildingControllerParent, damageableModuleDefinition)
	{
		DamageableModule = base.BuildingModule as DamageableModule;
	}

	public void AddEffectDisplay(IDisplayableEffect displayableEffect)
	{
		base.BuildingControllerParent.BlueprintModuleController.AddEffectDisplay(displayableEffect);
	}

	public void ChangeCanPrepareForDeath(bool canPrepareForDeath)
	{
		if (canPrepareForDeath || !DamageableModule.IsDead)
		{
			CanPrepareForDeath = canPrepareForDeath;
		}
	}

	public void Demolish()
	{
		LoseHealth(DamageableModule.Health);
		base.BuildingControllerParent.BuildingView.PlayDieAnim();
	}

	public void DisplayEffects(float delay = 0f)
	{
		base.BuildingControllerParent.BlueprintModuleController.DisplayEffects(delay);
	}

	public virtual float GainArmor(float amount, bool refreshHud = true)
	{
		TPSingleton<BuildingManager>.Instance.LogWarning("Tried to add armor on a building : " + DamageableModule.BuildingParent.Id);
		return 0f;
	}

	public virtual float GainHealth(float amount, bool refreshHud = true)
	{
		float health = DamageableModule.Health;
		SetHealth(DamageableModule.Health + amount, refreshHud);
		return DamageableModule.Health - health;
	}

	public int GetEffectsCount()
	{
		return base.BuildingControllerParent.BlueprintModuleController.GetEffectsCount();
	}

	public virtual void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
	}

	public virtual void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true, string skillName = null)
	{
		if (base.BuildingControllerParent.Building.DebugIsIndesctructible || (base.BuildingControllerParent.Building.BlueprintModule.IsIndestructible && !base.BuildingControllerParent.Building.IsDemolishableIfIndestructible) || DamageableModule.State != DamageableModule.E_State.Alive)
		{
			return;
		}
		SetHealth(DamageableModule.Health - amount, refreshHud: false);
		if (base.BuildingControllerParent.Building is MagicCircle magicCircle)
		{
			while (DamageableModule.Health / magicCircle.MageLife < (float)(magicCircle.MageCount - 1))
			{
				magicCircle.MageCount--;
				TPSingleton<BuildingManager>.Instance.Log($"Magic circle lost health, MageCount = {magicCircle.MageCount}", CLogLevel.MAJOR);
				magicCircle.MagicCircleView.Dirty = true;
			}
		}
		if (DamageableModule.DamageableModuleDefinition.CanPanic && base.BuildingControllerParent.Building.IsInCity && TPSingleton<GameManager>.Instance.Game.IsNightCycle)
		{
			float num = Mathf.Min(amount, DamageableModule.HealthTotal) / DamageableModule.HealthTotal;
			float num2 = DamageableModule.DamageableModuleDefinition.TotalPanicValue * num;
			PanicManager.Panic.PanicController.AddValue(num2);
			TPSingleton<PanicManager>.Instance.Log($"{base.BuildingControllerParent.Building.Name} hit! Adding {num2} Panic ({num} of {DamageableModule.DamageableModuleDefinition.TotalPanicValue}, bringing total Panic to {PanicManager.Panic.Value}", CLogLevel.NORMAL, forcePrintInUnity: true);
		}
		if (DamageableModule.Health <= 0f)
		{
			if (base.BuildingControllerParent.Building is MagicCircle magicCircle2)
			{
				ACameraView.MoveTo(base.BuildingControllerParent.BuildingView.transform.position, CameraView.AnimationMoveSpeed);
				GameController.SetState(Game.E_State.GameOver);
				magicCircle2.MageCount = 0;
			}
			if (base.BuildingControllerParent.Building.IsDefensive)
			{
				TrophyManager.AppendValueToTrophiesConditions<DefensesLostTrophyConditionController>(new object[1] { 1 });
			}
			else
			{
				TrophyManager.AppendValueToTrophiesConditions<BuildingsLostTrophyConditionController>(new object[1] { 1 });
			}
			TPSingleton<AchievementManager>.Instance.HandleDestroyedBuilding(base.BuildingControllerParent.Building, attacker);
			DamageableModule.State = DamageableModule.E_State.Dead;
			TPSingleton<GameManager>.Instance.StartCoroutine(PrepareForDeath());
		}
	}

	public virtual void OnHit(ISkillCaster attacker)
	{
	}

	public void OnAttackDataComputed(PerkDataContainer perkDataContainer)
	{
	}

	public virtual float Repair()
	{
		SoundManager.PlayAudioClip(BuildingManager.RepairAudioClip, BuildingManager.BuildingPooledAudioSourceData);
		return GainHealth(DamageableModule.HealthTotal - DamageableModule.Health, refreshHud: false);
	}

	public virtual void SetHealth(float health, bool refreshHud = true)
	{
		DamageableModule.Health = Mathf.Clamp(health, 0f, DamageableModule.HealthTotal);
		if (refreshHud)
		{
			base.BuildingControllerParent.BuildingView?.BuildingHUD.RefreshHealth();
			base.BuildingControllerParent.BuildingView?.RefreshBuildingDamagedAppearance();
		}
	}

	public virtual void UpdateHealth(float newHealthTotal, bool refreshHud = true)
	{
		float num = DamageableModule.Health / DamageableModule.HealthTotal;
		DamageableModule.HealthTotal = newHealthTotal;
		SetHealth(num * DamageableModule.HealthTotal, refreshHud);
		float healthGain = num * DamageableModule.HealthTotal - DamageableModule.Health;
		DamageableModule.DamageableView.DamageableHUD.PlayHealthGainAnim(healthGain, num * DamageableModule.HealthTotal);
		base.BuildingControllerParent.BuildingView?.BuildingHUD.RefreshHealth();
	}

	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new DamageableModule(building, buildingModuleDefinition as DamageableModuleDefinition, this);
	}

	protected virtual void OnDeath()
	{
		BuildingManager.DestroyBuilding(base.BuildingControllerParent.Building.OriginTile);
		(Damageable.DamageableView as BuildingView)?.HandledDefensesHUD?.DisplayHandledDefensesUses(state: false);
		TPSingleton<BuildingManager>.Instance.RestoreBuildingIfNeeded(base.BuildingControllerParent.Building.OriginTile);
	}

	private IEnumerator PrepareForDeath()
	{
		yield return new WaitUntil(() => CanPrepareForDeath);
		yield return TPSingleton<EnemyUnitManager>.Instance.WaitUntilDeathRattlingEnemiesAreDone;
		if (base.BuildingModule.BuildingParent.IsBossPhaseActor)
		{
			base.BuildingModule.BuildingParent.PrepareBossActorDeath();
		}
		base.BuildingModule.BuildingParent.BattleModule?.BattleModuleController.PrepareForDeathRattle();
		yield return FinalizeDeathWhenNeeded();
	}

	private void FinalizeDeath()
	{
		if (DamageableModule.IsDead)
		{
			TPSingleton<BuildingManager>.Instance.Log("Finalizing Death of a Building " + base.BuildingControllerParent.Building.BuildingDefinition.Id + " !");
			OnDeath();
		}
	}

	private IEnumerator FinalizeDeathWhenNeeded()
	{
		bool shouldUpdatedIsDeathRattling = base.BuildingModule.BuildingParent.PassivesModule?.PassivesModuleDefinition.HasOnDeathEffect ?? false;
		if (shouldUpdatedIsDeathRattling)
		{
			DamageableModule.IsDestroyAnimating = true;
		}
		base.BuildingModule.BuildingParent.BattleModule?.BattleModuleController.ExecuteDeathRattle();
		yield return base.BuildingControllerParent.BuildingView.WaitUntilDeathCanBeFinalized;
		if (shouldUpdatedIsDeathRattling)
		{
			DamageableModule.IsDestroyAnimating = false;
		}
		FinalizeDeath();
	}
}
