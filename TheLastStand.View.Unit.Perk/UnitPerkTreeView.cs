using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class UnitPerkTreeView : TabbedPageView
{
	public static class Constants
	{
		private const string CollectionAssetsPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/";

		private const string BotAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot";

		private const string CenterAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center";

		private const string CrestAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}";

		private const string OffSuffix = "_Off";

		private const string OnSuffix = "_On";

		public const string TopAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Top";

		public const string BotOffAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_Off";

		public const string BotOnAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_On";

		public const string CenterOffAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_Off";

		public const string CenterOnAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_On";

		public const string CrestOffAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_Off";

		public const string CrestOnAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_On";

		public const string PerkUnlockAnimationPathFormat = "Animation/PerkUnlock/PerkUnlock_{0}";

		public const string DefaultCollectionName = "Misc";
	}

	[SerializeField]
	private int bannersSortingOrder;

	[SerializeField]
	private List<UnitPerkTierView> unitPerkTierView;

	[SerializeField]
	private BetterButton trainButton;

	[SerializeField]
	private Canvas bannersCanvas;

	[SerializeField]
	private RectTransform perkSelectorRect;

	[SerializeField]
	private Animator perkSelectorAnimator;

	[SerializeField]
	private TextMeshProUGUI unavailableText;

	[SerializeField]
	private TextMeshProUGUI perkPointsCount;

	[SerializeField]
	private RectTransform chainsRect;

	[SerializeField]
	private RectTransform perkLinesRect;

	[SerializeField]
	private List<PerkCollectionBannerView> bannerViews = new List<PerkCollectionBannerView>();

	[SerializeField]
	private CanvasGroup basicTopPanelParent;

	[SerializeField]
	private CanvasGroup rerollTopPanelParent;

	[SerializeField]
	private CanvasGroup rerollModeButtonParent;

	[SerializeField]
	private CanvasGroup cancelRerollParent;

	[SerializeField]
	private CanvasGroup confirmRerollParent;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsCountText;

	[SerializeField]
	private TextMeshProUGUI rerollWarningInfoText;

	[SerializeField]
	private TextMeshProUGUI rerollWarningCanNotRerollCollectionText;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip[] perkUnlockedClips;

	[SerializeField]
	private AudioClip[] perkRerollClips;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private GameObject defaultRerollObjectSelected;

	public static PerkTooltipDisplayer HoveredPerkTooltipDisplayer;

	private int lastUnlockPerkClipIndex = -1;

	public bool CanPlayRerollSound { get; set; } = true;

	public bool Inited { get; private set; }

	public bool IsInRerollMode { get; private set; }

	public IPerkRerollSelectable RerollTargetSelected { get; private set; }

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public UnitPerkDisplay SelectedPerk { get; private set; }

	public UnitPerkTree UnitPerkTree { get; private set; }

	public List<UnitPerkTierView> UnitPerkTierViews => unitPerkTierView;

	public List<IPerkRerollSelectable> PerkRerollSelectables { get; private set; } = new List<IPerkRerollSelectable>();

	public static T GetCollectionAssetOrDefault<T>(string assetPathFormat, string collectionId) where T : Object
	{
		T val = ResourcePooler.LoadOnce<T>(string.Format(assetPathFormat, collectionId), failSilently: true);
		if (val == null)
		{
			TPSingleton<PlayableUnitManager>.Instance.Log("Could not find collection asset at path \"" + string.Format(assetPathFormat, collectionId) + "\". Using default (Misc) instead.");
			val = ResourcePooler.LoadOnce<T>(string.Format(assetPathFormat, "Misc"));
		}
		return val;
	}

	public override void Close()
	{
		if (base.IsOpened)
		{
			PlayableUnitManager.PerkTooltip.Hide();
			base.Close();
		}
	}

	public void Init()
	{
		for (int i = 0; i < unitPerkTierView.Count; i++)
		{
			unitPerkTierView[i].Tier = i;
			unitPerkTierView[i].RefreshText();
			PerkRerollSelectables.Add(unitPerkTierView[i]);
		}
		int collectionIndex;
		for (collectionIndex = 0; collectionIndex < bannerViews.Count; collectionIndex++)
		{
			PerkRerollSelectables.Add(bannerViews[collectionIndex]);
			bannerViews[collectionIndex].PerkDisplays.AddRange(unitPerkTierView.Select((UnitPerkTierView tier) => tier.PerkDisplays[collectionIndex]));
		}
		Inited = true;
	}

	public void OnTrainButtonClick()
	{
		UnitPerkTree.UnitPerkTreeController.BuyPerk();
	}

	public override void Open()
	{
		if (!base.IsOpened)
		{
			IsInRerollMode = false;
			ChangeTargetSelectedToReroll(null);
			base.Open();
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnPerksOpen);
		}
	}

	public void PlayPerkSelectionSound()
	{
		int num = UnityEngine.Random.Range(0, perkUnlockedClips.Length);
		if (perkUnlockedClips.Length > 1)
		{
			while (num == lastUnlockPerkClipIndex)
			{
				num = UnityEngine.Random.Range(0, perkUnlockedClips.Length);
			}
		}
		lastUnlockPerkClipIndex = num;
		audioSource.PlayOneShot(perkUnlockedClips[num]);
	}

	public void RefreshPerkPoints()
	{
		perkPointsCount.text = (UnitPerkTree.HasReachedMaxPerks() ? Localizer.Get("CharacterSheet_MaxPerksPoints") : UnitPerkTree.PlayableUnit.PerksPoints.ToString());
	}

	public void RefreshSelectedPerk(UnitPerkDisplay selectedPerk)
	{
		UnitPerkDisplay selectedPerk2 = SelectedPerk;
		SelectedPerk = selectedPerk;
		if (selectedPerk2 != null)
		{
			selectedPerk2.Refresh();
		}
		if (SelectedPerk != null)
		{
			SelectedPerk.Refresh();
		}
		bool flag = SelectedPerk != null && !SelectedPerk.Perk.UnlockedInPerkTree && SelectedPerk.Perk.PerkTier.Available && UnitPerkTree.CanBuyPerk();
		bool flag2 = TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver;
		trainButton.Interactable = SelectedPerk != null && flag;
		perkSelectorRect.gameObject.SetActive(SelectedPerk != null && (!flag2 || InputManager.IsLastControllerJoystick));
		if (!(SelectedPerk == null))
		{
			perkSelectorAnimator.SetBool("isAvailable", flag);
			string key = string.Empty;
			if (!flag && !flag2)
			{
				key = ((!SelectedPerk.Perk.UnlockedInPerkTree) ? (UnitPerkTree.HasReachedMaxPerks() ? "CharacterSheet_MaxPerksReached" : (SelectedPerk.Perk.PerkTier.Available ? "CharacterSheet_NotEnoughPoints" : "CharacterSheet_TierLocked")) : "CharacterSheet_PerkAlreadyUnlocked");
			}
			unavailableText.text = Localizer.Get(key);
			perkSelectorRect.SetParent(SelectedPerk.transform);
			perkSelectorRect.localPosition = SelectedPerk.PerkSelectorPos.localPosition;
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		UnitPerkTree = TileObjectSelectionManager.SelectedPlayableUnit.PerkTree;
		if (!InputManager.IsLastControllerJoystick)
		{
			SelectedPerk = null;
		}
		else if (InputManager.JoystickConfig.HUDNavigation.SelectFirstPerkOnUnitChange)
		{
			SelectedPerk = UnitPerkTierViews[0].PerkDisplays[0];
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(SelectedPerk.gameObject);
			}
		}
		HashSet<int> lockedPerkCollectionSlots = TPSingleton<MetaUpgradesManager>.Instance.GetLockedPerkCollectionSlots();
		for (int i = 0; i < UnitPerkTree.UnitPerkCollectionIds.Count; i++)
		{
			bannerViews[i].CollectionIndex = i;
			if (lockedPerkCollectionSlots.Contains(i + 1))
			{
				bannerViews[i].gameObject.SetActive(value: false);
				continue;
			}
			bannerViews[i].gameObject.SetActive(value: true);
			bannerViews[i].Refresh(UnitPerkTree.UnitPerkCollectionIds[i]);
		}
		bool flag = true;
		for (int j = 0; j < UnitPerkTierViews.Count; j++)
		{
			UnitPerkTierViews[j].UnitPerkTier = UnitPerkTree.UnitPerkTiers[j];
			for (int k = 0; k < UnitPerkTierViews[j].PerkDisplays.Count; k++)
			{
				if (lockedPerkCollectionSlots.Contains(k + 1))
				{
					UnitPerkTierViews[j].PerkDisplays[k].gameObject.SetActive(value: false);
					continue;
				}
				UnitPerkTierViews[j].PerkDisplays[k].gameObject.SetActive(value: true);
				UnitPerkTierViews[j].PerkDisplays[k].SetContent(UnitPerkTree.UnitPerkTiers[j].Perks[k]);
				UnitPerkTierViews[j].PerkDisplays[k].Init();
			}
			bool flag2 = flag && !UnitPerkTierViews[j].UnitPerkTier.Available;
			if (flag2)
			{
				chainsRect.gameObject.SetActive(value: true);
				chainsRect.offsetMax = new Vector2(chainsRect.offsetMax.x, UnitPerkTierViews[j].Separator.position.y - perkLinesRect.position.y);
			}
			UnitPerkTierViews[j].RefreshAvailability(flag2);
			flag = UnitPerkTierViews[j].UnitPerkTier.Available;
		}
		if (flag)
		{
			chainsRect.gameObject.SetActive(value: false);
		}
		RefreshHoveredPerkTooltipDisplayer();
		RefreshSelectedPerk(SelectedPerk);
		RefreshPerkPoints();
		bannersCanvas.sortingOrder = bannersSortingOrder;
		RefreshTopPanel();
		RefreshRerollSelectionFeedback();
		InitializeJoystickNavigation();
	}

	public void OnStartRerollButtonClick()
	{
		IsInRerollMode = true;
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<CharacterSheetPanel>.Instance.PerksJoystickTarget.GetSelectionInfo());
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		RefreshSelectedPerk(null);
		RefreshTopPanel();
		ChangeTargetSelectedToReroll(null);
		RefreshRerollSelectionFeedback();
		UnitPerkTree.UnitPerkTreeController.ComputeRerollPrices();
		perkSelectorRect.gameObject.SetActive(value: false);
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(defaultRerollObjectSelected);
		}
	}

	public void OnCancelRerollButtonClick()
	{
		CancelReroll();
	}

	public void OnConfirmRerollButtonClick()
	{
		RerollTargetSelected?.ExecuteReroll();
		IsInRerollMode = false;
		ChangeTargetSelectedToReroll(null);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<CharacterSheetPanel>.Instance.PerksJoystickTarget.GetSelectionInfo());
		}
		Refresh();
	}

	public void CancelReroll()
	{
		IsInRerollMode = false;
		ChangeTargetSelectedToReroll(null);
		RefreshTopPanel();
		RefreshRerollSelectionFeedback();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<CharacterSheetPanel>.Instance.PerksJoystickTarget.GetSelectionInfo());
		}
	}

	public void ChangeTargetSelectedToReroll(IPerkRerollSelectable newTargetToReroll)
	{
		RerollTargetSelected = newTargetToReroll;
		bool flag = RerollTargetSelected != null && RerollTargetSelected.CanReroll() && !InputManager.IsLastControllerJoystick;
		confirmRerollParent.Display(flag);
		cancelRerollParent.Display(!flag);
		UpdateTargetsSelectionView();
	}

	public void RefreshTopPanel()
	{
		basicTopPanelParent.Display(!IsInRerollMode);
		rerollTopPanelParent.Display(IsInRerollMode);
		rerollModeButtonParent.Display(TPSingleton<SinkManager>.Instance.IsSinkUnlocked);
		damnedSoulsCountText.text = $"{ApplicationManager.Application.DamnedSouls} <style=\"DamnedSouls\">";
	}

	private void UpdateTargetsSelectionView()
	{
		foreach (IPerkRerollSelectable perkRerollSelectable in PerkRerollSelectables)
		{
			if (RerollTargetSelected != perkRerollSelectable)
			{
				perkRerollSelectable.ChangeSelection(isSelected: false);
			}
		}
		RerollTargetSelected?.ChangeSelection(isSelected: true);
		bool flag = RerollTargetSelected?.CanCollectionRerollsCompletely ?? false;
		rerollWarningInfoText.enabled = flag;
		rerollWarningCanNotRerollCollectionText.enabled = !flag && (RerollTargetSelected?.IsCollectionRerollCompletelyLocked ?? false);
	}

	private void RefreshRerollSelectionFeedback()
	{
		foreach (UnitPerkTierView unitPerkTierView in UnitPerkTierViews)
		{
			unitPerkTierView.DisplayRerollFeedback(IsInRerollMode);
		}
		foreach (PerkCollectionBannerView bannerView in bannerViews)
		{
			bannerView.DisplayRerollFeedback(IsInRerollMode);
		}
	}

	public void PlayPerkRerollSound()
	{
		int num = UnityEngine.Random.Range(0, perkRerollClips.Length);
		audioSource.PlayOneShot(perkRerollClips[num]);
	}

	private void RefreshHoveredPerkTooltipDisplayer()
	{
		if (HoveredPerkTooltipDisplayer != null && TileObjectSelectionManager.SelectedPlayableUnit != null)
		{
			HoveredPerkTooltipDisplayer.DisplayTooltip(display: false);
			HoveredPerkTooltipDisplayer.DisplayTooltip(display: true);
		}
	}

	private void RevertJoystickNavigation()
	{
		foreach (UnitPerkTierView unitPerkTierView in UnitPerkTierViews)
		{
			foreach (UnitPerkDisplay perkDisplay in unitPerkTierView.PerkDisplays)
			{
				perkDisplay.JoystickSelectable.ClearNavigation();
			}
		}
	}

	private void InitializeJoystickNavigation()
	{
		RevertJoystickNavigation();
		List<List<UnitPerkDisplay>> list = new List<List<UnitPerkDisplay>>();
		foreach (UnitPerkTierView unitPerkTierView in UnitPerkTierViews)
		{
			List<UnitPerkDisplay> list2 = new List<UnitPerkDisplay>();
			foreach (UnitPerkDisplay perkDisplay in unitPerkTierView.PerkDisplays)
			{
				if (perkDisplay.gameObject.activeInHierarchy)
				{
					list2.Add(perkDisplay);
				}
			}
			list.Add(list2);
		}
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Count; j++)
			{
				bool num = j == 0;
				bool flag = j == list[i].Count - 1;
				bool flag2 = i == 0;
				bool flag3 = i == list.Count - 1;
				UnitPerkDisplay unitPerkDisplay = list[i][j];
				unitPerkDisplay.JoystickSelectable.SetMode(Navigation.Mode.Explicit);
				if (!num)
				{
					unitPerkDisplay.JoystickSelectable.SetSelectOnLeft(list[i][j - 1].JoystickSelectable);
				}
				if (!flag)
				{
					unitPerkDisplay.JoystickSelectable.SetSelectOnRight(list[i][j + 1].JoystickSelectable);
				}
				if (!flag2)
				{
					unitPerkDisplay.JoystickSelectable.SetSelectOnUp(list[i - 1][j].JoystickSelectable);
				}
				else
				{
					unitPerkDisplay.JoystickSelectable.SetSelectOnUp(TPSingleton<CharacterSheetPanel>.Instance.UnitRaceDisplay.JoystickSelectable);
				}
				if (!flag3)
				{
					unitPerkDisplay.JoystickSelectable.SetSelectOnDown(list[i + 1][j].JoystickSelectable);
				}
				if (unitPerkDisplay.JoystickSelectable.navigation.selectOnLeft == null)
				{
					unitPerkDisplay.JoystickSelectable.SetSelectOnLeft(unitPerkDisplay.DefaultSelectOnLeft);
				}
			}
		}
	}
}
