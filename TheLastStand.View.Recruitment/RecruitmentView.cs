using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit.Perk;
using TheLastStand.View.Unit.Race;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.Trait;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Recruitment;

public class RecruitmentView : TPSingleton<RecruitmentView>, IOverlayUser
{
	private static class Constants
	{
		public const string RerollAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Inn";

		public const string AmbienceAudioLoopsFolderPath = "Sounds/SFX/Ambient/AMB_Tavern";
	}

	[SerializeField]
	private DataColor defaultButtonColor;

	[SerializeField]
	private DataColor unableButtonColor;

	[SerializeField]
	private SimpleFontLocalizedParent simpleLocalizedFontParent;

	[SerializeField]
	private ToggleGroup unitRosterParent;

	[SerializeField]
	private RecruitmentUnitDisplay recruitmentUnitDisplayPrefab;

	[SerializeField]
	private RecruitmentMageDisplay recruitmentMageDisplay;

	[SerializeField]
	private BetterButton rerollButton;

	[SerializeField]
	private TextMeshProUGUI rerollCost;

	[SerializeField]
	private Canvas unitInnCanvas;

	[SerializeField]
	private Image unitInnSprite;

	[SerializeField]
	private Image mageInnImage;

	[SerializeField]
	private TextMeshProUGUI unitCountText;

	[SerializeField]
	private TextMeshProUGUI mageCountText;

	[SerializeField]
	private GameObject unitDetailsParent;

	[SerializeField]
	private Image unitToDetailSprite;

	[SerializeField]
	private TextMeshProUGUI unitName;

	[SerializeField]
	private UnitLevelDisplay unitLevel;

	[SerializeField]
	private UnitRaceDisplay unitRaceDisplay;

	[SerializeField]
	private UnitTraitDisplay[] traits = new UnitTraitDisplay[3];

	[SerializeField]
	private Image perkCollectionLeft;

	[SerializeField]
	private Image perkCollectionRight;

	[SerializeField]
	private UnitStatDisplay healthStatDisplay;

	[SerializeField]
	private UnitStatDisplay healthRegenStatDisplay;

	[SerializeField]
	private UnitStatDisplay manaStatDisplay;

	[SerializeField]
	private UnitStatDisplay manaRegenStatDisplay;

	[SerializeField]
	private UnitStatDisplay actionPointsStatDisplay;

	[SerializeField]
	private UnitStatDisplay movementPointsDisplay;

	[SerializeField]
	private UnitStatDisplay overallDamageStatDisplay;

	[SerializeField]
	private UnitStatDisplay criticalStatDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceReductionStatDisplay;

	[SerializeField]
	private UnitStatDisplay accuracyStatDisplay;

	[SerializeField]
	private UnitStatDisplay blockStatDisplay;

	[SerializeField]
	private UnitStatDisplay armorStatDisplay;

	[SerializeField]
	private UnitStatDisplay resistanceStatDisplay;

	[SerializeField]
	private UnitStatDisplay dodgeStatDisplay;

	[SerializeField]
	private SkillListDisplay equippedSkillsDisplay;

	[SerializeField]
	private GameObject mageDetailsParent;

	[SerializeField]
	private RecruitmentButton recruitButton;

	[SerializeField]
	private TextMeshProUGUI recruitCost;

	[SerializeField]
	private GenericTooltip warningTooltip;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	[SerializeField]
	private AudioClip[] openClips;

	[SerializeField]
	private AudioClip closeClip;

	[SerializeField]
	private AudioSource ambienceAudioSource;

	[SerializeField]
	private float ambientSoundsFadeInDuration = 1f;

	[SerializeField]
	private float ambientSoundsFadeOutDuration = 1f;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationIn;

	[SerializeField]
	[Min(0f)]
	private float snapshotTransitionDurationOut;

	public bool UnitPlacingDoneThisFrame;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private RectTransform rectTransform;

	private Dictionary<Toggle, RecruitmentUnitDisplay> recruitmentUnitDisplays = new Dictionary<Toggle, RecruitmentUnitDisplay>();

	private Toggle selectedUnit;

	private UnitStatDisplay[] statDisplays;

	private RecruitmentController.E_ImpossibleRecruitmentReason impossibleHeroRecruitmentReason;

