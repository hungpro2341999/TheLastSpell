using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Unit;
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.View.HUD;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.Trait;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet;

public class CharacterDetailsView : TabbedPageView
{
	public static class Constants
	{
		public const string LifetimeStatsNamePrefix = "LifetimeStats_";

		public const string LifetimeStatsNameLocalizationKey = "LifetimeStats_Name";

		public const string SecondaryAttributesNameLocalizationKey = "SecondaryAttributes_Name";

		public const string BestBlowNameLocalizationKey = "LifetimeStats_BestBlow";

		public const string BestFiendNameLocalizationKey = "LifetimeStats_BestFiend";

		public const string CriticalHitsNameLocalizationKey = "LifetimeStats_CriticalHits";

		public const string DamagesBlockedNameLocalizationKey = "LifetimeStats_DamagesBlocked";

		public const string DamagesInflictedNameLocalizationKey = "LifetimeStats_DamagesInflicted";

		public const string DamagesTakenOnArmorNameLocalizationKey = "LifetimeStats_DamagesTakenOnArmor";

		public const string DodgesNameLocalizationKey = "LifetimeStats_Dodges";

		public const string HealthLostNameLocalizationKey = "LifetimeStats_HealthLost";

		public const string JumpsOverWallNameLocalizationKey = "LifetimeStats_JumpsOverWall";

		public const string KillsLocalizationKey = "LifetimeStats_Kills";

		public const string ManaSpentNameLocalizationKey = "LifetimeStats_ManaSpent";

		public const string MostUnitsInOneBlowNameLocalizationKey = "LifetimeStats_MostUnitsKilledInOneBlow";

		public const string NemesisNameLocalizationKey = "LifetimeStats_Nemesis";

		public const string PreferredWeaponNameLocalizationKey = "LifetimeStats_PreferredWeapon";

		public const string PunchesUsedNameLocalizationKey = "LifetimeStats_PunchesUsed";

		public const string StunnedEnemiesNameLocalizationKey = "LifetimeStats_StunnedEnemies";

		public const string TilesCrossedNameLocalizationKey = "LifetimeStats_TilesCrossed";

		public const string NoBestFiendLocalizationKey = "LifetimeStats_NoBestFiend";

		public const string NoNemesisLocalizationKey = "LifetimeStats_NoNemesis";

		public const string NoPreferredWeaponLocalizationKey = "LifetimeStats_NoPreferredWeapon";
	}

	[SerializeField]
	private Scrollbar characterDetailsPanelScrollbar;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private GameObject levelUpButtonPanel;

	[SerializeField]
	private Button levelUpButton;

	[SerializeField]
	private RectTransform detailsViewportJoystickScroll;

	[SerializeField]
	private Scrollbar detailsScrollbar;

	[SerializeField]
	private ScrollRect detailsScrollRect;

	[SerializeField]
	private DismissHeroButton dismissHeroButton;

	[SerializeField]
	private TextMeshProUGUI lifetimeStatsTitleText;

	[SerializeField]
	private LifetimeStatDisplay bestBlowDisplay;

	[SerializeField]
	private LifetimeStatDisplay bestFiendDisplay;

	[SerializeField]
	private LifetimeStatDisplay criticalHitsDisplay;

	[SerializeField]
	private LifetimeStatDisplay damagesBlockedDisplay;

	[SerializeField]
	private LifetimeStatDisplay damagesInflictedDisplay;

	[SerializeField]
	private LifetimeStatDisplay damagedTakenOnArmorDisplay;

	[SerializeField]
	private LifetimeStatDisplay dodgesDisplay;

	[SerializeField]
	private LifetimeStatDisplay healthLostDisplay;

	[SerializeField]
	private LifetimeStatDisplay jumpsOverWallUsedDisplay;

	[SerializeField]
	private LifetimeStatDisplay killsDisplay;

	[SerializeField]
	private LifetimeStatDisplay manaSpentDisplay;

	[SerializeField]
	private LifetimeStatDisplay mostUnitsInOneBlowDisplay;

	[SerializeField]
	private LifetimeStatDisplay nemesisDisplay;

	[SerializeField]
	private LifetimeStatDisplay preferredWeaponDisplay;

	[SerializeField]
	private LifetimeStatDisplay punchesUsedDisplay;

	[SerializeField]
	private LifetimeStatDisplay stunnedEnemiesDisplay;

	[SerializeField]
	private LifetimeStatDisplay tilesCrossedDisplay;

	[SerializeField]
	private TextMeshProUGUI secondaryStatsTitleText;

	[SerializeField]
	private UnitStatDisplay[] secondaryAttributesDisplays;

