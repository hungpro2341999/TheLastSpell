using TMPro;
using TPLib;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.NightReport;

public class PlayableReportDisplay : MonoBehaviour, ISubmitHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI unitName;

	[SerializeField]
	private UnitPortraitView unitPortraitView;

	[SerializeField]
	private UnitLevelDisplay unitLevelDisplay;

	[SerializeField]
	private UnitStatDisplay healthDisplay;

	[SerializeField]
	private RectTransform killCountParent;

	[SerializeField]
	private TextMeshProUGUI killCountText;

	[SerializeField]
	private Vector2 defaultKillCountPos = Vector3.zero;

	[SerializeField]
	private Vector2 killCountOffset = Vector3.zero;

	[SerializeField]
	private GameObject deathNightParent;

	[SerializeField]
	private TextMeshProUGUI deathNightText;

	[SerializeField]
	private Button characterSheetButton;

	[SerializeField]
	private Image deadCache;

	[SerializeField]
	private UnitExperienceTooltipDisplayer unitExperienceDisplayer;

	[SerializeField]
	private JoystickSelectable selectable;

	[SerializeField]
	private RectTransform rectTransform;

	private RectTransform parentRectTransform;

	private RectTransform scrollViewport;

	public PlayableUnit PlayableUnit { get; private set; }

	public UnitLevelDisplay UnitLevelDisplay => unitLevelDisplay;

	public JoystickSelectable Selectable => selectable;

	public void Init(RectTransform parent, RectTransform scrollView)
	{
		parentRectTransform = parent;
		scrollViewport = scrollView;
	}

	public void Refresh(PlayableUnit playableUnit, bool isDead = false, bool isEndGamePanel = false, int nightOfDeath = 0)
	{
		if (PlayableUnit != playableUnit)
		{
			PlayableUnit = playableUnit;
			unitLevelDisplay.PlayableUnit = playableUnit;
			unitLevelDisplay.Refresh();
			unitName.text = PlayableUnit.Name;
			unitPortraitView.PlayableUnit = PlayableUnit;
			unitPortraitView.RefreshPortrait();
			healthDisplay.TargetUnit = PlayableUnit;
			unitExperienceDisplayer.PlayableUnit = PlayableUnit;
		}
		deadCache.enabled = isDead;
		deathNightParent.SetActive(isDead && isEndGamePanel);
		characterSheetButton.gameObject.SetActive(isEndGamePanel);
		healthDisplay.Refresh();
		killCountParent.anchoredPosition = ((isDead && isEndGamePanel) ? killCountOffset : defaultKillCountPos);
		if (!isEndGamePanel)
		{
			killCountText.text = $"x{TPSingleton<PlayableUnitManager>.Instance.NightReport.GetTotalKillsThisNightForEntity(PlayableUnit)}";
			return;
		}
		killCountText.text = $"x{playableUnit.LifetimeStats.Kills}";
		if (isDead)
		{
			deathNightText.text = $"{nightOfDeath}";
		}
	}

	public void RefreshView()
	{
		unitLevelDisplay.Refresh();
		unitName.text = PlayableUnit.Name;
		unitPortraitView.RefreshPortrait();
		healthDisplay.Refresh();
	}

	public void OnSelect()
	{
		GUIHelpers.AdjustHorizontalScrollViewToFocusedItem(rectTransform, scrollViewport, parentRectTransform, 2f);
		characterSheetButton.OnPointerEnter(null);
	}

	public void OnDeselect()
	{
		characterSheetButton.OnPointerExit(null);
	}

	public void OpenCharacterSheet()
	{
		CharacterSheetManager.OpenCharacterSheetPanelWith(PlayableUnit, fromAnotherPopup: true);
		TPSingleton<CharacterSheetPanel>.Instance.OpenUnitDetails();
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OpenCharacterSheet();
	}

	private void Awake()
	{
		healthDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		healthDisplay.SecondaryStatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.HealthTotal];
	}
}
