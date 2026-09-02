using System.IO;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model.Animation;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class UnitLevelUpStatView : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	private static class Constants
	{
		public static readonly string LevelUpRarityBackgroundPath = Path.Combine("View", "Sprites", "UI", "LevelUp", "CharacterSheet_LevelUp_Box{0}_Off");

		public static readonly string LevelUpRarityBackgroundPathSelected = Path.Combine("View", "Sprites", "UI", "LevelUp", "CharacterSheet_LevelUp_Box{0}_On");

		public static readonly string[] LevelUpRarityLevelNames = new string[4] { "Common", "Magic", "Rare", "Epic" };
	}

	[SerializeField]
	private Image bonusIcon;

	[SerializeField]
	private Image bonusJewel;

	[SerializeField]
	private TextMeshProUGUI bonusJewelName;

	[SerializeField]
	private DataSpriteTable bonusJewelSprites;

	[SerializeField]
	private CanvasGroup statBoxCanvasGroup;

	[SerializeField]
	private ImprovedToggle statBoxToggle;

	[SerializeField]
	private Animator statBoxValidationAnimator;

	[SerializeField]
	private CanvasGroup statBoxValidationCanvasGroup;

	[SerializeField]
	private Image statArrowImage;

	[SerializeField]
	private Image statBoxBG;

	[SerializeField]
	private UnitStatDisplay statDisplay;

	[SerializeField]
	private StatTooltipDisplayer statTooltipDisplayer;

	[SerializeField]
	private TextMeshProUGUI statResultText;

	[SerializeField]
	[Range(0f, 1f)]
	private float unselectedAlpha = 0.5f;

	[SerializeField]
	private CanvasGroup selectedCanvasGroup;

	[SerializeField]
	private RectTransform confirmBox;

	[SerializeField]
	private Image confirmBoxImage;

	[SerializeField]
	private CanvasGroup confirmBoxCanvasGroup;

	[SerializeField]
	private Sprite confirmBoxOffSprite;

	[SerializeField]
	private Sprite confirmBoxOnSprite;

	[SerializeField]
	private EventTrigger confirmBoxEventTrigger;

	[SerializeField]
	private TextMeshProUGUI confirmBoxText;

	[SerializeField]
	private Vector2TweenAnimation confirmBoxAnimationDatas = new Vector2TweenAnimation();

	[SerializeField]
	private ColorTweenAnimation confirmBoxTextAnimationDatas = new ColorTweenAnimation();

	[SerializeField]
	private Selectable selectable;

	private Sprite background;

	private Sprite highlightedBackground;

	public Selectable Selectable => selectable;

	public UnityEvent OnConfirmBoxClicked { get; } = new UnityEvent();

	public UnitLevelUp.SelectedStatToLevelUp StatBonus { get; set; }

	public ImprovedToggle StatBoxToggle => statBoxToggle;

	public StatTooltipDisplayer StatTooltipDisplayer => statTooltipDisplayer;

	public PlayableUnit TargetUnit { get; set; }

	public void InitializeToggle()
	{
		statBoxToggle.OnPointerClickEvent.AddListener(delegate
		{
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonClickAudioClip);
		});
		statBoxToggle.OnPointerEnterEvent.AddListener(delegate
		{
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonHoverAudioClip);
			statBoxBG.sprite = highlightedBackground;
			OnPointerEnterOverConfirmBox();
		});
		statBoxToggle.OnPointerExitEvent.AddListener(delegate
		{
			OnPointerExitOverConfirmBox();
			statBoxBG.sprite = background;
		});
		EventTrigger.Entry entry = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter
		};
		entry.callback.AddListener(delegate
		{
			statBoxToggle.OnPointerExitEvent?.Invoke();
		});
		confirmBoxEventTrigger.triggers.Add(entry);
		EventTrigger.Entry entry2 = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit
		};
		entry2.callback.AddListener(delegate
		{
			statBoxToggle.OnPointerExitEvent?.Invoke();
		});
		confirmBoxEventTrigger.triggers.Add(entry2);
		EventTrigger.Entry entry3 = new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerDown
		};
		entry3.callback.AddListener(delegate
		{
			OnConfirmBoxClicked?.Invoke();
		});
		confirmBoxEventTrigger.triggers.Add(entry3);
	}

	private void OnPointerExitOverConfirmBox()
	{
		confirmBoxTextAnimationDatas.StatusTransitionTween?.Kill();
		confirmBoxTextAnimationDatas.StatusTransitionTween = confirmBoxText.DOColor(confirmBoxTextAnimationDatas.StatusOne, confirmBoxTextAnimationDatas.TransitionDuration).SetEase(confirmBoxTextAnimationDatas.TransitionEase);
		confirmBoxImage.sprite = confirmBoxOffSprite;
	}

	private void OnPointerEnterOverConfirmBox()
	{
		confirmBoxTextAnimationDatas.StatusTransitionTween?.Kill();
		confirmBoxTextAnimationDatas.StatusTransitionTween = confirmBoxText.DOColor(confirmBoxTextAnimationDatas.StatusTwo, confirmBoxTextAnimationDatas.TransitionDuration).SetEase(confirmBoxTextAnimationDatas.TransitionEase);
		confirmBoxImage.sprite = confirmBoxOnSprite;
	}

	public void Refresh()
	{
		int rarityLevel = (int)StatBonus.RarityLevel;
		statBoxCanvasGroup.alpha = 0f;
		statBoxCanvasGroup.DOFade(1f, 0.5f);
		statDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[StatBonus.Definition.Stat];
		statDisplay.TargetUnit = TargetUnit;
		statDisplay.Refresh();
		statBoxValidationAnimator.enabled = false;
		statBoxValidationCanvasGroup.alpha = 0f;
		bonusIcon.sprite = UnitStatDisplay.GetStatIconSprite(UnitDatabase.UnitStatDefinitions[StatBonus.Definition.Stat].Id, UnitStatDisplay.E_IconSize.VerySmall);
		background = GetRarityBackgroundSprite(rarityLevel);
		highlightedBackground = GetRarityBackgroundSprite(rarityLevel, isHovered: true);
		statBoxBG.sprite = background;
		SpriteState spriteState = statBoxToggle.spriteState;
		spriteState.highlightedSprite = GetRarityBackgroundSprite(rarityLevel, isHovered: true);
		statBoxToggle.spriteState = spriteState;
		bonusJewel.sprite = bonusJewelSprites.GetSpriteAt(rarityLevel);
		bonusJewelName.text = Localizer.Get("RarityName_" + Constants.LevelUpRarityLevelNames[rarityLevel]);
		statResultText.text = string.Format("{0}{1}", Mathf.Round(statDisplay.TargetUnit.UnitStatsController.GetStat(StatBonus.Definition.Stat).Base + (float)StatBonus.Definition.Bonuses[StatBonus.BonusIndex]), statDisplay.StatDefinition.Id.ShownAsPercentage() ? "<size=80%>%</size>" : string.Empty);
	}

	public void Select(bool isSelected, bool isAnythingSelected = false)
	{
		selectedCanvasGroup.DOFade(isSelected ? 1f : 0f, 0f);
		confirmBoxCanvasGroup.DOFade(isSelected ? 1f : 0f, 0.1f);
		confirmBoxCanvasGroup.interactable = isSelected;
		confirmBoxCanvasGroup.blocksRaycasts = isSelected;
		if (!isSelected)
		{
			confirmBoxAnimationDatas.StatusTransitionTween.Complete();
			if (!confirmBoxAnimationDatas.InStatusOne)
			{
				confirmBoxAnimationDatas.StatusTransitionTween = confirmBox.DOAnchorPos(confirmBoxAnimationDatas.StatusOne, confirmBoxAnimationDatas.TransitionDuration).SetEase(confirmBoxAnimationDatas.TransitionEase).OnComplete(delegate
				{
					confirmBoxAnimationDatas.InStatusOne = true;
				});
			}
		}
		else
		{
			confirmBoxAnimationDatas.StatusTransitionTween.Complete();
			if (confirmBoxAnimationDatas.InStatusOne)
			{
				confirmBoxAnimationDatas.StatusTransitionTween = confirmBox.DOAnchorPos(confirmBoxAnimationDatas.StatusTwo, confirmBoxAnimationDatas.TransitionDuration).SetEase(confirmBoxAnimationDatas.TransitionEase).OnComplete(delegate
				{
					confirmBoxAnimationDatas.InStatusOne = false;
				});
			}
		}
		if (isAnythingSelected && !isSelected)
		{
			statBoxCanvasGroup.DOFade(unselectedAlpha, 0.25f).SetEase(Ease.OutCubic);
		}
		else
		{
			statBoxCanvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutCubic);
		}
	}

	public void Validate(bool isChosen)
	{
		selectedCanvasGroup.DOFade(0f, 0f);
		if (isChosen)
		{
			if (InputManager.IsLastControllerJoystick)
			{
				Selectable.ClearNavigation();
			}
			statArrowImage.DOFade(0f, 0.5f).SetEase(Ease.OutCubic);
			statResultText.DOColor(Color.white, 1f);
			statDisplay.StatValueText.DOColor(Color.grey.WithA(0f), 1f);
			statResultText.transform.DOPunchScale(Vector3.one * 0.3f, 0.2f, 0, 0.1f);
			statBoxValidationAnimator.enabled = true;
			statBoxValidationCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic);
		}
		else
		{
			statBoxCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutCubic);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		statBoxToggle.OnPointerEnterEvent?.Invoke();
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			StatTooltipDisplayer.DisplayTooltip(display: true);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		statBoxToggle.OnPointerExitEvent?.Invoke();
		StatTooltipDisplayer.DisplayTooltip(display: false);
	}

	private Sprite GetRarityBackgroundSprite(int rarityLevel, bool isHovered = false)
	{
		return ResourcePooler<Sprite>.LoadOnce(string.Format(isHovered ? Constants.LevelUpRarityBackgroundPathSelected : Constants.LevelUpRarityBackgroundPath, (rarityLevel + 1).ToString("00")));
	}

	private void OnDestroy()
	{
		statBoxToggle.OnPointerClickEvent.RemoveAllListeners();
		statBoxToggle.OnPointerEnterEvent.RemoveAllListeners();
		StatBoxToggle.onValueChanged.RemoveAllListeners();
		statBoxToggle.OnPointerExitEvent.RemoveAllListeners();
		statBoxToggle.OnBeforePointerClickEvent.RemoveAllListeners();
		confirmBoxEventTrigger.triggers.ForEach(delegate(EventTrigger.Entry x)
		{
			x.callback.RemoveAllListeners();
		});
		confirmBoxEventTrigger.triggers.Clear();
		OnConfirmBoxClicked.RemoveAllListeners();
	}
}