	[SerializeField]
	private Color defaultColor = Color.white;

	[SerializeField]
	private DataColor bonusColor;

	[SerializeField]
	private DataColor malusColor;

	[SerializeField]
	private List<UnitTraitDisplay> unitTraits;

	[SerializeField]
	private TextMeshProUGUI unitNameDetails;

	[SerializeField]
	private RectTransform secondaryStatsLeftPanel;

	[SerializeField]
	private RectTransform secondaryStatsRightPanel;

	[SerializeField]
	private Selectable levelUpButtonDisabledTarget;

	[SerializeField]
	private HUDJoystickTarget secondaryAttributesHUDJoystickTarget;

	[SerializeField]
	private bool initSecondaryAttributesNavigationOnInit;

	[SerializeField]
	private Selectable dismissHeroButtonSelectable;

	[SerializeField]
	private Selectable leftTraitSelectable;

	private PlayableUnit playableUnit;

	public Selectable DismissHeroButtonSelectable => dismissHeroButtonSelectable;

	public Selectable LeftTraitSelectable => leftTraitSelectable;

	public Button LevelUpButton => levelUpButton;

	public Selectable LevelUpButtonDisabledTarget => levelUpButtonDisabledTarget;

	public HUDJoystickTarget SecondaryAttributesHUDJoystickTarget => secondaryAttributesHUDJoystickTarget;

	public override void Close()
	{
		if (base.IsOpened)
		{
			PlayableUnitManager.TraitTooltip.Hide();
			PlayableUnitManager.StatTooltip.Hide();
			base.Close();
			detailsScrollRect.enabled = false;
		}
	}

	public void OnBotButtonClick()
	{
		characterDetailsPanelScrollbar.value = Mathf.Clamp01(characterDetailsPanelScrollbar.value - scrollButtonsSensitivity);
	}

	public void OnTopButtonClick()
	{
		characterDetailsPanelScrollbar.value = Mathf.Clamp01(characterDetailsPanelScrollbar.value + scrollButtonsSensitivity);
	}

	public void OnLevelUpButtonJoystickSelect()
	{
		detailsScrollbar.value = 1f;
	}

	public override void Open()
	{
		if (!base.IsOpened)
		{
			detailsScrollRect.enabled = true;
			base.Open();
			this.DoAfter(0.05f, delegate
			{
				characterDetailsPanelScrollbar.value = 1f;
			});
			StartCoroutine(TriggerTutorialAfterCharacterSheetTween());
		}
	}