	private RecruitmentController.E_ImpossibleRecruitmentReason impossibleMageRecruitmentReason;

	private bool isOpened;

	private float posYInit;

	private Tween moveTween;

	private Tween ambientSoundsFadeTween;

	private AudioClip[] ambienceAudioLoops;

	private AudioClip[] rerollAudioClips;

	public static bool HasUnitSelected => !TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Toggle.isOn;

	public static GenericTooltip WarningTooltip => TPSingleton<RecruitmentView>.Instance.warningTooltip;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public bool IsGeneratingNewRoster { get; set; }

	public static void Refresh()
	{
		TPSingleton<RecruitmentView>.Instance.rerollButton.Interactable = TPSingleton<ResourceManager>.Instance.Gold >= RecruitmentController.ComputeRerollCost();
		TPSingleton<RecruitmentView>.Instance.rerollCost.text = $"{RecruitmentController.ComputeRerollCost()}";
		TPSingleton<RecruitmentView>.Instance.rerollCost.color = ((TPSingleton<ResourceManager>.Instance.Gold >= RecruitmentController.ComputeRerollCost()) ? TPSingleton<RecruitmentView>.Instance.defaultButtonColor._Color : TPSingleton<RecruitmentView>.Instance.unableButtonColor._Color);
		int num = 0;
		foreach (KeyValuePair<Toggle, RecruitmentUnitDisplay> recruitmentUnitDisplay in TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays)
		{
			recruitmentUnitDisplay.Value.Refresh(TPSingleton<PlayableUnitManager>.Instance.Recruitment.CurrentGeneratedUnits[num]);
			if (TPSingleton<RecruitmentView>.Instance.selectedUnit == null && recruitmentUnitDisplay.Value.gameObject.activeSelf)
			{
				recruitmentUnitDisplay.Key.isOn = true;
			}
			num++;
		}
		if (TPSingleton<RecruitmentView>.Instance.selectedUnit == null && TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage)
		{
			TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Toggle.isOn = true;
		}
		bool num2 = TPSingleton<RecruitmentView>.Instance.selectedUnit != null && TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays.ContainsKey(TPSingleton<RecruitmentView>.Instance.selectedUnit);
		RefreshUnitDetails();
		RefreshMageDetails();
		TPSingleton<RecruitmentView>.Instance.unitCountText.text = $"{TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count}/{TPSingleton<PlayableUnitManager>.Instance.Recruitment.UnitsLimit}";
		TPSingleton<RecruitmentView>.Instance.mageCountText.text = $"{BuildingManager.MagicCircle.MageCount}/{BuildingManager.MagicCircle.MageSlots}";
		TPSingleton<RecruitmentView>.Instance.unitInnCanvas.enabled = TPSingleton<RecruitmentView>.Instance.selectedUnit != null && TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays.ContainsKey(TPSingleton<RecruitmentView>.Instance.selectedUnit);
		if (TPSingleton<RecruitmentView>.Instance.selectedUnit != null && TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays.ContainsKey(TPSingleton<RecruitmentView>.Instance.selectedUnit))
		{
			TPSingleton<RecruitmentView>.Instance.unitInnSprite.sprite = TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.UiSprite;
		}
		TPSingleton<RecruitmentView>.Instance.mageInnImage.enabled = TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage;
		bool flag = num2 && RecruitmentController.CanRecruitUnit(TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit, out TPSingleton<RecruitmentView>.Instance.impossibleHeroRecruitmentReason);
		bool flag2 = RecruitmentController.CanRecruitMage(out TPSingleton<RecruitmentView>.Instance.impossibleMageRecruitmentReason);
		TPSingleton<RecruitmentView>.Instance.recruitButton.gameObject.SetActive(TPSingleton<RecruitmentView>.Instance.selectedUnit != null);
		TPSingleton<RecruitmentView>.Instance.recruitButton.Button.Interactable = flag2 || (TPSingleton<RecruitmentView>.Instance.selectedUnit != null && TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays.ContainsKey(TPSingleton<RecruitmentView>.Instance.selectedUnit) && flag);
		TPSingleton<RecruitmentView>.Instance.recruitCost.color = (TPSingleton<RecruitmentView>.Instance.recruitButton.Button.Interactable ? TPSingleton<RecruitmentView>.Instance.defaultButtonColor._Color : TPSingleton<RecruitmentView>.Instance.unableButtonColor._Color);
		TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Refresh(TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage);
	}

