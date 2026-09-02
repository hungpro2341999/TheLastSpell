using System;
using System.Text;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.DLC;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.DLC;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model.Animation;
using TheLastStand.Model.WorldMap;
using TheLastStand.View.Camera;
using TheLastStand.View.Tutorial;
using TheLastStand.View.WorldMap.ItemRestriction;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class WorldMapCityView : MonoBehaviour
{
	private static class Constants
	{
		public const string AnimatorPathFormat = "Animators/Cities/Worldmap/{0}Animations/{0}_Animator";

		public const string IdleAnimation = "CityIdle";

		public const string HoveredAnimation = "CityHovered";

		public const string CompletedIdleAnimation = "CityCompletedIdle";

		public const string CompletedHoveredAnimation = "CityCompletedHovered";

		public const string LockedIdleAnimation = "CityLockedIdle";

		public const string LockedHoveredAnimation = "CityLockedHovered";
	}

	public WorldMapCity WorldMapCity;

	private bool isSelected;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private EventTrigger buyDLCEventTrigger;

	[SerializeField]
	private EventTrigger startEventTrigger;

	[SerializeField]
	private Transform targetPos;

	[SerializeField]
	private BetterButton buyDLCButton;

	[SerializeField]
	private TextMeshProUGUI cantStartNewGameText;

	[SerializeField]
	private Canvas cityNameCanvas;

	[SerializeField]
	private Image cityNamePanel;

	[SerializeField]
	private Sprite cityNamePanelHovered;

	[SerializeField]
	private Sprite cityNamePanelNormal;

	[SerializeField]
	private TextMeshProUGUI cityNameText;

	[SerializeField]
	private TextMeshProUGUI missingDLCText;

	[SerializeField]
	private GameObject selectionCursor;

	[SerializeField]
	private BetterButton startButton;

	[SerializeField]
	private CityDifficultyView cityDifficultyView;

	[SerializeField]
	private Animator apocalypseFlameAnimator;

	[SerializeField]
	private TextMeshProUGUI apocalypseNameText;

	[SerializeField]
	private TextMeshProUGUI apocalypseMaxLevelReachedText;

	[SerializeField]
	private Vector2TweenAnimation zoomPanelCityNameAnimation;

	[SerializeField]
	private RectTransform panelCityNameRect;

	[SerializeField]
	private Vector2TweenAnimation zoomCityNameAnimation;

	[SerializeField]
	private RectTransform cityNameRect;

	[SerializeField]
	private Vector2TweenAnimation zoomPanelDifficultySkullsScaleAnimation;

	[SerializeField]
	private Vector2TweenAnimation zoomPanelDifficultySkullsPosAnimation;

	[SerializeField]
	private RectTransform difficultyPanelRect;

	[SerializeField]
	private Vector2TweenAnimation zoomWingsAnimation;

	[SerializeField]
	private RectTransform wingsRect;

	[SerializeField]
	private Vector2TweenAnimation zoomFlamesSizeAnimation;

	[SerializeField]
	private Vector2TweenAnimation zoomFlamesPosAnimation;

	[SerializeField]
	private RectTransform flamesRect;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseNameSizeAnimation;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseNamePosAnimation;

	[SerializeField]
	private RectTransform apocalypseNameRect;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseLevelSizeAnimation;

	[SerializeField]
	private Vector2TweenAnimation zoomApocalypseLevelPosAnimation;

	[SerializeField]
	private RectTransform apocalypseLevelRect;

	private bool validApocalypseLastState = true;

	private bool validWeaponRestrictionsLastState = true;

	private bool unlockedLastState;

	public bool CanStartGame
	{
		get
		{
			if (ValidApocalypseStateToStart && ValidWeaponRestrictionsStateToStart)
			{
				return WorldMapCity.IsUnlocked;
			}
			return false;
		}
	}

	public bool IsTutorialOpened
	{
		get
		{
			if (TPSingleton<TutorialView>.Exist())
			{
				return TPSingleton<TutorialView>.Instance.DisplayCoroutineRunning;
			}
			return false;
		}
	}

	public bool ValidApocalypseStateToStart => true;

	public bool ValidWeaponRestrictionsStateToStart
	{
		get
		{
			if (TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsAvailable)
			{
				return TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.AreAllCategoriesCorrectlyConfigured();
			}
			return true;
		}
	}

	public Transform TargetPos => targetPos;

	public bool IsHovered { get; private set; }

	public void Init()
	{
		if (WorldMapCity != null)
		{
			WorldMapCity.RefreshIsSelectable();
			selectionCursor.SetActive(value: false);
			animator.runtimeAnimatorController = ResourcePooler<RuntimeAnimatorController>.LoadOnce(string.Format("Animators/Cities/Worldmap/{0}Animations/{0}_Animator", WorldMapCity.CityDefinition.Id));
			RefreshAnimation();
			cityNameText.text = ((WorldMapCity.NumberOfRuns > 0) ? $"{WorldMapCity.CityDefinition.Name} #{WorldMapCity.NumberOfRuns + 1}" : WorldMapCity.CityDefinition.Name);
			cityDifficultyView.RefreshDifficultySkulls(WorldMapCity.CityDefinition);
			if (!WorldMapCity.IsSelectable)
			{
				cityNameCanvas.enabled = false;
			}
			if (WorldMapCity.MaxApocalypsePassed >= 1)
			{
				apocalypseMaxLevelReachedText.text = $"<style=Bad>{WorldMapCity.MaxApocalypsePassed}</style>";
			}
			else
			{
				apocalypseMaxLevelReachedText.enabled = false;
			}
			cityNamePanel.gameObject.SetActive(value: false);
			apocalypseNameText.text = Localizer.Get((WorldMapCity.MaxApocalypsePassed == 0) ? "WorldMap_ApocalypseDifficulty_Normal" : "WorldMap_ApocalypseDifficulty_Apocalypse");
			base.gameObject.SetActive(WorldMapCity.IsVisible && !WorldMapCity.CityDefinition.Hidden);
			ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Combine(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoom));
			GameConfigurationsView instance = TPSingleton<GameConfigurationsView>.Instance;
			instance.OnApocalypseSelectionHasChanged = (GameConfigurationsView.DelApocalypseSelectionChanged)Delegate.Combine(instance.OnApocalypseSelectionHasChanged, new GameConfigurationsView.DelApocalypseSelectionChanged(OnSelectionHasChanged));
			WeaponRestrictionsPanel.OnPanelClosed += OnWeaponRestrictionsPanelClosed;
			InitBuyDLCButton();
			InitStartButton();
		}
	}

	public void RefreshAnimation()
	{
		animator.Play(GetAnimationName(hovered: false), 0, UnityEngine.Random.value);
	}

	public void OnDeselection()
	{
		isSelected = false;
		SwitchCursorActiveState(show: false);
		SwitchMaxApocalypseLevelActiveState(IsHovered && WorldMapCity.MaxApocalypsePassed != -1 && TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0);
		SwitchNamePanelSprite(IsHovered);
		SwitchButtonStartOrBuy(show: false);
		cantStartNewGameText.gameObject.SetActive(value: false);
		animator.Play(GetAnimationName(IsHovered), 0, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
	}

	public void OnSelection()
	{
		isSelected = true;
		SwitchCursorActiveState(show: true);
		SwitchMaxApocalypseLevelActiveState(WorldMapCity.MaxApocalypsePassed != -1 && TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0);
		SwitchNamePanelSprite(show: true);
		SwitchButtonStartOrBuy(show: true, CanStartGame);
		animator.Play(GetAnimationName(hovered: false), 0, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
	}

	private void OnWeaponRestrictionsPanelClosed()
	{
		if (isSelected)
		{
			SwitchButtonStartOrBuy(show: true, CanStartGame);
		}
	}

	private string GetAnimationName(bool hovered)
	{
		if (hovered)
		{
			if (!WorldMapCity.IsUnlocked)
			{
				return "CityLockedHovered";
			}
			if (WorldMapCity.NumberOfWins <= 0)
			{
				return "CityHovered";
			}
			return "CityCompletedHovered";
		}
		if (!WorldMapCity.IsUnlocked)
		{
			return "CityLockedIdle";
		}
		if (WorldMapCity.NumberOfWins <= 0)
		{
			return "CityIdle";
		}
		return "CityCompletedIdle";
	}

	private void HandleMouseHover(bool hovering)
	{
		if (IsHovered != hovering)
		{
			IsHovered = hovering;
			if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity == null && !isSelected)
			{
				animator.Play(GetAnimationName(hovering && TPSingleton<WorldMapCityManager>.Instance.SelectedCity != WorldMapCity), 0, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
				SwitchNamePanelSprite(hovering || TPSingleton<WorldMapCityManager>.Instance.SelectedCity == WorldMapCity);
				SwitchMaxApocalypseLevelActiveState((hovering || TPSingleton<WorldMapCityManager>.Instance.SelectedCity == WorldMapCity) && WorldMapCity.MaxApocalypsePassed != -1 && TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable > 0);
			}
		}
	}

	private void InitBuyDLCButton()
	{
		buyDLCButton.onClick.AddListener(GameConfigurationsView.OpenSelectedCityLinkedDLCStorePage);
		buyDLCButton.interactable = false;
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter
		};
		EventTrigger.Entry entry2 = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit
		};
		entry.callback.AddListener(delegate
		{
			if (buyDLCButton.Interactable)
			{
				missingDLCText.gameObject.SetActive(value: true);
			}
		});
		buyDLCEventTrigger.triggers.Add(entry);
		entry2.callback.AddListener(delegate
		{
			missingDLCText.gameObject.SetActive(value: false);
		});
		buyDLCEventTrigger.triggers.Add(entry2);
	}

	private void InitStartButton()
	{
		startButton.onClick.AddListener(GameConfigurationsView.StartNewGameIfEnoughGlyph);
		startButton.interactable = false;
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter
		};
		EventTrigger.Entry entry2 = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit
		};
		entry.callback.AddListener(delegate
		{
			if (!startButton.Interactable)
			{
				cantStartNewGameText.gameObject.SetActive(value: true);
			}
		});
		startEventTrigger.triggers.Add(entry);
		entry2.callback.AddListener(delegate
		{
			cantStartNewGameText.gameObject.SetActive(value: false);
		});
		startEventTrigger.triggers.Add(entry2);
	}

	private void OnDestroy()
	{
		ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Remove(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoom));
		WeaponRestrictionsPanel.OnPanelClosed -= OnWeaponRestrictionsPanelClosed;
	}

	private void OnMouseDown()
	{
		if (!IsTutorialOpened)
		{
			TPSingleton<WorldMapCityManager>.Instance.SelectCity(WorldMapCity);
		}
	}

	private void OnMouseEnter()
	{
		if (WorldMapCityManager.CanSelectAnyCity && !IsTutorialOpened && WorldMapCity.IsSelectable)
		{
			HandleMouseHover(hovering: true);
		}
	}

	private void OnMouseExit()
	{
		if (WorldMapCity.IsSelectable)
		{
			HandleMouseHover(hovering: false);
		}
	}

	private void OnSelectionHasChanged(bool selected)
	{
		if (isSelected)
		{
			SwitchButtonStartOrBuy(show: true, CanStartGame);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		OnMouseEnter();
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		OnMouseExit();
	}

	private void OnZoom(bool zoomed)
	{
		SwitchSizeZoomStatus(zoomed, zoomPanelCityNameAnimation, panelCityNameRect);
		SwitchScaleZoomStatus(zoomed, zoomPanelDifficultySkullsScaleAnimation, difficultyPanelRect);
		SwitchPosZoomStatus(zoomed, zoomPanelDifficultySkullsPosAnimation, difficultyPanelRect);
		SwitchSizeZoomStatus(zoomed, zoomCityNameAnimation, cityNameRect);
		SwitchSizeZoomStatus(zoomed, zoomWingsAnimation, wingsRect);
		SwitchSizeZoomStatus(zoomed, zoomFlamesSizeAnimation, flamesRect);
		SwitchPosZoomStatus(zoomed, zoomFlamesPosAnimation, flamesRect);
		SwitchSizeZoomStatus(zoomed, zoomApocalypseNameSizeAnimation, apocalypseNameRect);
		SwitchPosZoomStatus(zoomed, zoomApocalypseNamePosAnimation, apocalypseNameRect);
		SwitchSizeZoomStatus(zoomed, zoomApocalypseLevelSizeAnimation, apocalypseLevelRect);
		SwitchPosZoomStatus(zoomed, zoomApocalypseLevelPosAnimation, apocalypseLevelRect);
	}

	private void RefreshCantStartNewGameText()
	{
		bool flag = false;
		bool isUnlocked = WorldMapCity.IsUnlocked;
		bool validApocalypseStateToStart = ValidApocalypseStateToStart;
		if (validApocalypseStateToStart != validApocalypseLastState)
		{
			flag = true;
			validApocalypseLastState = validApocalypseStateToStart;
		}
		bool validWeaponRestrictionsStateToStart = ValidWeaponRestrictionsStateToStart;
		if (validWeaponRestrictionsStateToStart != validWeaponRestrictionsLastState)
		{
			flag = true;
			validWeaponRestrictionsLastState = validWeaponRestrictionsStateToStart;
		}
		if (WorldMapCity.IsUnlocked != unlockedLastState)
		{
			flag = true;
			unlockedLastState = WorldMapCity.IsUnlocked;
		}
		if (!(!flag && isUnlocked))
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (!validApocalypseStateToStart && isUnlocked)
			{
				stringBuilder.Append(Localizer.Get("WorldMap_CantStartNewGame")).AppendLine();
			}
			if (!validWeaponRestrictionsStateToStart && isUnlocked)
			{
				stringBuilder.Append(Localizer.Get("WorldMap_CantStartNewGame_WeaponRestrictions")).AppendLine();
			}
			if (!isUnlocked)
			{
				stringBuilder.Append(WorldMapCity.GetLockedCityText());
			}
			cantStartNewGameText.text = stringBuilder.ToString();
		}
	}

	private void RefreshMissingDLCText()
	{
		if (!WorldMapCity.IsUnlocked && WorldMapCity.CityDefinition.HasLinkedDLC && !WorldMapCity.IsLinkedDLCOwned)
		{
			DLCDefinition dLCFromId = TPSingleton<DLCManager>.Instance.GetDLCFromId(WorldMapCity.CityDefinition.LinkedDLCId);
			if (dLCFromId != null)
			{
				missingDLCText.text = dLCFromId.LocalizedName;
			}
		}
	}

	private void SwitchButtonStartOrBuy(bool show, bool interactable = false)
	{
		if (!show)
		{
			startButton.gameObject.SetActive(value: false);
			buyDLCButton.gameObject.SetActive(value: false);
			return;
		}
		if (WorldMapCity.CityDefinition.IsStoryMap)
		{
			startButton.gameObject.SetActive(value: true);
			startButton.Interactable = interactable;
			buyDLCButton.gameObject.SetActive(value: false);
		}
		else
		{
			buyDLCButton.gameObject.SetActive(value: false);
			startButton.gameObject.SetActive(value: false);
			if (WorldMapCity.CityDefinition.HasLinkedDLC && !WorldMapCity.IsLinkedDLCOwned)
			{
				buyDLCButton.gameObject.SetActive(value: true);
				buyDLCButton.Interactable = true;
			}
			else
			{
				startButton.gameObject.SetActive(value: true);
				startButton.Interactable = interactable;
			}
			if (buyDLCButton.gameObject.activeSelf)
			{
				RefreshMissingDLCText();
			}
		}
		if (startButton.gameObject.activeSelf && !interactable)
		{
			RefreshCantStartNewGameText();
		}
	}

	private void SwitchCursorActiveState(bool show)
	{
		selectionCursor.SetActive(show);
	}

	private void SwitchMaxApocalypseLevelActiveState(bool show)
	{
		apocalypseFlameAnimator.gameObject.SetActive(show);
		apocalypseMaxLevelReachedText.gameObject.SetActive(show);
		apocalypseNameText.gameObject.SetActive(show);
		if (show)
		{
			apocalypseFlameAnimator.Play("WorldMapFlamesIdle", 0, UnityEngine.Random.value);
		}
	}

	private void SwitchNamePanelSprite(bool show)
	{
		cityNamePanel.gameObject.SetActive(IsHovered || isSelected);
		cityNamePanel.sprite = (show ? cityNamePanelHovered : cityNamePanelNormal);
	}

	private void SwitchScaleZoomStatus(bool zoomed, Vector2TweenAnimation vector2Animation, RectTransform rectTransform)
	{
		if (zoomed == vector2Animation.InStatusOne)
		{
			vector2Animation.StatusTransitionTween?.Kill();
			Vector2 vector = (vector2Animation.InStatusOne ? vector2Animation.StatusTwo : vector2Animation.StatusOne);
			vector2Animation.StatusTransitionTween = rectTransform.DOScale(vector, vector2Animation.TransitionDuration).SetEase(vector2Animation.TransitionEase);
			vector2Animation.InStatusOne = !vector2Animation.InStatusOne;
		}
	}

	private void SwitchSizeZoomStatus(bool zoomed, Vector2TweenAnimation vector2Animation, RectTransform rectTransform)
	{
		if (zoomed == vector2Animation.InStatusOne)
		{
			vector2Animation.StatusTransitionTween?.Kill();
			Vector2 endValue = (vector2Animation.InStatusOne ? vector2Animation.StatusTwo : vector2Animation.StatusOne);
			vector2Animation.StatusTransitionTween = rectTransform.DOSizeDelta(endValue, vector2Animation.TransitionDuration).SetEase(vector2Animation.TransitionEase);
			vector2Animation.InStatusOne = !vector2Animation.InStatusOne;
		}
	}

	private void SwitchPosZoomStatus(bool zoomed, Vector2TweenAnimation vector2Animation, RectTransform rectTransform)
	{
		if (zoomed == vector2Animation.InStatusOne)
		{
			vector2Animation.StatusTransitionTween?.Kill();
			Vector2 endValue = (vector2Animation.InStatusOne ? vector2Animation.StatusTwo : vector2Animation.StatusOne);
			vector2Animation.StatusTransitionTween = rectTransform.DOAnchorPos(endValue, vector2Animation.TransitionDuration).SetEase(vector2Animation.TransitionEase);
			vector2Animation.InStatusOne = !vector2Animation.InStatusOne;
		}
	}
}
