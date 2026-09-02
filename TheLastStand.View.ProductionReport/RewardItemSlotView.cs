using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Manager;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.Generic;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class RewardItemSlotView : ItemSlotView
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private Image selectedBG;

	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private AudioClip selectAudioClip;

	public RewardItemSlot RewardItemSlot
	{
		get
		{
			return base.ItemSlot as RewardItemSlot;
		}
		set
		{
			base.ItemSlot = value;
		}
	}

	public Selectable Selectable => selectable;

	public override void DisplayRarity(ItemDefinition.E_Rarity rarity, float offsetColorValue = 0f)
	{
		base.DisplayRarity(rarity, offsetColorValue);
		ToggleRarityParticles(TPSingleton<ChooseRewardPanel>.Instance.IsOpened);
	}

	public override void Refresh()
	{
		base.Refresh();
		ClearSlot();
		if (base.ItemSlot.Item == null)
		{
			base.ItemIcon.enabled = false;
			base.ItemIconBG.enabled = false;
		}
		else
		{
			base.ItemIcon.sprite = ItemView.GetUiSprite(base.ItemSlot.Item.ItemDefinition.ArtId);
			base.ItemIcon.enabled = true;
			base.ItemIconBG.sprite = ItemView.GetUiSprite(base.ItemSlot.Item.ItemDefinition.ArtId, isBG: true);
			base.ItemIconBG.enabled = true;
			base.ItemIconBG.color = iconRarityColors.GetColorAt((int)(base.ItemSlot.Item.Rarity - 1));
			DisplayRarity(base.ItemSlot.Item.Rarity);
			selectedBG.sprite = base.BackgroundGlowImage.sprite;
		}
		if (base.HasFocus)
		{
			DisplayTooltip(display: true);
		}
	}

	public override void DisplayTooltip(bool display)
	{
		DisplayTooltip(display, TPSingleton<ChooseRewardPanel>.Instance.UnitToCompareIndex, followDatas);
	}

	public void DisplaySelectionBG()
	{
		selectedBG.enabled = TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.ChosenItem != null && TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.ChosenItem == RewardItemSlot.Item;
	}

	public void OnClick()
	{
		OnPointerClick(null);
		base.HasFocus = true;
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			DisplayTooltip(display: true);
		}
	}

	public void OnDeselect()
	{
		base.HasFocus = false;
		DisplayTooltip(display: false);
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.BeginDragAudioClip);
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if (TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.ChosenItem != RewardItemSlot.Item)
		{
			base.OnPointerClick(eventData);
			TPSingleton<UIManager>.Instance.PlayAudioClip(selectAudioClip);
			TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.ChosenItem = RewardItemSlot.Item;
			TPSingleton<ChooseRewardPanel>.Instance.OnChosenItemChanged();
			base.HasFocus = true;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		DisplayTooltip(display: true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		DisplayTooltip(display: false);
	}

	protected override void Awake()
	{
		base.Awake();
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDestroy()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	protected override TheLastStand.Model.Item.Item GetItem()
	{
		return base.ItemSlot.Item;
	}

	protected override void ToggleRarityParticlesHook(bool enable)
	{
		if (enable)
		{
			if (!isParticleSystemHooked)
			{
				TPSingleton<ChooseRewardPanel>.Instance.OnRewardPanelToggle += base.ToggleRarityParticles;
			}
		}
		else if (isParticleSystemHooked)
		{
			TPSingleton<ChooseRewardPanel>.Instance.OnRewardPanelToggle -= base.ToggleRarityParticles;
		}
		isParticleSystemHooked = enable;
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (TPSingleton<ChooseRewardPanel>.Instance.IsOpened && TPSingleton<ChooseRewardPanel>.Instance.ProductionItem.ChosenItem == RewardItemSlot.Item)
		{
			DisplayTooltip(showTooltips && base.HasFocus);
		}
	}
}
