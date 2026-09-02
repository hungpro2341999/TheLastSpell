using System;
using System.Collections;
using Sirenix.OdinInspector;
using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public abstract class UnitHUD : SerializedMonoBehaviour, IDamageableHUD
{
	[SerializeField]
	[FormerlySerializedAs("NoArmorVerticalOffset")]
	private int noArmorVerticalOffset = -5;

	[SerializeField]
	private RectTransform bgRectTransform;

	[SerializeField]
	private Canvas bgHighlightCanvas;

	[SerializeField]
	[FormerlySerializedAs("healthValueRectTransform")]
	private RectTransform offsetContainerRectTransform;

	[SerializeField]
	protected UnitHealthStatGaugeDisplay healthDisplay;

	[SerializeField]
	protected UnitStatGaugeDisplay armorDisplay;

	[SerializeField]
	private RectTransform skillTargetingAnchor;

	[SerializeField]
	private SkillActionEstimationView attackEstimationDisplay;

	[SerializeField]
	protected Image highlightImage;

	[SerializeField]
	protected Canvas highlightCanvas;

	[SerializeField]
	protected Canvas hudCanvas;

	[SerializeField]
	protected Canvas gaugesCanvas;

	[SerializeField]
	private FollowElement followElement;

	[SerializeField]
	protected Image healthGaugeImage;

	[SerializeField]
	protected Sprite healthGaugeInvulnerableSprite;

	[SerializeField]
	protected DataSpriteTable injuryStageSpritesTable;

	[SerializeField]
	protected Image injuryStageSprite;

	[SerializeField]
	protected GameObject contagionStatusGo;

	[SerializeField]
	protected GameObject buffStatusGo;

	[SerializeField]
	protected GameObject debuffStatusGo;

	[SerializeField]
	protected GameObject chargeStatusGo;

	[SerializeField]
	protected GameObject immunityStatusGo;

	[SerializeField]
	protected GameObject poisonDeathFeedback;

	private TheLastStand.Model.Unit.Unit unit;

	private float bgOriginalVerticalSize;

	private bool hasStatus;

	private float healthValueOriginalVerticalPos;

	private SkillTargetingMark skillTargetingMark;

	private int childrenViewsActive;

	private int followElementToggleBuffer;

	private Sprite healthGaugeBaseSprite;

	public SkillActionEstimationView AttackEstimationDisplay => attackEstimationDisplay;

	public Canvas BgHighlightCanvas => bgHighlightCanvas;

	public virtual bool HealthDisplayed
	{
		get
		{
			return gaugesCanvas.enabled;
		}
		set
		{
			bool flag = !PlayableUnitManager.DebugDisableHealthDisplay && value;
			if (gaugesCanvas.enabled != flag)
			{
				gaugesCanvas.enabled = flag;
				healthDisplay.ToggleSliders(flag);
				armorDisplay.ToggleSliders(flag);
				OnChildViewToggled(flag);
			}
		}
	}

	public abstract bool Highlight { get; set; }

	public bool IsAnimating
	{
		get
		{
			UnitHealthStatGaugeDisplay unitHealthStatGaugeDisplay = healthDisplay;
			if ((object)unitHealthStatGaugeDisplay == null || !unitHealthStatGaugeDisplay.IsAnimating)
			{
				return armorDisplay?.IsAnimating ?? false;
			}
			return true;
		}
	}

	public Transform Transform => base.transform;

	public TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			if (value != null)
			{
				unit = value;
				base.name = unit.UniqueIdentifier + " HUD";
				armorDisplay.Damageable = unit;
				healthDisplay.Damageable = unit;
				RefreshArmor();
				RefreshHealth();
				RefreshStatuses();
				RefreshInjuryStage();
				if (this is PlayableUnitHUD playableUnitHUD)
				{
					playableUnitHUD.RefreshMana();
				}
				if (followElement != null)
				{
					followElement.ChangeTarget(Unit.UnitView.HudFollowTarget);
					if (unit.OverrideDefaultHUDOffset)
					{
						followElement.ChangeOffset(unit.HUDOffset);
					}
				}
				Highlight = false;
			}
			else
			{
				Highlight = false;
				healthDisplay.ResetTweeners();
				armorDisplay.ResetTweeners();
				this.AnimatedDisplayFinishEvent = null;
				DisplayIconFeedback(show: false);
				unit = null;
				armorDisplay.Damageable = null;
				healthDisplay.Damageable = null;
			}
		}
	}

	protected bool DisplayArmor
	{
		get
		{
			if (armorDisplay != null)
			{
				return armorDisplay.gameObject.activeSelf;
			}
			return false;
		}
		set
		{
			if (value != DisplayArmor)
			{
				armorDisplay.gameObject.SetActive(value);
				if (bgRectTransform != null)
				{
					Vector2 sizeDelta = bgRectTransform.sizeDelta;
					sizeDelta.y = (value ? bgOriginalVerticalSize : (bgOriginalVerticalSize + (float)noArmorVerticalOffset));
					bgRectTransform.sizeDelta = sizeDelta;
				}
				if (offsetContainerRectTransform != null)
				{
					Vector2 anchoredPosition = offsetContainerRectTransform.anchoredPosition;
					anchoredPosition.y = (value ? healthValueOriginalVerticalPos : (healthValueOriginalVerticalPos + (float)noArmorVerticalOffset));
					offsetContainerRectTransform.anchoredPosition = anchoredPosition;
				}
			}
		}
	}

	protected virtual bool ShouldArmorBeDisplayed => unit.ArmorTotal > 0f;

	protected virtual bool ShouldHealthBeDisplayed
	{
		get
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying && !Unit.OriginTile.HasFog)
			{
				if (!(unit.Armor < unit.ArmorTotal) && !(unit.Health < unit.HealthTotal) && !unit.IsPoisoned)
				{
					return unit.IsStunned;
				}
				return true;
			}
			return false;
		}
	}

	protected virtual bool ShouldInjuryStageBeDisplayed
	{
		get
		{
			if (!Unit.OriginTile.HasFog)
			{
				return Unit.UnitStatsController.UnitStats.InjuryStage > 0;
			}
			return false;
		}
	}

	public event Action AnimatedDisplayFinishEvent;

	public void DisplayArmorIfNeeded()
	{
		DisplayArmor = ShouldArmorBeDisplayed;
	}

	public void DisplayHealthIfNeeded()
	{
		HealthDisplayed = ShouldHealthBeDisplayed;
	}

	public virtual void DisplayIconAndTileFeedback(bool show)
	{
		if (!show || (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits && !Unit.IsDead))
		{
			show &= Unit.WillDieByPoison;
			if (poisonDeathFeedback.activeSelf != show)
			{
				poisonDeathFeedback.SetActive(show);
				TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, Unit.OccupiedTiles, show ? "View/Tiles/Feedbacks/PoisonDeath" : null);
			}
		}
	}

	public void ToggleSkillTargeting(bool show)
	{
		if (show)
		{
			Tile tile = ((GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered() != null) ? GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit.OriginTile : TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			OnSkillTargetHover(tile == Unit.OriginTile);
			skillTargetingMark.OnShow?.Invoke();
		}
		else if (skillTargetingMark != null)
		{
			skillTargetingMark.OnHide?.Invoke();
			skillTargetingMark.gameObject.SetActive(value: false);
			ReleaseSkillTargeting();
			skillTargetingMark = null;
		}
	}

	public virtual void DisplayIconFeedback(bool show = true)
	{
		poisonDeathFeedback.SetActive(show && Unit.WillDieByPoison);
	}

	[ContextMenu("Force Display HUD")]
	public virtual void ForceDisplayHUD()
	{
		RefreshArmor();
		RefreshHealth();
		RefreshStatuses();
		RefreshInjuryStage();
		DisplayIconFeedback();
		DisplayIconAndTileFeedback(show: true);
	}

	public virtual void ForceHideHUD()
	{
		HealthDisplayed = false;
		injuryStageSprite.gameObject.SetActive(value: false);
		buffStatusGo.SetActive(value: false);
		debuffStatusGo.SetActive(value: false);
		chargeStatusGo.SetActive(value: false);
		immunityStatusGo.SetActive(value: false);
		poisonDeathFeedback.SetActive(value: false);
	}

	public virtual void OnSkillTargetHover(bool hover)
	{
		if (skillTargetingMark == null)
		{
			skillTargetingMark = ObjectPooler.GetPooledComponent("SkillTargetingMarkUI", SkillManager.SkillTargetingMarkUIPrefab, skillTargetingAnchor);
			skillTargetingMark.transform.localPosition = Vector3.zero;
		}
		if (!skillTargetingMark.gameObject.activeInHierarchy)
		{
			skillTargetingMark.gameObject.SetActive(value: true);
		}
		skillTargetingMark.SetHoverAnimatorState(hover);
	}

	public void PlayArmorGainAnim(float armorGain, float armorAfterGain)
	{
		StartCoroutine(GaugeHealArmorDisplayCoroutine(armorGain, armorAfterGain));
	}

	public void PlayDamageAnim(AttackSkillActionExecutionTileData attackData)
	{
		StartCoroutine(GaugeDamageDisplayCoroutine(attackData));
	}

	public void PlayHealthGainAnim(float healthGain, float healthAfterGain)
	{
		StartCoroutine(GaugeHealDisplayCoroutine(healthGain, healthAfterGain));
	}

	public void PlayHealthLossAnim(float healthLoss, float healthAfterLoss)
	{
		float targetNormalizedValue = healthAfterLoss / Unit.HealthTotal;
		StartCoroutine(healthDisplay.DecreaseDisplayCoroutine(targetNormalizedValue));
	}

	public void RefreshArmor()
	{
		if (Unit != null)
		{
			if (ShouldArmorBeDisplayed)
			{
				armorDisplay.RefreshStatInstantly();
			}
			DisplayArmorIfNeeded();
			DisplayHealthIfNeeded();
		}
	}

	public void RefreshHealth()
	{
		if (Unit != null)
		{
			healthDisplay.RefreshStatInstantly();
			DisplayHealthIfNeeded();
		}
	}

	public void RefreshInjuryStage()
	{
		if (Unit != null)
		{
			if (!ShouldInjuryStageBeDisplayed)
			{
				injuryStageSprite.gameObject.SetActive(value: false);
				return;
			}
			injuryStageSprite.gameObject.SetActive(value: true);
			injuryStageSprite.sprite = injuryStageSpritesTable.GetSpriteAt(Unit.UnitStatsController.UnitStats.InjuryStage - 1);
		}
	}

	public void RefreshPositionInstantly()
	{
		followElement.AutoMove();
	}

	public virtual void RefreshStat(UnitStatDefinition.E_Stat stat)
	{
		switch (stat)
		{
		case UnitStatDefinition.E_Stat.Armor:
			RefreshArmor();
			break;
		case UnitStatDefinition.E_Stat.Health:
			RefreshHealth();
			break;
		}
	}

	public virtual void RefreshStatuses()
	{
		if (Unit != null)
		{
			bool flag = !Unit.OriginTile.HasFog && (Unit.IsBuffed || Unit.IsDebuffed || Unit.IsContagious || Unit.IsCharged || Unit.IsImmune);
			if (flag != hasStatus)
			{
				hasStatus = flag;
				OnChildViewToggled(flag);
			}
			if (buffStatusGo != null)
			{
				buffStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsBuffed);
			}
			if (debuffStatusGo != null)
			{
				debuffStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsDebuffed);
			}
			if (contagionStatusGo != null)
			{
				contagionStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsContagious);
			}
			if (chargeStatusGo != null)
			{
				chargeStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsCharged);
			}
			if (immunityStatusGo != null)
			{
				immunityStatusGo.SetActive(!Unit.OriginTile.HasFog && Unit.IsImmune);
			}
			if (healthGaugeImage != null)
			{
				healthGaugeImage.sprite = (unit.IsInvulnerable ? healthGaugeInvulnerableSprite : healthGaugeBaseSprite);
			}
			DisplayHealthIfNeeded();
		}
	}

	public void ReleaseSkillTargeting()
	{
		if (!(skillTargetingMark == null))
		{
			ObjectPooler.SetPoolAsParent(skillTargetingMark.gameObject, "SkillTargetingMarkUI");
		}
	}

	public void ToggleFollowElement(bool toggle)
	{
		if (toggle)
		{
			if (++followElementToggleBuffer == 1)
			{
				followElement.enabled = true;
			}
		}
		else if (--followElementToggleBuffer == 0)
		{
			followElement.enabled = false;
		}
	}

	protected virtual void OnChildViewToggled(bool toggle)
	{
		if (toggle)
		{
			if (++childrenViewsActive == 1)
			{
				ToggleGlobalHud(toggle: true);
			}
		}
		else if (--childrenViewsActive == 0)
		{
			ToggleGlobalHud(toggle: false);
		}
	}

	private void Awake()
	{
		if (hudCanvas == null)
		{
			Debug.LogWarning("Canvas has not been referenced and will be fetched using <b>GetComponent</b> method, which should be avoided.");
			hudCanvas = GetComponent<Canvas>();
		}
		hudCanvas.worldCamera = ACameraView.MainCam;
		if (healthDisplay != null)
		{
			healthDisplay.AnimatedDisplayFinishEvent += OnAnimatedDisplayFinished;
		}
		if (armorDisplay != null)
		{
			armorDisplay.AnimatedDisplayFinishEvent += OnAnimatedDisplayFinished;
		}
		if (bgRectTransform != null)
		{
			bgOriginalVerticalSize = bgRectTransform.sizeDelta.y;
		}
		if (offsetContainerRectTransform != null)
		{
			healthValueOriginalVerticalPos = offsetContainerRectTransform.anchoredPosition.y;
		}
		if (attackEstimationDisplay != null)
		{
			attackEstimationDisplay.OnViewToggled += OnChildViewToggled;
		}
		if (healthGaugeImage != null)
		{
			healthGaugeBaseSprite = healthGaugeImage.sprite;
		}
	}

	private IEnumerator GaugeDamageDisplayCoroutine(AttackSkillActionExecutionTileData attackData)
	{
		if (!(attackData.TotalDamage <= 0f))
		{
			HealthDisplayed = !Unit.OriginTile.HasFog;
			if (attackData.ArmorDamage > 0f)
			{
				float targetNormalizedValue = attackData.TargetRemainingArmor / attackData.TargetArmorTotal;
				StartCoroutine(armorDisplay.DecreaseDisplayCoroutine(targetNormalizedValue));
			}
			if (attackData.HealthDamage > 0f)
			{
				float targetNormalizedValue2 = attackData.TargetRemainingHealth / attackData.TargetHealthTotal;
				StartCoroutine(healthDisplay.DecreaseDisplayCoroutine(targetNormalizedValue2));
			}
		}
		yield break;
	}

	private IEnumerator GaugeHealArmorDisplayCoroutine(float healAmount, float healthAfterHeal)
	{
		if (!(healAmount <= 0f))
		{
			HealthDisplayed = !Unit.OriginTile.HasFog;
			float targetNormalizedValue = healthAfterHeal / Unit.ArmorTotal;
			yield return armorDisplay.IncreaseDisplayCoroutine(targetNormalizedValue);
		}
	}

	private IEnumerator GaugeHealDisplayCoroutine(float healAmount, float healthAfterHeal)
	{
		if (!(healAmount <= 0f))
		{
			HealthDisplayed = !Unit.OriginTile.HasFog;
			float targetNormalizedValue = healthAfterHeal / Unit.HealthTotal;
			yield return healthDisplay.IncreaseDisplayCoroutine(targetNormalizedValue);
		}
	}

	private void OnAnimatedDisplayFinished()
	{
		if ((healthDisplay == null || !healthDisplay.IsAnimating) && (armorDisplay == null || !armorDisplay.IsAnimating))
		{
			this.AnimatedDisplayFinishEvent?.Invoke();
		}
		if (!Unit.IsDead && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			DisplayIconAndTileFeedback(show: true);
		}
		DisplayHealthIfNeeded();
		DisplayArmorIfNeeded();
	}

	private void OnDestroy()
	{
		if (healthDisplay != null)
		{
			healthDisplay.AnimatedDisplayFinishEvent -= OnAnimatedDisplayFinished;
		}
		if (armorDisplay != null)
		{
			armorDisplay.AnimatedDisplayFinishEvent -= OnAnimatedDisplayFinished;
		}
		if (attackEstimationDisplay != null)
		{
			attackEstimationDisplay.OnViewToggled -= OnChildViewToggled;
		}
	}

	private void ToggleGlobalHud(bool toggle)
	{
		hudCanvas.enabled = toggle;
	}

	[ContextMenu("Display Armor")]
	public void ForceDisplayArmor()
	{
		armorDisplay.gameObject.SetActive(value: true);
		DisplayArmor = true;
	}
}
