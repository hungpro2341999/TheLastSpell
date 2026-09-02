using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Generic;
using TheLastStand.View.Unit.Perk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class PerkIconDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image perkIcon;

	[SerializeField]
	private TextMeshProUGUI dynamicValueText;

	[SerializeField]
	private TextMeshProUGUI dynamicMalusValueText;

	[SerializeField]
	private GameObject counterContainer;

	[SerializeField]
	private TextMeshProUGUI counterText;

	[SerializeField]
	private GameObject highlightSign;

	[SerializeField]
	private Canvas highlightSignCanvas;

	[SerializeField]
	private List<LocalizedFont> localizedFonts;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private FollowElement.FollowDatas perkTooltipFollowDatas;

	[SerializeField]
	private Color greyOutColor = Color.gray;

	private RectTransform rectTransform;

	public List<LocalizedFont> LocalizedFonts => localizedFonts;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RectTransform RectTransform
	{
		get
		{
			if (rectTransform == null)
			{
				rectTransform = base.transform as RectTransform;
			}
			return rectTransform;
		}
	}

	public Perk Perk { get; private set; }

	public static event Action<PerkIconDisplay> HighlightSignDisplayed;

	public event Action<PerkIconDisplay> Hovered;

	public event Action<PerkIconDisplay> Unhovered;

	public void Display(Perk unitPerk, bool greyOut, bool displayDynamicValue = true, bool displayCounter = true, bool displayMalusDynamicValue = true)
	{
		Perk = unitPerk;
		perkIcon.sprite = Perk.PerkDefinition.PerkSprite;
		perkIcon.color = (greyOut ? greyOutColor : Color.white);
		if (displayDynamicValue && !greyOut && Perk.DisplayDynamicValue(out var value))
		{
			dynamicValueText.text = value.ToString();
			dynamicValueText.gameObject.SetActive(value: true);
		}
		else
		{
			dynamicValueText.gameObject.SetActive(value: false);
		}
		if (displayMalusDynamicValue && Perk.DisplayMalusDynamicValue(out var value2))
		{
			dynamicMalusValueText.text = value2.ToString();
			dynamicMalusValueText.gameObject.SetActive(value: true);
		}
		else
		{
			dynamicMalusValueText.gameObject.SetActive(value: false);
		}
		if (displayCounter && Perk.DisplayCounter(out var counter))
		{
			counterText.text = counter.ToString();
			counterContainer.SetActive(value: true);
		}
		else
		{
			counterContainer.SetActive(value: false);
		}
		base.gameObject.SetActive(value: true);
	}

	public void DisplayHighlightSign(bool show, bool triggerEvent)
	{
		highlightSign.SetActive(show);
		if (show && triggerEvent)
		{
			PerkIconDisplay.HighlightSignDisplayed?.Invoke(this);
		}
	}

	public void OverrideHighlightSignCanvasSorting(bool state)
	{
		highlightSignCanvas.overrideSorting = state;
	}

	public void Hide()
	{
		OnPointerExit(null);
		base.gameObject.SetActive(value: false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.Hovered?.Invoke(this);
		StartCoroutine(DisplayTooltipDelayed());
		Perk.PerkController.DisplayRange(show: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.Unhovered?.Invoke(this);
		PlayableUnitManager.PerkTooltip.Hide();
		Perk.PerkController.DisplayRange(show: false);
	}

	public void OnJoystickSelect()
	{
		this.Hovered?.Invoke(this);
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			StartCoroutine(DisplayTooltipDelayed());
		}
		Perk.PerkController.DisplayRange(show: true);
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnTooltipsToggled(bool showTooltips)
	{
		if (showTooltips)
		{
			StartCoroutine(DisplayTooltipDelayed());
		}
		else
		{
			PlayableUnitManager.PerkTooltip.Hide();
		}
	}

	private IEnumerator DisplayTooltipDelayed()
	{
		yield return SharedYields.WaitForEndOfFrame;
		PerkTooltip perkTooltip = PlayableUnitManager.PerkTooltip;
		perkTooltip.CompendiumPanel.Hide();
		perkTooltip.TooltipRectTransform.anchorMin = Vector2.zero;
		perkTooltip.TooltipRectTransform.anchorMax = Vector2.zero;
		perkTooltip.TooltipRectTransform.pivot = new Vector2(0.5f, 0f);
		perkTooltip.SetContent(Perk);
		perkTooltip.FollowElement.ChangeFollowDatas(perkTooltipFollowDatas);
		perkTooltip.FollowElement.ChangeTarget(RectTransform);
		perkTooltip.Display();
	}
}
