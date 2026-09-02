using System.Collections.Generic;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using TheLastStand.View.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class DraggedItem : TooltipBase
{
	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image iconBGImage;

	[SerializeField]
	protected DataColorTable rarityColors;

	[SerializeField]
	private Image itemBox;

	[SerializeField]
	private GameObject epicParticles;

	[SerializeField]
	private GameObject rareParticles;

	private bool canSell;

	public ItemSlot ItemSlot { get; private set; }

	public ItemSlot TargetItemSlot { get; set; }

	protected Dictionary<ItemDefinition.E_Rarity, GameObject> RarityParticles { get; private set; }

	public void Reset()
	{
		ResetContent();
		Hide();
	}

	public void SetContent(ItemSlot sourceItemSlot)
	{
		ItemSlot = sourceItemSlot;
		TargetItemSlot = sourceItemSlot;
	}

	protected override void Awake()
	{
		base.Awake();
		RarityParticles = new Dictionary<ItemDefinition.E_Rarity, GameObject>(default(ItemDefinition.RarityComparer))
		{
			{
				ItemDefinition.E_Rarity.Epic,
				epicParticles
			},
			{
				ItemDefinition.E_Rarity.Rare,
				rareParticles
			}
		};
	}

	protected override bool CanBeDisplayed()
	{
		return ItemSlot != null;
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (TPSingleton<ShopView>.Instance.ShopSellRectView.IsInRect && ItemSlot is ShopInventorySlot && !(TargetItemSlot is ShopSlot))
		{
			TargetItemSlot = TPSingleton<BuildingManager>.Instance.Shop.ShopSlots[0];
		}
		if (TargetItemSlot != null && TargetItemSlot != ItemSlot)
		{
			if (TargetItemSlot is EquipmentSlot equipmentSlot)
			{
				if (equipmentSlot.EquipmentSlotController.IsItemCompatible(ItemSlot.Item))
				{
					TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.EquipItem(ItemSlot.Item, equipmentSlot);
					RefreshCharacterSheetAfterEquippingItem();
					TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
				}
				else
				{
					TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropFailAudioClip);
				}
			}
			else if (ItemSlot is EquipmentSlot equipmentSlot2)
			{
				if (TargetItemSlot.Item == null)
				{
					ItemSlot.ItemSlotController.SwapItems(TargetItemSlot);
					equipmentSlot2.PlayableUnit.PlayableUnitController.RefreshStats();
					equipmentSlot2.PlayableUnit.PlayableUnitView?.RefreshBodyParts();
					RefreshCharacterSheetAfterEquippingItem();
					TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
				}
				else if (!ItemSlot.ItemSlotController.IsItemCompatible(TargetItemSlot.Item))
				{
					TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.OnEquipmentSlotDoubleClick(ItemSlot as EquipmentSlot);
					TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropFailAudioClip);
				}
				else
				{
					TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.EquipItem(TargetItemSlot.Item, equipmentSlot2);
					RefreshCharacterSheetAfterEquippingItem();
				}
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
			}
			else if (ItemSlot is ShopInventorySlot shopInventorySlot)
			{
				if (TargetItemSlot is ShopInventorySlot shopInventorySlot2)
				{
					ItemSlot = TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot.ShopInventorySlotView.ItemIndex];
					TargetItemSlot = TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot2.ShopInventorySlotView.ItemIndex];
					ItemSlot.ItemSlotController.SwapItems(TargetItemSlot);
					ItemSlot.ItemSlotView.Refresh();
					shopInventorySlot.Item = TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot.ShopInventorySlotView.ItemIndex].Item;
					shopInventorySlot2.Item = TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot2.ShopInventorySlotView.ItemIndex].Item;
					shopInventorySlot.ShopInventorySlotView.Refresh();
					shopInventorySlot2.ShopInventorySlotView.Refresh();
				}
				else if (TargetItemSlot is ShopSlot || TPSingleton<ShopView>.Instance.ShopSellRectView.IsInRect)
				{
					TPSingleton<BuildingManager>.Instance.Shop.ShopController.TrySellItem(shopInventorySlot);
				}
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
			}
			else if (ItemSlot is ShopSlot shopSlot)
			{
				if (TargetItemSlot is ShopInventorySlot shopInventorySlot3 && TPSingleton<BuildingManager>.Instance.Shop.ShopController.TryBuyItem(shopSlot, TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot3.ShopInventorySlotView.ItemIndex]))
				{
					TPSingleton<InventoryManager>.Instance.Inventory.InventorySlots[shopInventorySlot3.ShopInventorySlotView.ItemIndex].InventorySlotView.Refresh();
					TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
				}
				else
				{
					TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropFailAudioClip);
				}
			}
			else
			{
				ItemSlot.ItemSlotController.SwapItems(TargetItemSlot);
				TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropSuccessAudioClip);
			}
		}
		else
		{
			TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.DropFailAudioClip);
		}
		ResetContent();
	}

	protected override void RefreshContent()
	{
		iconImage.sprite = ItemView.GetUiSprite(ItemSlot.Item.ItemDefinition.ArtId);
		iconBGImage.sprite = ItemView.GetUiSprite(ItemSlot.Item.ItemDefinition.ArtId, isBG: true);
		iconBGImage.color = rarityColors.GetColorAt((int)(ItemSlot.Item.Rarity - 1));
		itemBox.sprite = ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}_Off", "View/Sprites/UI/Items/Rarity/ItemBox_0", (int)ItemSlot.Item.Rarity));
		if (RarityParticles == null)
		{
			return;
		}
		foreach (KeyValuePair<ItemDefinition.E_Rarity, GameObject> rarityParticle in RarityParticles)
		{
			rarityParticle.Value?.SetActive(rarityParticle.Key == ItemSlot.Item.Rarity);
		}
	}

	private void RefreshCharacterSheetAfterEquippingItem()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		TPSingleton<CharacterSheetPanel>.Instance.RefreshStats();
		TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar(selectedPlayableUnit);
		TPSingleton<CharacterSheetPanel>.Instance.RefreshSkills(selectedPlayableUnit);
	}

	private void ResetContent()
	{
		ItemSlot = null;
		TargetItemSlot = null;
		iconImage.sprite = null;
		iconBGImage.sprite = null;
	}

	private void Update()
	{
		if (base.Displayed && ItemSlot is ShopInventorySlot)
		{
			if (!canSell && (TargetItemSlot is ShopSlot || TPSingleton<ShopView>.Instance.ShopSellRectView.IsInRect))
			{
				itemBox.sprite = ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}_On", "View/Sprites/UI/Items/Rarity/ItemBox_0", (int)ItemSlot.Item.Rarity));
				canSell = true;
			}
			else if (canSell && !(TargetItemSlot is ShopSlot) && !TPSingleton<ShopView>.Instance.ShopSellRectView.IsInRect)
			{
				itemBox.sprite = ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}_Off", "View/Sprites/UI/Items/Rarity/ItemBox_0", (int)ItemSlot.Item.Rarity));
				canSell = false;
			}
		}
	}
}
