using DG.Tweening;
using TheLastStand.Database;
using TheLastStand.Definition.Panic;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model.Panic;
using UnityEngine;

namespace TheLastStand.View.Panic;

public class PanicPanel : MonoBehaviour
{
	[SerializeField]
	private float closedPanelPosY;

	[SerializeField]
	private float openedPanelPosY = -100f;

	[SerializeField]
	[Range(0f, 5f)]
	private float openPanelTweenDuration = 0.7f;

	[SerializeField]
	[Range(0f, 5f)]
	private float closePanelTweenDuration = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float panicGaugeTweenDuration = 0.2f;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private BetterSlider panicGaugeSlider;

	[SerializeField]
	private BetterSlider panicPreviewGaugeSlider;

	[SerializeField]
	private Transform panicLevelsPanelTransform;

	[SerializeField]
	private PanicLevel panicLevelPrefab;

	private PanicLevel[] panicLevels;

	private Sequence movePanelSequence;

	private Sequence refreshPanicGaugeSequence;

	private float targetPanicGaugeSliderValue;

	private Sequence refreshPanicPreviewGaugeSequence;

	private float targetPanicPreviewGaugeSliderValue;

	private float panicLevelsPanelWidth;

	public float ClosePanelTweenDuration => closePanelTweenDuration;

	public void Close()
	{
		if (base.gameObject.activeInHierarchy)
		{
			panicLevelsPanelTransform.gameObject.SetActive(value: false);
			if (movePanelSequence == null)
			{
				movePanelSequence = DOTween.Sequence().SetId("MovePanel");
			}
			movePanelSequence.Append(rectTransform.DOAnchorPosY(closedPanelPosY, closePanelTweenDuration, snapping: true).SetFullId("ClosePanicPanel", this).SetEase(Ease.InBack)
				.OnComplete(delegate
				{
					base.gameObject.SetActive(value: false);
				}));
		}
	}

	public void InstantiatePanicLevels()
	{
		panicLevels = new PanicLevel[PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length];
		for (int num = PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length - 1; num > 0; num--)
		{
			PanicLevelDefinition panicLevelDefinition = PanicDatabase.PanicDefinition.PanicLevelDefinitions[num];
			PanicLevel panicLevel = Object.Instantiate(panicLevelPrefab, panicLevelsPanelTransform);
			panicLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(panicLevelDefinition.PanicValueNeeded / PanicDatabase.PanicDefinition.ValueMax * panicLevelsPanelWidth, 0f);
			panicLevel.InstantiateIndicators(num);
			panicLevels[num] = panicLevel;
		}
	}

	public void Open()
	{
		if (!base.gameObject.activeInHierarchy && UIManager.DebugToggleUI != false)
		{
			base.gameObject.SetActive(value: true);
			if (movePanelSequence == null)
			{
				movePanelSequence = DOTween.Sequence().SetId("MovePanel");
			}
			movePanelSequence.Append(rectTransform.DOAnchorPosY(openedPanelPosY, openPanelTweenDuration, snapping: true).SetFullId("OpenPanicPanel", this).SetEase(Ease.OutBounce)
				.OnComplete(delegate
				{
					panicLevelsPanelTransform.gameObject.SetActive(value: true);
				}));
		}
	}

	public void Refresh(TheLastStand.Model.Panic.Panic panic)
	{
		RefreshPanicValue(panic);
		RefreshPanicExpectedValue(panic);
	}

	public void RefreshPanicExpectedValue(TheLastStand.Model.Panic.Panic panic)
	{
		float num = panic.ExpectedValue / panic.PanicDefinition.ValueMax;
		if (num != targetPanicPreviewGaugeSliderValue)
		{
			targetPanicPreviewGaugeSliderValue = num;
			if (refreshPanicPreviewGaugeSequence == null)
			{
				refreshPanicPreviewGaugeSequence = DOTween.Sequence().SetId("RefreshPanicPreviewGauge");
			}
			refreshPanicPreviewGaugeSequence.Append(DOTween.To(() => panicPreviewGaugeSlider.value, delegate(float x)
			{
				panicPreviewGaugeSlider.value = x;
			}, targetPanicPreviewGaugeSliderValue, panicGaugeTweenDuration).SetFullId("RefreshPanicPreviewGauge", this));
		}
		RefreshPanicLevels(panic);
	}

	public void RefreshPanicLevels(TheLastStand.Model.Panic.Panic panic)
	{
		if (UIManager.DebugToggleUI == false)
		{
			return;
		}
		for (int num = PanicDatabase.PanicDefinition.PanicLevelDefinitions.Length - 1; num > 0; num--)
		{
			if (num <= panic.Level)
			{
				panicLevels[num].Activate();
				panicLevels[num].DisactivateThresholdMaterial();
			}
			else
			{
				panicLevels[num].Disactivate();
				if (num <= panic.ExpectedLevel)
				{
					panicLevels[num].ActivateThresholdMaterial();
				}
				else
				{
					panicLevels[num].DisactivateThresholdMaterial();
				}
			}
		}
	}

	public void RefreshPanicValue(TheLastStand.Model.Panic.Panic panic)
	{
		float num = panic.Value / panic.PanicDefinition.ValueMax;
		if (num != targetPanicGaugeSliderValue)
		{
			targetPanicGaugeSliderValue = num;
			if (refreshPanicGaugeSequence == null)
			{
				refreshPanicGaugeSequence = DOTween.Sequence().SetId("RefreshPanicGauge");
			}
			refreshPanicGaugeSequence.Append(DOTween.To(() => panicGaugeSlider.value, delegate(float value)
			{
				panicGaugeSlider.value = value;
			}, targetPanicGaugeSliderValue, panicGaugeTweenDuration).SetFullId("RefreshPanicGauge", this));
			RefreshPanicLevels(panic);
		}
	}

	private void Awake()
	{
		panicLevelsPanelWidth = panicLevelsPanelTransform.GetComponent<RectTransform>().sizeDelta.x;
		InstantiatePanicLevels();
	}
}