	public RecruitmentController.E_ImpossibleRecruitmentReason ComputeImpossibleRecruitmentReason()
	{
		if (TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Toggle.isOn)
		{
			return TPSingleton<RecruitmentView>.Instance.impossibleMageRecruitmentReason;
		}
		return TPSingleton<RecruitmentView>.Instance.impossibleHeroRecruitmentReason;
	}

	public static void Unselect()
	{
		TPSingleton<RecruitmentView>.Instance.unitRosterParent.SetAllTogglesOff();
	}

	public void Close()
	{
		if (isOpened)
		{
			CLoggerManager.Log("RecruitmentView closed", this, LogType.Log, CLogLevel.DETAILED);
			CameraView.AttenuateWorldForPopupFocus(null);
			SoundManager.PlayAudioClip(closeClip);
			TPSingleton<SoundManager>.Instance.TransitionToNormalSnapshot(snapshotTransitionDurationOut);
			SoundManager.FadeOutAudioSource(ambienceAudioSource, ref ambientSoundsFadeTween, ambientSoundsFadeOutDuration);
			if (moveTween != null)
			{
				moveTween.Kill();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			}
			isOpened = false;
			moveTween = rectTransform.DOAnchorPosY(posYInit, 0.25f).SetEase(Ease.InBack).OnComplete(delegate
			{
				canvas.enabled = false;
			});
			canvasGroup.blocksRaycasts = false;
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OnPopupExitToWorld();
				recruitButton.DisableWarningTooltip();
			}
		}
	}

	public void Open()
	{
		if (!isOpened)
		{
			CLoggerManager.Log("RecruitmentView opened", this, LogType.Log, CLogLevel.DETAILED);
			CameraView.AttenuateWorldForPopupFocus(this);
			SoundManager.PlayAudioClip(openClips.PickRandom());
			TPSingleton<SoundManager>.Instance.TransitionToInnSnapshot(snapshotTransitionDurationIn);
			SoundManager.PlayFadeInAudioClip(ambienceAudioSource, ref ambientSoundsFadeTween, ambienceAudioLoops.PickRandom(), ambientSoundsFadeInDuration);
			if (moveTween != null)
			{
				moveTween.Kill();
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posYInit);
			}
			simpleLocalizedFontParent?.RefreshChildren();
			isOpened = true;
			Refresh();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
			moveTween = rectTransform.DOAnchorPosY(0f, 0.25f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			});
			canvas.enabled = true;
			canvasGroup.blocksRaycasts = true;
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
			}
		}
	}

	public void OnCloseButtonClick()
	{
		RecruitmentController.CloseRecruitmentPanel();
	}

	public void OnRerollClick()
	{
		SoundManager.PlayAudioClip(rerollAudioClips.PickRandom());
		RecruitmentController.BuyRerollRoster();
	}

	public void OnRecruitClick()
	{
		if (selectedUnit == null)
		{
			return;
		}
		if (recruitmentUnitDisplays.ContainsKey(selectedUnit))
		{
			RecruitmentController.PlaceNewUnit(TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[selectedUnit].Unit);
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			}
		}
		else if (selectedUnit == recruitmentMageDisplay.Toggle)
		{
			RecruitmentController.RecruitMage();
		}
	}

	private void TabbedPageToggle_ValueChanged(Toggle sender, bool value)
	{
		if (value)
		{
			foreach (KeyValuePair<Toggle, RecruitmentUnitDisplay> recruitmentUnitDisplay in recruitmentUnitDisplays)
			{
				recruitmentUnitDisplay.Key.interactable = recruitmentUnitDisplay.Key != sender;
			}
			selectedUnit = sender;
			Refresh();
		}
		else if (!unitRosterParent.AnyTogglesOn())
		{
			selectedUnit = null;
			Refresh();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		canvas = TPSingleton<RecruitmentView>.Instance.GetComponent<Canvas>();
		canvas.enabled = false;
		canvasGroup = TPSingleton<RecruitmentView>.Instance.GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		TransformExtensions.DestroyChildren(unitRosterParent.transform);
		isOpened = false;
		rectTransform = GetComponent<RectTransform>();
		posYInit = rectTransform.anchoredPosition.y;
		TPSingleton<RecruitmentView>.Instance.rerollAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Inn");
		TPSingleton<RecruitmentView>.Instance.ambienceAudioLoops = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/Ambient/AMB_Tavern");
		healthStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		healthStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
		healthRegenStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthRegen];
		manaStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Mana];
		manaStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaTotal];
		manaRegenStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ManaRegen];
		actionPointsStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
		actionPointsStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPointsTotal];
		movementPointsDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
		movementPointsDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePointsTotal];
		overallDamageStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.OverallDamage];
		criticalStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Critical];
		resistanceReductionStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ResistanceReduction];
		accuracyStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Accuracy];
		blockStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Block];
		resistanceStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Resistance];
		armorStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Armor];
		armorStatDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ArmorTotal];
		dodgeStatDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Dodge];
		unitRosterParent.RegisterToggle(recruitmentMageDisplay.Toggle);
		recruitmentMageDisplay.Toggle.group = unitRosterParent;
		recruitmentMageDisplay.Toggle.onValueChanged.AddListener(delegate(bool value)
		{
			TabbedPageToggle_ValueChanged(recruitmentMageDisplay.Toggle, value);
		});
		statDisplays = new UnitStatDisplay[14]
		{
			healthStatDisplay, healthRegenStatDisplay, manaStatDisplay, manaRegenStatDisplay, actionPointsStatDisplay, movementPointsDisplay, overallDamageStatDisplay, criticalStatDisplay, resistanceReductionStatDisplay, accuracyStatDisplay,
			blockStatDisplay, resistanceStatDisplay, armorStatDisplay, dodgeStatDisplay
		};
		for (int num = 0; num < PlayableUnitDatabase.RecruitmentDefinition.UnitsToGenerate.Count; num++)
		{
			RecruitmentUnitDisplay recruitmentUnitDisplay = Object.Instantiate(recruitmentUnitDisplayPrefab, unitRosterParent.transform);
			recruitmentUnitDisplays.Add(recruitmentUnitDisplay.Toggle, recruitmentUnitDisplay);
			unitRosterParent.RegisterToggle(recruitmentUnitDisplay.Toggle);
			recruitmentUnitDisplay.Toggle.group = unitRosterParent;
			recruitmentUnitDisplay.Toggle.onValueChanged.AddListener(delegate(bool value)
			{
				TabbedPageToggle_ValueChanged(recruitmentUnitDisplay.Toggle, value);
			});
			simpleLocalizedFontParent.AddChilds(recruitmentUnitDisplay.LocalizedFonts);
			recruitmentUnitDisplay.HidePreviousUnitInput = num == 0;
		}
		TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Refresh(TPSingleton<PlayableUnitManager>.Instance.Recruitment.HasMage);
	}

	private static void RefreshMageDetails()
	{
		TPSingleton<RecruitmentView>.Instance.mageDetailsParent.SetActive(TPSingleton<RecruitmentView>.Instance.selectedUnit != null && TPSingleton<RecruitmentView>.Instance.selectedUnit == TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Toggle);
		if (TPSingleton<RecruitmentView>.Instance.selectedUnit != null && TPSingleton<RecruitmentView>.Instance.selectedUnit == TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Toggle)
		{
			TPSingleton<RecruitmentView>.Instance.recruitCost.text = $"{RecruitmentController.ComputeMageCost()}";
			TPSingleton<RecruitmentView>.Instance.recruitCost.color = ((TPSingleton<ResourceManager>.Instance.Gold >= RecruitmentController.ComputeMageCost()) ? Color.white : Color.red);
		}
	}

	private static void RefreshUnitDetails()
	{
		if (TPSingleton<RecruitmentView>.Instance.selectedUnit == null || !TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays.ContainsKey(TPSingleton<RecruitmentView>.Instance.selectedUnit))
		{
			TPSingleton<RecruitmentView>.Instance.unitDetailsParent.SetActive(value: false);
			return;
		}
		if (TPSingleton<RecruitmentView>.Instance != null && !TPSingleton<RecruitmentView>.Instance.unitDetailsParent.activeInHierarchy)
		{
			TPSingleton<RecruitmentView>.Instance.unitDetailsParent.SetActive(value: true);
		}
		int num = RecruitmentController.ComputeUnitCost(TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit);
		TPSingleton<RecruitmentView>.Instance.recruitCost.text = $"{num}";
		TPSingleton<RecruitmentView>.Instance.recruitCost.color = ((TPSingleton<ResourceManager>.Instance.Gold >= num) ? Color.white : Color.red);
		TPSingleton<RecruitmentView>.Instance.unitToDetailSprite.sprite = TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.UiSprite;
		TPSingleton<RecruitmentView>.Instance.unitName.text = TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.Name;
		TPSingleton<RecruitmentView>.Instance.unitLevel.PlayableUnit = TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit;
		TPSingleton<RecruitmentView>.Instance.unitLevel.Refresh();
		TPSingleton<RecruitmentView>.Instance.unitRaceDisplay.SetContent(TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.RaceDefinition, TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit);
		TPSingleton<RecruitmentView>.Instance.unitRaceDisplay.Refresh();
		HashSet<int> lockedPerkCollectionSlots = TPSingleton<MetaUpgradesManager>.Instance.GetLockedPerkCollectionSlots();
		if (!lockedPerkCollectionSlots.Contains(4) && TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.PerkTree.UnitPerkCollectionIds.TryGetAtIndex(3, out var value))
		{
			TPSingleton<RecruitmentView>.Instance.perkCollectionLeft.sprite = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>("View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_Off", value);
			TPSingleton<RecruitmentView>.Instance.perkCollectionLeft.enabled = true;
		}
		else
		{
			TPSingleton<RecruitmentView>.Instance.perkCollectionLeft.enabled = false;
		}
		if (!lockedPerkCollectionSlots.Contains(5) && TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.PerkTree.UnitPerkCollectionIds.TryGetAtIndex(4, out var value2))
		{
			TPSingleton<RecruitmentView>.Instance.perkCollectionRight.sprite = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>("View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_Off", value2);
			TPSingleton<RecruitmentView>.Instance.perkCollectionRight.enabled = true;
		}
		else
		{
			TPSingleton<RecruitmentView>.Instance.perkCollectionRight.enabled = false;
		}
		for (int i = 0; i < TPSingleton<RecruitmentView>.Instance.statDisplays.Length; i++)
		{
			TPSingleton<RecruitmentView>.Instance.statDisplays[i].TargetUnit = TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit;
			TPSingleton<RecruitmentView>.Instance.statDisplays[i].Refresh(forceFullRefresh: true);
		}
		List<TheLastStand.Model.Skill.Skill> skills = TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.PlayableUnitController.GetSkills(avoidContextualSkills: true);
		TPSingleton<RecruitmentView>.Instance.equippedSkillsDisplay.SetSkills(skills, TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit);
		for (int j = 0; j < TPSingleton<RecruitmentView>.Instance.traits.Length; j++)
		{
			TPSingleton<RecruitmentView>.Instance.traits[j].TargetUnit = ((j < TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.UnitTraitDefinitions.Count) ? TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit : null);
			TPSingleton<RecruitmentView>.Instance.traits[j].UnitTraitDefinition = ((j < TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.UnitTraitDefinitions.Count) ? TPSingleton<RecruitmentView>.Instance.recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit.UnitTraitDefinitions[j] : null);
			TPSingleton<RecruitmentView>.Instance.traits[j].Refresh();
		}
	}

	private void SelectNextRecruit()
	{
		if (recruitmentMageDisplay.Toggle.isOn)
		{
			return;
		}
		for (int i = 0; i < recruitmentUnitDisplays.Count; i++)
		{
			if (!recruitmentUnitDisplays.ElementAt(i).Key.isOn)
			{
				continue;
			}
			bool flag = false;
			if (i < recruitmentUnitDisplays.Count - 1)
			{
				for (int j = i + 1; j < recruitmentUnitDisplays.Count; j++)
				{
					if (recruitmentUnitDisplays.ElementAt(j).Value.gameObject.activeSelf)
					{
						recruitmentUnitDisplays.ElementAt(j).Key.isOn = true;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				recruitmentMageDisplay.Toggle.isOn = true;
			}
			break;
		}
	}

	private void SelectPreviousRecruit()
	{
		if (recruitmentMageDisplay.Toggle.isOn)
		{
			recruitmentUnitDisplays.Last((KeyValuePair<Toggle, RecruitmentUnitDisplay> o) => o.Value.gameObject.activeSelf).Key.isOn = true;
			if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips && ComputeImpossibleRecruitmentReason() != RecruitmentController.E_ImpossibleRecruitmentReason.None)
			{
				recruitButton.OnPointerEnter(null);
			}
			return;
		}
		for (int num = recruitmentUnitDisplays.Count - 1; num >= 1; num--)
		{
			if (recruitmentUnitDisplays.ElementAt(num).Key.isOn)
			{
				bool flag = false;
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					if (recruitmentUnitDisplays.ElementAt(num2).Value.gameObject.activeSelf)
					{
						recruitmentUnitDisplays.ElementAt(num2).Key.isOn = true;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
	}

	public void SelectRecruitButton()
	{
		if (EventSystem.current.currentSelectedGameObject == recruitButton.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
		EventSystem.current.SetSelectedGameObject(recruitButton.gameObject);
	}

	private void OnNewUnitJoystickSelected()
	{
		Refresh();
		if (PlayableUnitManager.TraitTooltip.Displayed)
		{
			if (TPSingleton<RecruitmentView>.Instance.selectedUnit == TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Toggle)
			{
				PlayableUnitManager.TraitTooltip.Hide();
			}
			else
			{
				for (int i = 0; i < traits.Length; i++)
				{
					if (EventSystem.current.currentSelectedGameObject == traits[i].gameObject)
					{
						PlayableUnitManager.TraitTooltip.SetContent(traits[i].UnitTraitDefinition);
						PlayableUnitManager.TraitTooltip.Refresh();
						break;
					}
				}
			}
		}
		if (PlayableUnitManager.StatTooltip.Displayed)
		{
			if (TPSingleton<RecruitmentView>.Instance.selectedUnit == TPSingleton<RecruitmentView>.Instance.recruitmentMageDisplay.Toggle)
			{
				PlayableUnitManager.StatTooltip.Hide();
			}
			else
			{
				for (int j = 0; j < statDisplays.Length; j++)
				{
					if (EventSystem.current.currentSelectedGameObject == statDisplays[j].gameObject)
					{
						PlayableUnitManager.StatTooltip.TooltipUnitStatDisplay.TargetUnit = recruitmentUnitDisplays[TPSingleton<RecruitmentView>.Instance.selectedUnit].Unit;
						PlayableUnitManager.StatTooltip.Refresh();
						break;
					}
				}
			}
		}
		if (EventSystem.current.currentSelectedGameObject == null || InputManager.JoystickConfig.HUDNavigation.SelectRecruitButtonOnUnitChanged)
		{
			EventSystem.current.SetSelectedGameObject(recruitButton.gameObject);
		}
	}

	private void Update()
	{
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Recruitment)
		{
			return;
		}
		if (InputManager.GetButtonDown(29) || (InputManager.GetButtonDown(80) && InputManager.IsLastControllerJoystick))
		{
			if (!UnitPlacingDoneThisFrame)
			{
				RecruitmentController.CloseRecruitmentPanel();
			}
		}
		else if (InputManager.GetButtonDown(104))
		{
			RecruitmentController.BuyRerollRoster();
		}
		else if (InputManager.GetButtonDown(105))
		{
			SelectNextRecruit();
			OnNewUnitJoystickSelected();
		}
		else if (InputManager.GetButtonDown(106))
		{
			SelectPreviousRecruit();
			OnNewUnitJoystickSelected();
		}
		if (UnitPlacingDoneThisFrame)
		{
			UnitPlacingDoneThisFrame = false;
		}
	}

	[ContextMenu("Close")]
	public void DebugClose()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			Debug.LogError("Unable to use this context menu when the application is not running");
		}
		else if (TPSingleton<RecruitmentView>.Instance.isOpened)
		{
			RecruitmentController.CloseRecruitmentPanel();
		}
	}

	[ContextMenu("Open")]
	public void DebugOpen()
	{
		if (!UnityEngine.Application.isPlaying)
		{
			Debug.LogError("Unable to use this context menu when the application is not running");
		}
		else if (!TPSingleton<RecruitmentView>.Instance.isOpened)
		{
			RecruitmentController.OpenRecruitmentPanel();
		}
	}
}
