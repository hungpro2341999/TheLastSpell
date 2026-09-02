using System;
using TMPro;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class MetaShopTab : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private RectTransform contentRectTransform;

	[SerializeField]
	private TextMeshProUGUI categoryTitle;

	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private float yOffset = 10f;

	[SerializeField]
	private MetaUpgradeDefinition.E_MetaUpgradeCategory category;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Sprite iconSpriteOn;

	[SerializeField]
	private Sprite iconSpriteOff;

	[SerializeField]
	private Image backgroundImage;

	[SerializeField]
	private Sprite backgroundSpriteOn;

	[SerializeField]
	private Sprite backgroundSpriteOff;

	public MetaUpgradeDefinition.E_MetaUpgradeCategory Category => category;

	public bool Enabled { get; private set; }

	public event Action<MetaShopTab> OnClicked;

	public event Action<MetaShopTab, bool> OnHover;

	public void Enable(bool enable)
	{
		Enabled = enable;
		iconImage.sprite = (enable ? iconSpriteOn : iconSpriteOff);
		button.Interactable = enable;
	}

	public void Toggle(bool toggle)
	{
		contentRectTransform.anchoredPosition = new Vector2(contentRectTransform.anchoredPosition.x, toggle ? 0f : yOffset);
		backgroundImage.sprite = (toggle ? backgroundSpriteOn : backgroundSpriteOff);
		iconImage.sprite = (toggle ? iconSpriteOn : iconSpriteOff);
		ToggleCategoryTitle(toggle);
	}

	public void OnClick()
	{
		this.OnClicked?.Invoke(this);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.OnHover?.Invoke(this, arg2: true);
		ToggleCategoryTitle(toggle: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ToggleCategoryTitle(toggle: false);
		this.OnHover?.Invoke(this, arg2: false);
	}

	public void ToggleCategoryTitle(bool toggle)
	{
		categoryTitle.enabled = toggle;
	}
}