	public override void Refresh()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			base.Refresh();
			playableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
			unitNameDetails.text = playableUnit.Name;
			RefreshLocalizedTexts();
			RefreshLifetimeStats();
			RefreshSecondaryStats();
			RefreshTraits();
			if (dismissHeroButton != null)
			{
				dismissHeroButton.Refresh();
			}
			if (fontLocalizedParent != null)
			{
				fontLocalizedParent.RefreshChildren();
			}
			bool flag = playableUnit.StatsPoints > 0 && UnitLevelUpController.CanOpenUnitLevelUpView;
			levelUpButtonPanel.SetActive(flag);
			levelUpButton.interactable = flag;
		}
	}

	protected override void Start()
	{
		base.Start();
		Init();
	}

	private void Init()
	{
		levelUpButton.onClick.AddListener(delegate
		{
			TPSingleton<CharacterSheetPanel>.Instance.OnUnitLevelButtonClick(shouldRefreshTooltip: false);
			levelUpButton.interactable = playableUnit.StatsPoints > 0;
		});
		InitJoystickNavigation();
	}

	private void InitJoystickNavigation()
	{
		if (initSecondaryAttributesNavigationOnInit)
		{
			InitSecondaryAttributesJoystickNavigation();
		}
		for (int i = 0; i < secondaryStatsLeftPanel.childCount; i++)
		{
			Transform secondaryStat = secondaryStatsLeftPanel.GetChild(i);
			secondaryStat.GetComponent<JoystickSelectable>().AddListenerOnSelect(delegate
			{
				OnSecondaryStatJoystickSelect(secondaryStat as RectTransform);
			});
		}
		for (int num = 0; num < secondaryStatsRightPanel.childCount; num++)
		{
			Transform secondaryStat2 = secondaryStatsRightPanel.GetChild(num);
			secondaryStat2.GetComponent<JoystickSelectable>().AddListenerOnSelect(delegate
			{
				OnSecondaryStatJoystickSelect(secondaryStat2 as RectTransform);
			});
		}
		bestBlowDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(bestBlowDisplay.transform as RectTransform);
		});
		bestFiendDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(bestFiendDisplay.transform as RectTransform);
		});
		criticalHitsDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(criticalHitsDisplay.transform as RectTransform);
		});
		damagesBlockedDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(damagesBlockedDisplay.transform as RectTransform);
		});
		damagesInflictedDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(damagesInflictedDisplay.transform as RectTransform);
		});
		damagedTakenOnArmorDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(damagedTakenOnArmorDisplay.transform as RectTransform);
		});
		dodgesDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(dodgesDisplay.transform as RectTransform);
		});
		healthLostDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(healthLostDisplay.transform as RectTransform);
		});
		jumpsOverWallUsedDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(jumpsOverWallUsedDisplay.transform as RectTransform);
		});
		killsDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(killsDisplay.transform as RectTransform);
		});
		manaSpentDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(manaSpentDisplay.transform as RectTransform);
		});
		mostUnitsInOneBlowDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(mostUnitsInOneBlowDisplay.transform as RectTransform);
		});
		nemesisDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(nemesisDisplay.transform as RectTransform);
		});
		preferredWeaponDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(preferredWeaponDisplay.transform as RectTransform);
		});
		punchesUsedDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(punchesUsedDisplay.transform as RectTransform);
		});
		stunnedEnemiesDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(stunnedEnemiesDisplay.transform as RectTransform);
		});
		tilesCrossedDisplay.JoystickSelectable.AddListenerOnSelect(delegate
		{
			OnSecondaryStatJoystickSelect(tilesCrossedDisplay.transform as RectTransform);
		});
	}

	private void InitSecondaryAttributesJoystickNavigation()
	{
		JoystickSelectable[] array = new JoystickSelectable[secondaryStatsLeftPanel.childCount];
		JoystickSelectable[] array2 = new JoystickSelectable[secondaryStatsRightPanel.childCount];
		for (int i = 0; i < secondaryStatsLeftPanel.childCount; i++)
		{
			array[i] = secondaryStatsLeftPanel.GetChild(i).GetComponent<JoystickSelectable>();
		}
		for (int j = 0; j < secondaryStatsRightPanel.childCount; j++)
		{
			array2[j] = secondaryStatsRightPanel.GetChild(j).GetComponent<JoystickSelectable>();
		}
		for (int k = 0; k < array.Length; k++)
		{
			JoystickSelectable selectable = array[k];
			selectable.SetMode(Navigation.Mode.Explicit);
			if (k > 0)
			{
				selectable.SetSelectOnUp(array[k - 1]);
			}
			if (k < array.Length - 1)
			{
				selectable.SetSelectOnDown(array[k + 1]);
			}
			if (k < array2.Length)
			{
				selectable.SetSelectOnRight(array2[k]);
			}
		}
		for (int l = 0; l < array2.Length; l++)
		{
			JoystickSelectable selectable2 = array2[l];
			selectable2.SetMode(Navigation.Mode.Explicit);
			if (l > 0)
			{
				selectable2.SetSelectOnUp(array2[l - 1]);
			}
			if (l < array2.Length - 1)
			{
				selectable2.SetSelectOnDown(array2[l + 1]);
			}
			if (l < array.Length)
			{
				selectable2.SetSelectOnLeft(array[l]);
			}
		}
	}

	private void RefreshLocalizedTexts()
	{
		lifetimeStatsTitleText.text = Localizer.Get("LifetimeStats_Name");
		secondaryStatsTitleText.text = Localizer.Get("SecondaryAttributes_Name");
	}

	private void RefreshLifetimeStats()
	{
		bestBlowDisplay.Refresh(Localizer.Get("LifetimeStats_BestBlow"), playableUnit.LifetimeStats.BestBlow.ToString());
		criticalHitsDisplay.Refresh(Localizer.Get("LifetimeStats_CriticalHits"), playableUnit.LifetimeStats.CriticalHits.ToString());
		damagesBlockedDisplay.Refresh(Localizer.Get("LifetimeStats_DamagesBlocked"), playableUnit.LifetimeStats.DamagesBlocked.ToString());
		damagesInflictedDisplay.Refresh(Localizer.Get("LifetimeStats_DamagesInflicted"), playableUnit.LifetimeStats.DamagesInflicted.ToString());
		damagedTakenOnArmorDisplay.Refresh(Localizer.Get("LifetimeStats_DamagesTakenOnArmor"), playableUnit.LifetimeStats.DamagesTakenOnArmor.ToString());
		dodgesDisplay.Refresh(Localizer.Get("LifetimeStats_Dodges"), playableUnit.LifetimeStats.Dodges.ToString());
		healthLostDisplay.Refresh(Localizer.Get("LifetimeStats_HealthLost"), playableUnit.LifetimeStats.HealthLost.ToString());
		jumpsOverWallUsedDisplay.Refresh(Localizer.Get("LifetimeStats_JumpsOverWall"), playableUnit.LifetimeStats.JumpsOverWallUsed.ToString());
		killsDisplay.Refresh(Localizer.Get("LifetimeStats_Kills"), playableUnit.LifetimeStats.Kills.ToString());
		manaSpentDisplay.Refresh(Localizer.Get("LifetimeStats_ManaSpent"), playableUnit.LifetimeStats.ManaSpent.ToString());
		mostUnitsInOneBlowDisplay.Refresh(Localizer.Get("LifetimeStats_MostUnitsKilledInOneBlow"), playableUnit.LifetimeStats.MostUnitsKilledInOneBlow.ToString());
		punchesUsedDisplay.Refresh(Localizer.Get("LifetimeStats_PunchesUsed"), playableUnit.LifetimeStats.PunchesUsed.ToString());
		stunnedEnemiesDisplay.Refresh(Localizer.Get("LifetimeStats_StunnedEnemies"), playableUnit.LifetimeStats.StunnedEnemies.ToString());
		tilesCrossedDisplay.Refresh(Localizer.Get("LifetimeStats_TilesCrossed"), playableUnit.LifetimeStats.TilesCrossed.ToString());
		if (playableUnit.LifetimeStats.LifetimeStatsController.TryGetBestFiend(out string bestFiendId))
		{
			string value = string.Empty;
			if (!Localizer.TryGet("EnemyName_" + bestFiendId, out value))
			{
				Localizer.TryGet("BossName_" + bestFiendId, out value);
			}
			bestFiendDisplay.Refresh(Localizer.Get("LifetimeStats_BestFiend"), value);
		}
		else
		{
			bestFiendDisplay.Refresh(Localizer.Get("LifetimeStats_BestFiend"), Localizer.Get("LifetimeStats_NoBestFiend"));
		}
		if (playableUnit.LifetimeStats.LifetimeStatsController.TryGetNemesisId(out var nemesisId))
		{
			string value2;
			string statValue = (Localizer.TryGet("BossName_" + nemesisId, out value2) ? value2 : Localizer.Get("EnemyName_" + nemesisId));
			nemesisDisplay.Refresh(Localizer.Get("LifetimeStats_Nemesis"), statValue);
		}
		else
		{
			nemesisDisplay.Refresh(Localizer.Get("LifetimeStats_Nemesis"), Localizer.Get("LifetimeStats_NoNemesis"));
		}
		preferredWeaponDisplay.Refresh(Localizer.Get("LifetimeStats_PreferredWeapon"), playableUnit.LifetimeStats.LifetimeStatsController.TryGetPreferredWeaponId(out var weaponUses) ? ItemDatabase.ItemDefinitions[weaponUses.Item1].BaseName : Localizer.Get("LifetimeStats_NoPreferredWeapon"));
	}

	private void RefreshSecondaryStats()
	{
		for (int num = secondaryAttributesDisplays.Length - 1; num >= 0; num--)
		{
			secondaryAttributesDisplays[num].TargetUnit = playableUnit;
			secondaryAttributesDisplays[num].Refresh();
			float num2 = playableUnit.UnitStatsController.ComputeStatBonus(secondaryAttributesDisplays[num].StatDefinition.Id);
			if (num2 == 0f)
			{
				secondaryAttributesDisplays[num].ColorOverride = defaultColor;
			}
			else if (num2 > 0f)
			{
				secondaryAttributesDisplays[num].ColorOverride = bonusColor._Color;
			}
			else
			{
				secondaryAttributesDisplays[num].ColorOverride = malusColor._Color;
			}
			secondaryAttributesDisplays[num].Refresh();
		}
	}

	private void OnSecondaryStatJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(source, detailsViewportJoystickScroll, detailsScrollbar, 0.04f, 0f);
	}

	private void RefreshTraits()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			base.Refresh();
			PlayableUnitManager.TraitTooltip.Hide();
			PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
			for (int i = 0; i < unitTraits.Count; i++)
			{
				unitTraits[i].UnitTraitDefinition = ((selectedPlayableUnit.UnitTraitDefinitions.Count > i) ? selectedPlayableUnit.UnitTraitDefinitions[i] : null);
				unitTraits[i].Refresh();
			}
		}
	}

	private IEnumerator TriggerTutorialAfterCharacterSheetTween()
	{
		yield return new WaitUntil(() => !TPSingleton<CharacterSheetPanel>.Instance.IsDisplayTweenPlaying);
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnCharacterDetailsOpen);
	}
}
