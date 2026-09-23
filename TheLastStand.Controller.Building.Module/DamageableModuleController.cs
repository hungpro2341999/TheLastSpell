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
	#region Fields & Properties
	/// <summary>
	/// Cho biết công trình có thể chuẩn bị tử trận (phát hiệu ứng / Death Rattle) hay không.
	/// </summary>
	public bool CanPrepareForDeath { get; set; } = true;

	/// <summary>
	/// Model nhận sát thương (DamageableModule) của công trình.
	/// </summary>
	public DamageableModule DamageableModule { get; }

	/// <summary>
	/// Thực thể nhận sát thương (IDamageable).
	/// </summary>
	public IDamageable Damageable => DamageableModule;
	#endregion

	#region Initialization & Factory
	/// <summary>
	/// Khởi tạo Controller nhận sát thương cho công trình.
	/// </summary>
	public DamageableModuleController(BuildingController buildingControllerParent, DamageableModuleDefinition damageableModuleDefinition)
		: base(buildingControllerParent, damageableModuleDefinition)
	{
		DamageableModule = base.BuildingModule as DamageableModule;
	}

	/// <summary>
	/// Khởi tạo Model DamageableModule tương ứng.
	/// </summary>
	protected override BuildingModule CreateModel(TheLastStand.Model.Building.Building building, BuildingModuleDefinition buildingModuleDefinition)
	{
		return new DamageableModule(building, buildingModuleDefinition as DamageableModuleDefinition, this);
	}
	#endregion

	#region Health & Armor Management
	/// <summary>
	/// Cộng giáp cho công trình (Công trình thường không dùng giáp nên hàm này trả về 0).
	/// </summary>
	public virtual float GainArmor(float amount, bool refreshHud = true)
	{
		TPSingleton<BuildingManager>.Instance.LogWarning("Tried to add armor on a building : " + DamageableModule.BuildingParent.Id);
		return 0f;
	}

	/// <summary>
	/// Hồi máu cho công trình. Trả về lượng máu thực tế được hồi.
	/// </summary>
	public virtual float GainHealth(float amount, bool refreshHud = true)
	{
		float health = DamageableModule.Health;
		SetHealth(DamageableModule.Health + amount, refreshHud);
		return DamageableModule.Health - health;
	}

	/// <summary>
	/// Giảm giáp của công trình (để trống cho công trình).
	/// </summary>
	public virtual void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
	}

	/// <summary>
	/// Trừ máu của công trình: Cập nhật Panic value nếu công trình bị đánh trong đêm,
	/// kiểm tra lượng máu MagicCircle/MageCount, và chuyển sang trạng thái Dead khi máu <= 0.
	/// </summary>
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

	/// <summary>
	/// Sửa chữa công trình đầy máu và phát âm thanh sửa chữa.
	/// </summary>
	public virtual float Repair()
	{
		SoundManager.PlayAudioClip(BuildingManager.RepairAudioClip, BuildingManager.BuildingPooledAudioSourceData);
		return GainHealth(DamageableModule.HealthTotal - DamageableModule.Health, refreshHud: false);
	}

	/// <summary>
	/// Cập nhật máu hiện tại của công trình và làm mới HUD hiển thị.
	/// </summary>
	public virtual void SetHealth(float health, bool refreshHud = true)
	{
		DamageableModule.Health = Mathf.Clamp(health, 0f, DamageableModule.HealthTotal);
		if (refreshHud)
		{
			base.BuildingControllerParent.BuildingView?.BuildingHUD.RefreshHealth();
			base.BuildingControllerParent.BuildingView?.RefreshBuildingDamagedAppearance();
		}
	}

	/// <summary>
	/// Cập nhật tổng số máu tối đa của công trình và điều chỉnh lượng máu hiện tại tương ứng.
	/// </summary>
	public virtual void UpdateHealth(float newHealthTotal, bool refreshHud = true)
	{
		float num = DamageableModule.Health / DamageableModule.HealthTotal;
		DamageableModule.HealthTotal = newHealthTotal;
		SetHealth(num * DamageableModule.HealthTotal, refreshHud);
		float healthGain = num * DamageableModule.HealthTotal - DamageableModule.Health;
		DamageableModule.DamageableView.DamageableHUD.PlayHealthGainAnim(healthGain, num * DamageableModule.HealthTotal);
		base.BuildingControllerParent.BuildingView?.BuildingHUD.RefreshHealth();
	}

	/// <summary>
	/// Callback khi công trình trúng đòn.
	/// </summary>
	public virtual void OnHit(ISkillCaster attacker)
	{
	}

	/// <summary>
	/// Callback khi dữ liệu tấn công được tính toán xong.
	/// </summary>
	public void OnAttackDataComputed(PerkDataContainer perkDataContainer)
	{
	}
	#endregion

	#region Effect Display
	/// <summary>
	/// Thêm hiển thị hiệu ứng thông qua BlueprintModuleController.
	/// </summary>
	public void AddEffectDisplay(IDisplayableEffect displayableEffect)
	{
		base.BuildingControllerParent.BlueprintModuleController.AddEffectDisplay(displayableEffect);
	}

	/// <summary>
	/// Hiển thị các hiệu ứng kỹ năng thông qua BlueprintModuleController.
	/// </summary>
	public void DisplayEffects(float delay = 0f)
	{
		base.BuildingControllerParent.BlueprintModuleController.DisplayEffects(delay);
	}

	/// <summary>
	/// Trả về số lượng hiệu ứng từ BlueprintModuleController.
	/// </summary>
	public int GetEffectsCount()
	{
		return base.BuildingControllerParent.BlueprintModuleController.GetEffectsCount();
	}
	#endregion

	#region Death & Demolish Logic
	/// <summary>
	/// Cho phép hoặc ngắt quyền chuẩn bị tử trận của công trình.
	/// </summary>
	public void ChangeCanPrepareForDeath(bool canPrepareForDeath)
	{
		if (canPrepareForDeath || !DamageableModule.IsDead)
		{
			CanPrepareForDeath = canPrepareForDeath;
		}
	}

	/// <summary>
	/// Phá hủy/tháo dỡ công trình (gây sát thương bằng tổng lượng máu và chạy animation biến mất).
	/// </summary>
	public void Demolish()
	{
		LoseHealth(DamageableModule.Health);
		base.BuildingControllerParent.BuildingView.PlayDieAnim();
	}

	/// <summary>
	/// Xử lý tiêu hủy công trình trên bản đồ và giải phóng các ô tile.
	/// </summary>
	protected virtual void OnDeath()
	{
		BuildingManager.DestroyBuilding(base.BuildingControllerParent.Building.OriginTile);
		(Damageable.DamageableView as BuildingView)?.HandledDefensesHUD?.DisplayHandledDefensesUses(state: false);
		TPSingleton<BuildingManager>.Instance.RestoreBuildingIfNeeded(base.BuildingControllerParent.Building.OriginTile);
	}

	/// <summary>
	/// Coroutine chuẩn bị các chuỗi hiệu ứng trăn trối (Death Rattle) trước khi tử trận hoàn toàn.
	/// </summary>
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

	/// <summary>
	/// Hoàn tất việc tử trận của công trình nếu trạng thái là Dead.
	/// </summary>
	private void FinalizeDeath()
	{
		if (DamageableModule.IsDead)
		{
			TPSingleton<BuildingManager>.Instance.Log("Finalizing Death of a Building " + base.BuildingControllerParent.Building.BuildingDefinition.Id + " !");
			OnDeath();
		}
	}

	/// <summary>
	/// Coroutine chờ hoàn tất hoạt ảnh bị phá hủy và các hiệu ứng Death Rattle trước khi kết thúc tử trận.
	/// </summary>
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
	#endregion
}
