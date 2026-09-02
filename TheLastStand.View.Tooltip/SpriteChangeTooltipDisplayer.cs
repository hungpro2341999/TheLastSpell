using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class SpriteChangeTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	protected Image image;

	[SerializeField]
	protected Sprite offSprite;

	[SerializeField]
	protected Sprite onSprite;

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		SetOnSprite();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		SetOffSprite();
	}

	public void SetOnSprite()
	{
		image.sprite = onSprite;
	}

	public void SetOffSprite()
	{
		image.sprite = offSprite;
	}
}
