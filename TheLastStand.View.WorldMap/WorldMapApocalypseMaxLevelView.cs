using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Rewired;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Animation;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.HUD;
using TheLastStand.View.WorldMap.Apocalypse;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class WorldMapApocalypseMaxLevelView : TPSingleton<WorldMapApocalypseMaxLevelView>
{
	[SerializeField]
	private FloatTweenAnimation foldAnimation;

	[SerializeField]
	private float foldDuration;

	[SerializeField]
	private float foldInitialDuration;

	[SerializeField]
	private ApocalypseLevelView apocalypseLevelView;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	private RectTransform scrollViewRectTransform;

	[SerializeField]
	private RectTransform scrollRectRectTransform;

	[SerializeField]
	private BetterButton leftButton;

	[SerializeField]
	private BetterButton rightButton;

	[SerializeField]
	private LayoutGroup apocalypseLayoutGroup;

	[SerializeField]
	private RectTransform foldRectTransform;

	[SerializeField]
	private ApocalypseMaxLevelCityView cityViewPrefab;

	[SerializeField]
	private float scrollSensitivity = 0.1f;

	[SerializeField]
	private HUDJoystickSimpleTarget cityViewsJoystickSimpleTarget;

	[SerializeField]
	private LayoutNavigationInitializer scrollViewNavigationInitializer;

	private List<ApocalypseMaxLevelCityView> apocalypseMaxLevelCityViews = new List<ApocalypseMaxLevelCityView>();

	private bool isFolded = true;

	public bool CanFocusUsingJoystick
	{
		get
		{
			if (ApocalypseManager.IsApocalypseUnlocked)
			{
				if (TPSingleton<WorldMapStateManager>.Instance.CurrentState != WorldMapStateManager.WorldMapState.DEFAULT)
				{
					return TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.EXPLORATION;
				}
				return true;
			}
			return false;
		}
	}

	public void AdjustScrollView(RectTransform modifierDisplay)
	{
	}

	public void CheckIfMustToCreateCityViews()
	{
		List<WorldMapCity> orderedCities = TPSingleton<WorldMapCityManager>.Instance.GetOrderedCities(ValidCityView);
		if (orderedCities.Count > apocalypseMaxLevelCityViews.Count)
		{
			StartCoroutine(PopulateApocalypseCityViews(orderedCities));
		}
	}

	public void FocusUsingJoystick()
	{
		if (TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.APOCALYPSE_MAX_LEVEL_FOCUS);
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(cityViewsJoystickSimpleTarget.GetSelectionInfo());
		}
	}

	public void OnExitJoystickFocus(bool triggerChangeState)
	{
		if (TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.APOCALYPSE_MAX_LEVEL_FOCUS)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			if (triggerChangeState)
			{
				WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.EXPLORATION);
			}
		}
	}

	public void OnLeftButtonClick()
	{
		scrollBar.value = Mathf.Clamp01(scrollBar.value - scrollSensitivity);
	}

	public void OnRightButtonClick()
	{
		scrollBar.value = Mathf.Clamp01(scrollBar.value + scrollSensitivity);
	}

	public void OnStateChange()
	{
		switch (TPSingleton<WorldMapStateManager>.Instance.CurrentState)
		{
		case WorldMapStateManager.WorldMapState.EXPLORATION:
			Unfold(foldDuration);
			break;
		case WorldMapStateManager.WorldMapState.FOCUSED:
			Fold(foldDuration);
			break;
		}
	}

	public IEnumerator PopulateApocalypseCityViews(List<WorldMapCity> orderedCities = null)
	{
		if (orderedCities == null)
		{
			foreach (WorldMapCity city in TPSingleton<WorldMapCityManager>.Instance.Cities)
			{
				city.RefreshIsUnlocked();
				city.RefreshIsVisible();
			}
			orderedCities = TPSingleton<WorldMapCityManager>.Instance.GetOrderedCities(ValidCityView);
		}
		int num = orderedCities.Count - apocalypseMaxLevelCityViews.Count;
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				ApocalypseMaxLevelCityView item = Object.Instantiate(cityViewPrefab, scrollRectRectTransform);
				apocalypseMaxLevelCityViews.Add(item);
			}
			apocalypseLayoutGroup.enabled = true;
		}
		for (int j = 0; j < apocalypseMaxLevelCityViews.Count; j++)
		{
			ApocalypseMaxLevelCityView item = apocalypseMaxLevelCityViews[j];
			if (j < orderedCities.Count)
			{
				WorldMapCity worldMapCity = orderedCities[j];
				item.gameObject.SetActive(value: true);
				item.Init(worldMapCity.CityDefinition, worldMapCity.MaxApocalypsePassed, worldMapCity.NumberOfWins > 0);
			}
			else
			{
				item.gameObject.SetActive(value: false);
			}
		}
		yield return new WaitForEndOfFrame();
		apocalypseLayoutGroup.enabled = false;
		yield return new WaitForEndOfFrame();
		RefreshJoystickNavigation();
		Unfold(foldInitialDuration);
	}

	public void Refresh()
	{
		leftButton.Interactable = scrollViewRectTransform.rect.width < scrollRectRectTransform.rect.width;
		rightButton.Interactable = scrollViewRectTransform.rect.width < scrollRectRectTransform.rect.width;
	}

	private void Fold(float duration)
	{
		if (!isFolded && ApocalypseManager.IsApocalypseUnlocked)
		{
			foldAnimation.StatusTransitionTween?.Kill();
			foldAnimation.StatusTransitionTween = foldRectTransform.DOAnchorPosY(foldAnimation.StatusOne, foldAnimation.TransitionDuration).SetEase(foldAnimation.TransitionEase).SetFullId("ApocalypseFold", this);
			foldAnimation.InStatusOne = true;
			isFolded = true;
			UIOptimization(stopAnimations: true);
		}
	}

	private void UIOptimization(bool stopAnimations)
	{
		apocalypseLevelView.StopAnimation(stopAnimations);
		foreach (ApocalypseMaxLevelCityView apocalypseMaxLevelCityView in apocalypseMaxLevelCityViews)
		{
			if (stopAnimations)
			{
				apocalypseMaxLevelCityView.PauseAnimations();
			}
			else
			{
				apocalypseMaxLevelCityView.ContinueAnimations();
			}
		}
	}

	private void Unfold(float duration)
	{
		if (isFolded && ApocalypseManager.IsApocalypseUnlocked)
		{
			foldAnimation.StatusTransitionTween?.Kill();
			foldAnimation.StatusTransitionTween = foldRectTransform.DOAnchorPosY(foldAnimation.StatusTwo, foldAnimation.TransitionDuration).SetEase(foldAnimation.TransitionEase).SetFullId("ApocalypseUnfold", this);
			foldAnimation.InStatusOne = false;
			isFolded = false;
			UIOptimization(stopAnimations: false);
		}
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		if (!isFolded && controllerType != ControllerType.Joystick && TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.APOCALYPSE_MAX_LEVEL_FOCUS)
		{
			OnExitJoystickFocus(triggerChangeState: true);
		}
	}

	private void RefreshJoystickNavigation()
	{
		cityViewsJoystickSimpleTarget.ClearSelectables();
		foreach (ApocalypseMaxLevelCityView apocalypseMaxLevelCityView in apocalypseMaxLevelCityViews)
		{
			cityViewsJoystickSimpleTarget.AddSelectable(apocalypseMaxLevelCityView.Selectable);
		}
		scrollViewNavigationInitializer.InitNavigation(reset: true);
	}

	private void Start()
	{
		if (ApocalypseManager.IsApocalypseUnlocked)
		{
			TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
			int level = TPSingleton<WorldMapCityManager>.Instance.Cities.Max((WorldMapCity o) => o.MaxApocalypsePassed);
			apocalypseLevelView.Init(level);
			StartCoroutine(PopulateApocalypseCityViews());
		}
	}

	private bool ValidCityView(WorldMapCity worldMapCity)
	{
		if (!worldMapCity.CityDefinition.IsTutorialMap && worldMapCity.IsUnlocked)
		{
			return worldMapCity.IsVisible;
		}
		return false;
	}
}
