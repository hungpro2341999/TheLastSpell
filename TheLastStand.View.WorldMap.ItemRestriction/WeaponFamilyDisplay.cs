using DG.Tweening;
using TPLib;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class WeaponFamilyDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
{
	private static class Constants
	{
		public const string WeaponFamilyIconsPath = "View/Sprites/UI/Meta/DarkShop/Icon_DarkShop_Unlock";

		public const string WeaponFamilyIconDefault = "GuessAgain";
	}

	[SerializeField]
	private FollowElement.FollowDatas followDatas;

	[SerializeField]
	private Transform parentPivot;

	[SerializeField]
	private Image selectedIcon;

	[SerializeField]
	private Image boxBackground;

	[SerializeField]
	private Image boxHoveredFeedback;

	[SerializeField]
	private Image weaponFamilyIcon;

	[SerializeField]
	private Material grayScaleMaterial;

	[SerializeField]
	private Sprite lockSprite;

	[SerializeField]
	private Sprite checkmarkSprite;

	[SerializeField]
	private Sprite selectedBoxSprite;

	[SerializeField]
	private Sprite notSelectedBoxSprite;

	[SerializeField]
	protected JoystickSelectable joystickSelectable;

	[SerializeField]
	protected AudioClip hoverClip;

	[SerializeField]
	private float shakeDuration = 0.2f;

	[SerializeField]
	private int shakeStrength = 10;

	[SerializeField]
	private int shakeVibrato = 40;

	private ItemRestrictionCategoriesCollection categoriesCollection;

	public bool CanUnSelect
	{
		get
		{
			if (ItemFamily != null && categoriesCollection != null)
			{
				return categoriesCollection.GetCanUnSelectItemFamilyFromCategory(ItemFamily.ItemFamilyDefinition.ItemCategory);
			}
			return false;
		}
	}

	public ItemRestrictionFamily ItemFamily { get; private set; }

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public bool JoystickSelected { get; private set; }

	public void Init(ItemRestrictionFamily itemRestrictionFamily, ItemRestrictionCategoriesCollection itemRestrictionCategoriesCollection)
	{
		categoriesCollection = itemRestrictionCategoriesCollection;
		ItemFamily = itemRestrictionFamily;
		weaponFamilyIcon.sprite = GetItemFamilyIcon(itemRestrictionFamily.ItemFamilyDefinition);
	}

	public void OnSelect(BaseEventData eventData)
	{
		OnPointerEnter(null);
		JoystickSelected = true;
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		OnPointerExit(null);
		JoystickSelected = false;
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OnPointerClick(null);
	}

	public void Refresh()
	{
		if (ItemFamily != null)
		{
			selectedIcon.enabled = ItemFamily.IsSelected;
			if (ItemFamily.IsSelected)
			{
				boxBackground.sprite = selectedBoxSprite;
				selectedIcon.sprite = (CanUnSelect ? checkmarkSprite : lockSprite);
			}
			else
			{
				selectedIcon.enabled = false;
				boxBackground.sprite = notSelectedBoxSprite;
			}
			RefreshMaterial();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (ItemFamily.IsSelected && !CanUnSelect)
		{
			parentPivot.transform.DOComplete();
			parentPivot.transform.DOShakePosition(shakeDuration, Vector3.right * shakeStrength, shakeVibrato, 90f, snapping: false, fadeOut: false);
			TPSingleton<WeaponRestrictionsPanel>.Instance.PlayErrorClip();
		}
		else
		{
			TPSingleton<ItemRestrictionManager>.Instance.TryChangeItemFamilySelected(!ItemFamily.IsSelected, ItemFamily.Id);
			TPSingleton<WeaponRestrictionsPanel>.Instance.OnWeaponFamilyDisplaySelectChanged(ItemFamily);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		boxHoveredFeedback.enabled = true;
		SoundManager.PlayAudioClip(TPSingleton<WeaponRestrictionsPanel>.Instance.GetNextAudioSource(), hoverClip);
		if (!InputManager.IsLastControllerJoystick || TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			ShowTooltip();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		boxHoveredFeedback.enabled = false;
		TPSingleton<WeaponRestrictionsPanel>.Instance.WeaponFamilyTooltip.Hide();
	}

	private Sprite GetItemFamilyIcon(ItemRestrictionFamilyDefinition itemFamilyDefinition)
	{
		Sprite sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Meta/DarkShop/Icon_DarkShop_Unlock" + itemFamilyDefinition.ShortId, failSilently: true);
		if (!sprite)
		{
			return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Meta/DarkShop/Icon_DarkShop_UnlockGuessAgain");
		}
		return sprite;
	}

	private void RefreshMaterial()
	{
		weaponFamilyIcon.material = (ItemFamily.IsSelected ? null : grayScaleMaterial);
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (showTooltips && JoystickSelected)
		{
			ShowTooltip();
		}
		else
		{
			TPSingleton<WeaponRestrictionsPanel>.Instance.WeaponFamilyTooltip.Hide();
		}
	}

	private void ShowTooltip()
	{
		WeaponFamilyTooltip weaponFamilyTooltip = TPSingleton<WeaponRestrictionsPanel>.Instance.WeaponFamilyTooltip;
		weaponFamilyTooltip.Init(ItemFamily, categoriesCollection);
		weaponFamilyTooltip.FollowElement.ChangeFollowDatas(followDatas);
		weaponFamilyTooltip.Display();
	}
}
