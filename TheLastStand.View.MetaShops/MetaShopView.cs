using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Helpers;
using TheLastStand.Manager;
using TheLastStand.View.HUD;
using TheLastStand.View.MetaNarration;
using TheLastStand.View.MetaShops.JoystickNavigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class MetaShopView : MonoBehaviour
{
	public static class Constants
	{
		public const float CycleThroughNewEntriesAvailableAlpha = 1f;

		public const float CycleThroughNewEntriesUnavailableAlpha = 0.15f;
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private Button backButton;

	[SerializeField]
	private GameObject backButtonParent;

	[SerializeField]
	private GoddessView goddessView;

	[SerializeField]
	private NarrationView narrationView;

	[SerializeField]
	private List<MetaShopTab> tabs;

	[SerializeField]
	private MetaShopSorter sorter;

	[SerializeField]
	private TextMeshProUGUI noUpgradeText;

	[SerializeField]
	private GameObject exitText;

	[SerializeField]
	private RectTransform shopPositionContainer;

	[SerializeField]
	private RectTransform shopPositionTarget;

	[SerializeField]
	protected TextMeshProUGUI unlockProgressionText;

	[SerializeField]
	private Animator fxAnimator;

	[SerializeField]
	private Scrollbar scrollbar;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected ContentSizeFitter scrollViewContentSizeFitter;

	[SerializeField]
	protected VerticalLayoutGroup scrollViewLayoutGroup;

	[SerializeField]
	protected RectTransform scrollViewport;

	[SerializeField]
	private List<MetaShopFilter> filters;

	[SerializeField]
	private MetaShopFilterLink filtersLink;

	[SerializeField]
	private HUDJoystickSimpleTarget upgradesJoystickSimpleTarget;

	[SerializeField]
	private HUDJoystickDynamicTarget joystickDynamicTarget;

	[SerializeField]
	private AFiltersToUpgradeNavigation filtersToUpgradeNavigation;

	public Canvas Canvas => canvas;

	public CanvasGroup CanvasGroup => canvasGroup;

	public GameObject ExitText => exitText;

	public List<MetaShopFilter> Filters => filters;

	public List<MetaUpgradeLineView> Lines { get; private set; } = new List<MetaUpgradeLineView>();

	public Animator FxAnimator => fxAnimator;

	public RectTransform FxTransform => FxAnimator.transform as RectTransform;

	public GoddessView GoddessView => goddessView;

	public RectTransform LayoutGroupContainer => scrollViewLayoutGroup.transform as RectTransform;

	public NarrationView NarrationView => narrationView;

	public ScrollRect ScrollRect => scrollRect;

	public MetaShopSorter Sorter => sorter;

	public List<MetaShopTab> Tabs => tabs;

	public HUDJoystickDynamicTarget JoystickDynamicTarget => joystickDynamicTarget;

	public void AddLine(MetaUpgradeLineView newLine)
	{
		Lines.Add(newLine);
		upgradesJoystickSimpleTarget?.AddSelectable(newLine.JoystickSelectable);
	}

	public void ResetView()
	{
		goddessView.FadeInContainer.SetActive(value: false);
		goddessView.IdleContainer.SetActive(value: false);
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = 0f;
			CanvasGroup.interactable = false;
		}
		backButtonParent.gameObject.SetActive(value: false);
		narrationView.Hide();
	}

	public void OnSlotViewJoystickSelect(RectTransform item)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(item, scrollViewport, scrollbar, 0.01f, 0.01f);
	}

	public void ResetScrollbar(bool forceRefresh = false)
	{
		UpdateAllLinesDisplay(forceRefresh);
		scrollbar.value = 1f;
	}

	public void EnableCanvas(bool enable)
	{
		Canvas.enabled = enable;
		CanvasHelper.ScaleCanvas(canvasScaler, allowDecimals: false);
	}

	public void AddBackButtonListener(UnityAction action)
	{
		backButton.onClick.AddListener(action);
	}

	public void RefreshFilterView(MetaUpgradeDefinition.E_MetaUpgradeFilter currentFilter)
	{
		foreach (MetaShopFilter filter in filters)
		{
			filter.Toggle((filter.Filter & currentFilter) != 0);
		}
		filtersLink.Refresh();
	}

	public void RemoveBackButtonListener(UnityAction action)
	{
		backButton.onClick.RemoveListener(action);
	}

	public void ShowBackButton(bool show)
	{
		backButtonParent.gameObject.SetActive(show);
	}

	public void SnapShopPosition()
	{
		shopPositionContainer.position = shopPositionTarget.position;
	}

	public void ToggleFiltersUpNavigation(bool toggle)
	{
		foreach (MetaShopFilter filter in filters)
		{
			if (toggle)
			{
				if (filter.Selectable.navigation.selectOnUp == null)
				{
					filter.Selectable.SetSelectOnUp(filtersToUpgradeNavigation);
				}
			}
			else if (filter.Selectable.navigation.selectOnUp == filtersToUpgradeNavigation)
			{
				filter.Selectable.SetSelectOnUp(null);
			}
		}
	}

	public void ToggleLayout(bool toggle)
	{
		scrollViewContentSizeFitter.enabled = toggle;
		scrollViewLayoutGroup.enabled = toggle;
	}

	public void ToggleNoUpgradeFeedback(bool toggle)
	{
		noUpgradeText.gameObject.SetActive(toggle);
	}

	public void UpdateAllLinesDisplay(Vector2 newValue)
	{
		UpdateAllLinesDisplay();
	}

	public void UpdateAllLinesDisplay(bool forceRefresh = false)
	{
		for (int num = Lines.Count - 1; num >= 0; num--)
		{
			Lines[num].UpdateDisplay(forceRefresh);
		}
	}

	public void UpdateAllLinesDisplayAfterAFrame(bool refreshSelection = true)
	{
		StartCoroutine(UpdateAllLinesDisplayAfterAFrameCoroutine(refreshSelection));
	}

	public void UpdateProgressionText(string text)
	{
		unlockProgressionText.text = text;
	}

	private IEnumerator UpdateAllLinesDisplayAfterAFrameCoroutine(bool refreshSelection = true)
	{
		yield return null;
		UpdateAllLinesDisplay();
		if (refreshSelection && InputManager.IsLastControllerJoystick && !TPSingleton<OraculumView>.Instance.TransitionRunning)
		{
			if (TPSingleton<OraculumView>.Instance.IsInDarkShop)
			{
				StartCoroutine(TPSingleton<DarkShopManager>.Instance.SelectFirstActiveChildEndOfFrame());
				TPSingleton<DarkShopManager>.Instance.RefreshShopJoystickNavigation();
			}
			else if (TPSingleton<OraculumView>.Instance.IsInLightShop)
			{
				StartCoroutine(TPSingleton<LightShopManager>.Instance.SelectFirstActiveChildEndOfFrame());
				TPSingleton<LightShopManager>.Instance.RefreshShopJoystickNavigation();
			}
		}
	}

	private void Start()
	{
		if (CanvasGroup != null)
		{
			CanvasGroup.alpha = 0f;
		}
		ShowBackButton(show: false);
		NarrationView.Hide();
	}
}
