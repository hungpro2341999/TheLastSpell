using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.WorldMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class OpenWeaponRestrictionsView : MonoBehaviour
{
	[SerializeField]
	private WeaponFamiliesCountDisplay weaponFamiliesCountDisplay;

	[SerializeField]
	private Image boundlessModeIcon;

	[SerializeField]
	private BetterButton openWeaponRestrictionsPanelButton;

	[SerializeField]
	private WeaponRestrictionsPanel weaponRestrictionsPanel;

	private bool canBeDisplayed;

	public void DisplayOrRefresh()
	{
		if (canBeDisplayed)
		{
			Refresh();
		}
		else if (TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsAvailable)
		{
			canBeDisplayed = true;
			RefreshDisplayState();
			Refresh();
		}
	}

	public void Refresh()
	{
		if (canBeDisplayed)
		{
			RefreshContent();
			RefreshBoundlessModeIcon();
		}
	}

	public void RefreshBoundlessModeIcon()
	{
		boundlessModeIcon.enabled = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive;
	}

	private void OnWeaponRestrictionsButtonClicked()
	{
		if (WorldMapUIManager.CanOpenWeaponRestrictionsPanel)
		{
			weaponRestrictionsPanel.Open();
		}
	}

	private void RefreshContent()
	{
		weaponFamiliesCountDisplay.Refresh();
	}

	private void RefreshDisplayState()
	{
		base.gameObject.SetActive(canBeDisplayed);
	}

	private void Awake()
	{
		canBeDisplayed = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsAvailable;
		weaponFamiliesCountDisplay.Init();
	}

	private void Start()
	{
		RefreshDisplayState();
		openWeaponRestrictionsPanelButton.onClick.AddListener(OnWeaponRestrictionsButtonClicked);
		WeaponRestrictionsPanel.OnPanelClosed += Refresh;
		if (canBeDisplayed)
		{
			Refresh();
		}
	}

	private void OnDestroy()
	{
		openWeaponRestrictionsPanelButton.onClick.RemoveListener(OnWeaponRestrictionsButtonClicked);
		WeaponRestrictionsPanel.OnPanelClosed -= Refresh;
	}
}
