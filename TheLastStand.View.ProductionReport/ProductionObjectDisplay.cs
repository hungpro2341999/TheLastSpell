using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.ProductionReport;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ProductionObjectDisplay : MonoBehaviour, ISubmitHandler, IEventSystemHandler
{
	public static class Constants
	{
		public const string ProductionIconPathPrefix = "View/Sprites/UI/ProductionReportPanel/Production_";

		public const string ProductionNightRewardIconPath = "View/Sprites/UI/ProductionReportPanel/Production_NightItemReward";
	}

	[SerializeField]
	private Image productionIcon;

	[SerializeField]
	private CanvasGroup productionObjectCanvas;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI productionBuildingText;

	[SerializeField]
	private GameObject productionObjectParent;

	[SerializeField]
	private List<UIParticle> uiParticles;

	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private AudioClip goldAudioClip;

	[SerializeField]
	private AudioClip materialAudioClip;

	[SerializeField]
	private AudioClip itemAudioClip;

	private Tween fadeTween;

	public ProductionObject ProductionObject { get; set; }

	public Selectable Selectable => selectable;

	public void Disable()
	{
		productionObjectCanvas.interactable = false;
		productionObjectCanvas.blocksRaycasts = false;
		fadeTween?.Kill();
		fadeTween = productionObjectCanvas.DOFade(0f, 0.3f).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			productionObjectParent.SetActive(value: false);
			TPSingleton<ProductionReportPanel>.Instance.CheckOnProductionObjectHide();
		});
		ToggleUIParticles(toggle: false);
	}

	public void Hide()
	{
		ToggleUIParticles(toggle: false);
	}

	public void OnProductionObjectClick()
	{
		if (ProductionObject is ProductionItems productionItems && productionItems.Items.Count > 0)
		{
			TPSingleton<ChooseRewardPanel>.Instance.ProductionItem = productionItems;
			TPSingleton<ChooseRewardPanel>.Instance.Open();
			TPSingleton<UIManager>.Instance.PlayAudioClip(itemAudioClip);
		}
	}

	public void Display()
	{
		fadeTween?.Kill();
		productionObjectCanvas.interactable = true;
		productionObjectCanvas.blocksRaycasts = true;
		fadeTween = productionObjectCanvas.DOFade(1f, 0f);
		Sprite sprite = null;
		if (ProductionObject.ProductionBuildingDefinition != null)
		{
			sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/ProductionReportPanel/Production_" + ProductionObject.ProductionBuildingDefinition.Id);
		}
		else if (ProductionObject is ProductionItems { IsNightProduction: not false })
		{
			sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/ProductionReportPanel/Production_NightItemReward");
		}
		if (sprite != null)
		{
			productionIcon.sprite = sprite;
		}
		ToggleUIParticles(toggle: true);
		RefreshText();
	}

	public void RefreshText()
	{
		productionBuildingText.text = Localizer.Get((ProductionObject.ProductionBuildingDefinition != null) ? ("BuildingName_" + ProductionObject.ProductionBuildingDefinition.Id) : "NightReportPanel_NightRewardObject");
		titleText.text = Localizer.Get("ProductionObject_ItemProduction");
	}

	public void OnHover(bool display)
	{
		if (display)
		{
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport)
			{
				TPSingleton<ProductionReportPanel>.Instance.AdjustScrollView((RectTransform)base.transform);
			}
			button.OnPointerEnter(null);
		}
		else
		{
			button.OnPointerExit(null);
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OnProductionObjectClick();
	}

	protected void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (base.gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	private void ToggleUIParticles(bool toggle)
	{
		foreach (UIParticle uiParticle in uiParticles)
		{
			uiParticle.enabled = toggle;
		}
	}
}
