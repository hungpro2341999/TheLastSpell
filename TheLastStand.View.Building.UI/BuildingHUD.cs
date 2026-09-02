using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingHUD : SerializedMonoBehaviour, IDamageableHUD
{
	[SerializeField]
	private FollowElement followElement;

	[SerializeField]
	[Tooltip("Delay before hiding (if needed), after being animated")]
	private float healthHideAfterAnimDelay = 0.6f;

	[SerializeField]
	private Canvas healthCanvas;

	[SerializeField]
	private TextMeshProUGUI healthText;

	[SerializeField]
	private UnitStatGaugeDisplay healthDisplay;

	[SerializeField]
	private Canvas brazierCanvas;

	[SerializeField]
	private TextMeshProUGUI brazierText;

	[SerializeField]
	protected UnitStatGaugeDisplay brazierDisplay;

	[SerializeField]
	protected Image highlightImage;

	[SerializeField]
	private GameObject production;

	[SerializeField]
	private Image productionIcon;

	private TheLastStand.Model.Building.Building building;

	private Coroutine hideHealthDisplayCoroutine;

	private UnitStatGaugeDisplay healthDisplayStored;

	public virtual TheLastStand.Model.Building.Building Building
	{
		get
		{
			return building;
		}
		set
		{
			if (value != null)
			{
				building = value;
				base.name = building.UniqueIdentifier + " HUD";
				if (building.BlueprintModule.IsIndestructible)
				{
					HealthDisplayed = false;
					healthDisplay = null;
				}
				else
				{
					healthDisplay.Damageable = building?.DamageableModule;
					RefreshHealth();
					healthDisplay.Building = building;
				}
				if (building.BrazierModule != null)
				{
					brazierDisplay.Building = building;
				}
				if (followElement != null)
				{
					followElement.ChangeTarget(Building.BuildingView.HudFollowTarget);
					followElement.ChangeOffset(Building.BuildingDefinition.BlueprintModuleDefinition.HUDOffset);
				}
				Highlight = false;
			}
		}
	}

	public bool HealthDisplayed
	{
		get
		{
			return healthCanvas.enabled;
		}
		set
		{
			bool flag = TPSingleton<PlayableUnitManager>.Exist() && !PlayableUnitManager.DebugDisableHealthDisplay && value;
			healthCanvas.enabled = flag;
			healthText.enabled = flag;
		}
	}

	public virtual bool Highlight
	{
		get
		{
			if (highlightImage != null)
			{
				return highlightImage.enabled;
			}
			return false;
		}
		set
		{
			if (!(highlightImage == null))
			{
				highlightImage.enabled = value;
			}
		}
	}

	public bool IsAnimating
	{
		get
		{
			if (healthDisplay != null)
			{
				return healthDisplay.IsAnimating;
			}
			return false;
		}
	}

	public Transform Transform => base.transform;

	public void CompleteCurrentHealthAnimation()
	{
		healthDisplay?.CompleteCurrentAnimation();
	}

	public void DisplayBrazierIfNeeded(bool? forceShow = null)
	{
		bool toggle = forceShow ?? ShouldBrazierBeDisplayed();
		brazierCanvas.enabled = toggle;
		brazierText.enabled = toggle;
		brazierDisplay.ToggleSliders(toggle);
	}

	public void DisplayHealthIfNeeded()
	{
		HealthDisplayed = ShouldHealthBeDisplayed();
		DisplayProductionIfNeeded();
	}

	public void DisplayIfNeeded()
	{
		DisplayHealthIfNeeded();
	}

	public void PlayArmorGainAnim(float armorGain, float armorAfterGain)
	{
		TPSingleton<BuildingManager>.Instance.LogError("Buildings don't have armor, so BuildingHUD can't PlayArmorGainAnim()");
	}

	public void PlayBrazierLossAnim(float brazierLoss, float brazierAfterLoss)
	{
		StartCoroutine(GaugeBrazierLossDisplayCoroutine(brazierLoss, brazierAfterLoss));
	}

	public void PlayDamageAnim(AttackSkillActionExecutionTileData attackData)
	{
		StartCoroutine(GaugeDamageDisplayCoroutine(attackData));
	}

	public void PlayHealthGainAnim(float healthGain, float healthAfterGain)
	{
		StartCoroutine(GaugeHealDisplayCoroutine(healthGain, healthAfterGain));
	}

	public void RefreshHealth()
	{
		healthDisplay.RefreshStatInstantly(-1f, GetHealthTotal());
		DisplayHealthIfNeeded();
	}

	public void RefreshPositionInstantly()
	{
		followElement.AutoMove();
	}

	protected virtual void DisplayProductionIfNeeded()
	{
		if (building.ProductionModule?.BuildingGaugeEffect != null && TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying)
		{
			production.SetActive(value: true);
			productionIcon.sprite = building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetProductionRewardIconSpriteBig();
		}
		else
		{
			production.SetActive(value: false);
		}
	}

	protected virtual bool ShouldHealthBeDisplayed()
	{
		if (!building.BlueprintModule.IsIndestructible && building.DamageableModule.HealthTotal > 0f && (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction || building.DamageableModule.DamageableModuleDefinition.KeepHUDDisplayedWhenDamaged) && building.DamageableModule.Health < building.DamageableModule.HealthTotal && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying)
		{
			return TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver;
		}
		return false;
	}

	private void Awake()
	{
		if (healthCanvas != null)
		{
			healthCanvas.worldCamera = ACameraView.MainCam;
		}
		if (healthDisplay != null)
		{
			healthDisplay.AnimatedDisplayFinishEvent += OnAnimatedDisplayFinished;
		}
		healthDisplayStored = healthDisplay;
	}

	private IEnumerator GaugeDamageDisplayCoroutine(AttackSkillActionExecutionTileData attackData)
	{
		if (!(attackData.TotalDamage <= 0f))
		{
			if (hideHealthDisplayCoroutine != null)
			{
				StopCoroutine(hideHealthDisplayCoroutine);
				hideHealthDisplayCoroutine = null;
			}
			HealthDisplayed = !Building.OriginTile.HasFog;
			float targetNormalizedValue = ((!(attackData.TargetTile.Building is MagicCircle magicCircle)) ? (attackData.TargetRemainingHealth / attackData.TargetHealthTotal) : (attackData.TargetRemainingHealth / magicCircle.CurrentHealthTotal));
			yield return healthDisplay.DecreaseDisplayCoroutine(targetNormalizedValue);
		}
	}

	private IEnumerator GaugeHealDisplayCoroutine(float healAmount, float healthAfterHeal)
	{
		if (!(healAmount <= 0f))
		{
			if (hideHealthDisplayCoroutine != null)
			{
				StopCoroutine(hideHealthDisplayCoroutine);
				hideHealthDisplayCoroutine = null;
			}
			HealthDisplayed = !Building.OriginTile.HasFog;
			float targetNormalizedValue = healthAfterHeal / GetHealthTotal();
			yield return healthDisplay.IncreaseDisplayCoroutine(targetNormalizedValue);
		}
	}

	protected virtual float GetHealthTotal()
	{
		return Building.DamageableModule.HealthTotal;
	}

	private IEnumerator GaugeBrazierLossDisplayCoroutine(float brazierLoss, float brazierAfterLoss)
	{
		if (brazierLoss <= 0f)
		{
			yield break;
		}
		float targetNormalizedValue = brazierAfterLoss / (float)Building.BrazierModule.BrazierPointsTotal;
		yield return brazierDisplay.DecreaseDisplayCoroutine(targetNormalizedValue);
		if (brazierDisplay.IsAnimatingDamage)
		{
			yield return new WaitUntil(() => !brazierDisplay.IsAnimatingDamage);
		}
		if (brazierAfterLoss == 0f)
		{
			building.BrazierModule.IsExtinguishing = true;
			building.BuildingController.PassivesModuleController?.ApplyPassiveEffect(E_EffectTime.OnExtinguish);
		}
	}

	private IEnumerator HideHealthDisplayCoroutine()
	{
		yield return SharedYields.WaitForSeconds(healthHideAfterAnimDelay);
		DisplayHealthIfNeeded();
		hideHealthDisplayCoroutine = null;
	}

	private void OnAnimatedDisplayFinished()
	{
		Building.BuildingView.RefreshBuildingDamagedAppearance();
		hideHealthDisplayCoroutine = StartCoroutine(HideHealthDisplayCoroutine());
	}

	private void OnDisable()
	{
		DisplayBrazierIfNeeded(false);
	}

	private void OnEnable()
	{
		healthDisplay = healthDisplayStored;
	}

	private bool ShouldBrazierBeDisplayed()
	{
		if (building.BrazierModule != null)
		{
			if (building.BrazierModule.BrazierPoints >= building.BrazierModule.BrazierPointsTotal)
			{
				return building.BuildingView.HoveredOrSelected;
			}
			return true;
		}
		return false;
	}
}
