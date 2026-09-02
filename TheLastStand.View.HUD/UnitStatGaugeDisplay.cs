using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class UnitStatGaugeDisplay : MonoBehaviour
{
	[SerializeField]
	private UnitStatDefinition.E_Stat statToUse = UnitStatDefinition.E_Stat.Health;

	[SerializeField]
	private Gradient decreaseGradient;

	[SerializeField]
	protected float decreasePhaseOneDuration = 0.1f;

	[SerializeField]
	protected Ease decreasePhaseOneEasing = Ease.InQuint;

	[SerializeField]
	[Tooltip("Phase 2 will start [this duration] after the end of Phase 1")]
	private float decreaseDelayBetweenPhases = 0.2f;

	[SerializeField]
	private float decreasePhaseTwoDuration = 0.3f;

	[SerializeField]
	private Ease decreasePhaseTwoEasing = Ease.InQuint;

	[SerializeField]
	private Gradient increaseGradient;

	[SerializeField]
	private float increasePhaseOneDuration = 0.1f;

	[SerializeField]
	private Ease increasePhaseOneEasing = Ease.InQuint;

	[SerializeField]
	[Tooltip("Phase 2 will start [this duration] after the end of Phase 1")]
	private float increaseDelayBetweenPhases = 0.2f;

	[SerializeField]
	protected float increasePhaseTwoDuration = 0.3f;

	[SerializeField]
	protected Ease increasePhaseTwoEasing = Ease.InQuint;

	[SerializeField]
	private RectTransform markersContainer;

	[SerializeField]
	private TextMeshProUGUI healthValueLbl;

	[SerializeField]
	private bool onlyDisplayValueDuringPhaseTwo;

	[SerializeField]
	private float hideValueLabelDelay = 0.3f;

	[SerializeField]
	protected Slider statGauge;

	[SerializeField]
	protected Slider hitGauge;

	protected GaugeMarkersDisplayer gaugeMarkersDisplayer;

	private IDamageable damageable;

	private TheLastStand.Model.Building.Building building;

	private float currentMaxValue = -1f;

	private Image hitGaugeImage;

	private float decreasePhaseTwoDelayTimer = -1f;

	private Tweener decreaseDisplayPhaseOneTweener;

	private Tween decreaseColorPhaseOneTweener;

	private Tweener decreaseDisplayPhaseTwoTweener;

	private Coroutine decreaseDisplayPhaseTwoCoroutine;

	private Coroutine decreasePhaseTwoDelayCoroutine;

	private float increasePhaseTwoDelayTimer = -1f;

	private Tweener increaseDisplayPhaseOneTweener;

	private Tween increaseColorPhaseOneTweener;

	private Tweener increaseDisplayPhaseTwoTweener;

	private Coroutine increaseDisplayPhaseTwoCoroutine;

	private Coroutine increasePhaseTwoDelayCoroutine;

	private Coroutine hideValueLabelCoroutine;

	public TheLastStand.Model.Building.Building Building
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
				RefreshStatInstantly();
			}
		}
	}

	public IDamageable Damageable
	{
		get
		{
			return damageable;
		}
		set
		{
			if (healthValueLbl != null)
			{
				healthValueLbl.enabled = false;
			}
			damageable = value;
			if (value != null)
			{
				RefreshStatInstantly();
			}
		}
	}

	public bool IsAnimating
	{
		get
		{
			if (!IsAnimatingDamage)
			{
				return IsAnimatingHeal;
			}
			return true;
		}
	}

	public bool IsAnimatingDamage
	{
		get
		{
			if (decreaseDisplayPhaseOneTweener == null && decreaseDisplayPhaseTwoCoroutine == null)
			{
				return decreasePhaseTwoDelayCoroutine != null;
			}
			return true;
		}
	}

	public bool IsAnimatingHeal
	{
		get
		{
			if (increaseDisplayPhaseOneTweener == null && increaseDisplayPhaseTwoCoroutine == null)
			{
				return increasePhaseTwoDelayCoroutine != null;
			}
			return true;
		}
	}

	public event Action AnimatedDisplayFinishEvent;

	public void CompleteCurrentAnimation()
	{
		if (IsAnimatingDamage)
		{
			decreaseColorPhaseOneTweener?.Complete();
			decreaseDisplayPhaseOneTweener?.Complete();
			decreaseDisplayPhaseTwoTweener?.Complete();
		}
		if (IsAnimatingHeal)
		{
			increaseColorPhaseOneTweener?.Complete();
			increaseDisplayPhaseOneTweener?.Complete();
			increaseDisplayPhaseTwoTweener?.Complete();
		}
		if (IsAnimating)
		{
			StopAllCoroutines();
		}
	}

	public IEnumerator DecreaseDisplayCoroutine(float targetNormalizedValue)
	{
		if (statGauge == null)
		{
			yield break;
		}
		if (IsAnimatingHeal)
		{
			yield return new WaitUntil(() => !IsAnimatingHeal);
		}
		decreasePhaseTwoDelayCoroutine = StartCoroutine(DecreasePhaseTwoDelayCoroutine(targetNormalizedValue));
		yield return StartCoroutine(DecreasePhaseOneDisplayCoroutine(targetNormalizedValue));
	}

	public IEnumerator IncreaseDisplayCoroutine(float targetNormalizedValue)
	{
		if (statGauge == null)
		{
			yield break;
		}
		if (IsAnimatingDamage)
		{
			yield return new WaitUntil(() => !IsAnimatingDamage);
		}
		increasePhaseTwoDelayCoroutine = StartCoroutine(IncreasePhaseTwoDelayCoroutine(targetNormalizedValue));
		yield return StartCoroutine(IncreasePhaseOneDisplayCoroutine(targetNormalizedValue));
	}

	public virtual void RefreshStatInstantly(float statValue = -1f, float statMaxValue = -1f)
	{
		if (statValue == -1f)
		{
			statValue = GetReferenceValue();
		}
		if (statMaxValue == -1f)
		{
			statMaxValue = GetReferenceValue(useTotal: true);
		}
		if (statMaxValue != currentMaxValue)
		{
			gaugeMarkersDisplayer.RefreshMarkers(statMaxValue, statValue);
			currentMaxValue = statMaxValue;
		}
		float normalizedValue = statValue / statMaxValue;
		gaugeMarkersDisplayer.RefreshEnabledMarkers(normalizedValue);
		if (statGauge != null)
		{
			statGauge.normalizedValue = normalizedValue;
		}
		if (hitGauge != null)
		{
			hitGauge.normalizedValue = normalizedValue;
		}
		if (healthValueLbl != null)
		{
			healthValueLbl.text = $"{(int)statValue}";
		}
	}

	public void ResetTweeners()
	{
		decreaseDisplayPhaseOneTweener?.Kill();
		decreaseDisplayPhaseTwoTweener?.Kill();
		decreaseDisplayPhaseOneTweener = null;
		decreaseDisplayPhaseTwoTweener = null;
	}

	public virtual void ToggleSliders(bool toggle)
	{
		hitGauge.enabled = toggle;
		statGauge.enabled = toggle;
	}

	protected virtual IEnumerator DecreasePhaseOneDisplayCoroutine(float targetNormalizedValue, bool updateMarkers = true)
	{
		float normalizedValue = statGauge.normalizedValue;
		if (normalizedValue == targetNormalizedValue)
		{
			yield break;
		}
		if (decreaseDisplayPhaseOneTweener == null)
		{
			if (healthValueLbl != null && onlyDisplayValueDuringPhaseTwo)
			{
				DisplayValueLabel();
			}
			decreaseDisplayPhaseOneTweener = DOTween.To(() => normalizedValue, delegate(float v)
			{
				normalizedValue = v;
				if (statGauge != null)
				{
					statGauge.normalizedValue = normalizedValue;
				}
				if (updateMarkers)
				{
					gaugeMarkersDisplayer.RefreshEnabledMarkers(normalizedValue);
				}
			}, targetNormalizedValue, decreasePhaseOneDuration).SetFullId("HealthDisplay DamageDisplayPhase1", this).SetEase(decreasePhaseOneEasing)
				.OnKill(delegate
				{
					decreaseDisplayPhaseOneTweener = null;
				});
			decreaseColorPhaseOneTweener = hitGaugeImage.DOGradientColor(decreaseGradient, decreasePhaseOneDuration + decreaseDelayBetweenPhases + decreasePhaseTwoDuration).SetFullId("HealthDisplay DamageColor", this).OnKill(delegate
			{
				decreaseColorPhaseOneTweener = null;
			});
		}
		else
		{
			CLoggerManager.Log($"reuse tween phase1, changing value to : {targetNormalizedValue}", this, LogType.Log, CLogLevel.NORMAL, forcePrintInUnity: false);
			decreaseDisplayPhaseOneTweener.ChangeEndValue(targetNormalizedValue, snapStartValue: true);
			decreaseColorPhaseOneTweener.Restart();
		}
		yield return decreaseDisplayPhaseOneTweener.WaitForCompletion();
	}

	protected virtual IEnumerator DecreasePhaseTwoDisplayCoroutine(float targetNormalizedValue)
	{
		float normalizedValue = hitGauge.normalizedValue;
		if (decreaseDisplayPhaseTwoTweener == null)
		{
			decreaseDisplayPhaseTwoTweener = DOTween.To(() => normalizedValue, delegate(float v)
			{
				normalizedValue = v;
				if (hitGauge != null)
				{
					hitGauge.normalizedValue = normalizedValue;
				}
				if (healthValueLbl != null)
				{
					healthValueLbl.text = Mathf.RoundToInt(normalizedValue * GetReferenceValue(useTotal: true)).ToString();
				}
			}, targetNormalizedValue, decreasePhaseTwoDuration).SetFullId("UnitHUD DamageDisplayPhase2", this).SetEase(decreasePhaseTwoEasing)
				.OnComplete(delegate
				{
					decreaseDisplayPhaseTwoTweener = null;
					decreaseDisplayPhaseTwoCoroutine = null;
					this.AnimatedDisplayFinishEvent?.Invoke();
					if (healthValueLbl != null && onlyDisplayValueDuringPhaseTwo)
					{
						hideValueLabelCoroutine = StartCoroutine(HideValueLabelDelayedCoroutine());
					}
				});
		}
		else
		{
			decreaseDisplayPhaseTwoTweener.ChangeEndValue(targetNormalizedValue, snapStartValue: true);
		}
		yield return decreaseDisplayPhaseTwoTweener.WaitForCompletion();
	}

	protected virtual IEnumerator IncreasePhaseOneDisplayCoroutine(float targetNormalizedValue)
	{
		float normalizedValue = hitGauge.normalizedValue;
		if (normalizedValue == targetNormalizedValue)
		{
			yield break;
		}
		if (increaseDisplayPhaseOneTweener == null)
		{
			if (healthValueLbl != null && onlyDisplayValueDuringPhaseTwo)
			{
				DisplayValueLabel();
			}
			increaseDisplayPhaseOneTweener = DOTween.To(() => normalizedValue, delegate(float v)
			{
				normalizedValue = v;
				if (hitGauge != null)
				{
					hitGauge.normalizedValue = normalizedValue;
				}
			}, targetNormalizedValue, increasePhaseOneDuration).SetFullId("HealthDisplay HealDisplayPhase1", this).SetEase(increasePhaseOneEasing)
				.OnKill(delegate
				{
					increaseDisplayPhaseOneTweener = null;
				});
			increaseColorPhaseOneTweener = hitGaugeImage.DOGradientColor(increaseGradient, increasePhaseOneDuration + increaseDelayBetweenPhases + increasePhaseTwoDuration).SetId("HealthDisplay HealColor").OnKill(delegate
			{
				increaseColorPhaseOneTweener = null;
			});
		}
		else
		{
			increaseDisplayPhaseOneTweener.ChangeEndValue(targetNormalizedValue, snapStartValue: true);
			increaseColorPhaseOneTweener.Restart();
		}
		yield return increaseDisplayPhaseOneTweener.WaitForCompletion();
	}

	protected virtual IEnumerator IncreasePhaseTwoDisplayCoroutine(float targetNormalizedValue, bool updateMarkers = true)
	{
		float normalizedValue = statGauge.normalizedValue;
		if (increaseDisplayPhaseTwoTweener == null)
		{
			increaseDisplayPhaseTwoTweener = DOTween.To(() => normalizedValue, delegate(float v)
			{
				normalizedValue = v;
				if (hitGauge != null)
				{
					statGauge.normalizedValue = normalizedValue;
				}
				if (healthValueLbl != null)
				{
					healthValueLbl.text = Mathf.RoundToInt(normalizedValue * GetReferenceValue(useTotal: true)).ToString();
				}
				if (updateMarkers)
				{
					gaugeMarkersDisplayer.RefreshEnabledMarkers(normalizedValue);
				}
			}, targetNormalizedValue, increasePhaseTwoDuration).SetFullId("UnitHUD HealDisplayPhase2", this).SetEase(increasePhaseTwoEasing)
				.OnComplete(delegate
				{
					increaseDisplayPhaseTwoTweener = null;
					increaseDisplayPhaseTwoCoroutine = null;
					this.AnimatedDisplayFinishEvent?.Invoke();
					if (healthValueLbl != null && onlyDisplayValueDuringPhaseTwo)
					{
						hideValueLabelCoroutine = StartCoroutine(HideValueLabelDelayedCoroutine());
					}
				});
		}
		else
		{
			increaseDisplayPhaseTwoTweener.ChangeEndValue(targetNormalizedValue, snapStartValue: true);
		}
		yield return increaseDisplayPhaseTwoTweener.WaitForCompletion();
	}

	private void Awake()
	{
		decreasePhaseTwoDelayTimer = decreasePhaseOneDuration + decreaseDelayBetweenPhases;
		increasePhaseTwoDelayTimer = increasePhaseOneDuration + increaseDelayBetweenPhases;
		if (hitGauge != null)
		{
			hitGaugeImage = hitGauge.fillRect.GetComponent<Image>();
		}
		if (healthValueLbl != null && onlyDisplayValueDuringPhaseTwo)
		{
			healthValueLbl.enabled = false;
		}
		gaugeMarkersDisplayer = new GaugeMarkersDisplayer(markersContainer);
	}

	private IEnumerator DecreasePhaseTwoDelayCoroutine(float targetNormalizedValue)
	{
		yield return SharedYields.WaitForSeconds(decreasePhaseTwoDelayTimer);
		decreaseDisplayPhaseTwoCoroutine = StartCoroutine(DecreasePhaseTwoDisplayCoroutine(targetNormalizedValue));
		decreasePhaseTwoDelayCoroutine = null;
	}

	private void DisplayValueLabel()
	{
		healthValueLbl.enabled = true;
		if (hideValueLabelCoroutine != null)
		{
			StopCoroutine(hideValueLabelCoroutine);
			hideValueLabelCoroutine = null;
		}
	}

	private float GetReferenceValue(bool useTotal = false)
	{
		switch (statToUse)
		{
		case UnitStatDefinition.E_Stat.Armor:
			if (!useTotal)
			{
				return damageable.Armor;
			}
			return damageable.ArmorTotal;
		case UnitStatDefinition.E_Stat.Health:
			if (damageable is DamageableModule { BuildingParent: MagicCircle buildingParent })
			{
				if (!useTotal)
				{
					return damageable.Health;
				}
				return buildingParent.CurrentHealthTotal;
			}
			if (!useTotal)
			{
				return damageable.Health;
			}
			return damageable.HealthTotal;
		case UnitStatDefinition.E_Stat.Mana:
			if (damageable is TheLastStand.Model.Unit.Unit unit)
			{
				if (!useTotal)
				{
					return unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana);
				}
				return unit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal);
			}
			if (damageable is PlayableUnit playableUnit)
			{
				if (!useTotal)
				{
					return playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana);
				}
				return playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal);
			}
			break;
		default:
			if (building?.BrazierModule != null)
			{
				return useTotal ? building.BrazierModule.BrazierPointsTotal : building.BrazierModule.BrazierPoints;
			}
			break;
		}
		Debug.LogError("Could not get a correct reference value to update gauge display!", base.gameObject);
		return 0f;
	}

	private IEnumerator IncreasePhaseTwoDelayCoroutine(float targetNormalizedValue)
	{
		yield return SharedYields.WaitForSeconds(increasePhaseTwoDelayTimer);
		increaseDisplayPhaseTwoCoroutine = StartCoroutine(IncreasePhaseTwoDisplayCoroutine(targetNormalizedValue));
		increasePhaseTwoDelayCoroutine = null;
	}

	private IEnumerator HideValueLabelDelayedCoroutine()
	{
		yield return SharedYields.WaitForSeconds(hideValueLabelDelay);
		healthValueLbl.enabled = false;
		hideValueLabelCoroutine = null;
	}
}
