using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class IconTooltipDisplayer : TooltipDisplayer
{
	protected bool isFromLightShop;

	protected Vector2 oldPivot;

	private FollowElement.FollowDatas oldFollowData;

	[SerializeField]
	private FollowElement.FollowDatas followData;

	public override void DisplayTooltip()
	{
		oldPivot = targetTooltip.RectTransform.pivot;
		oldFollowData = new FollowElement.FollowDatas(targetTooltip.FollowElement.FollowElementDatas);
		targetTooltip.FollowElement.ChangeFollowDatas(followData);
		if (isFromLightShop)
		{
			targetTooltip.FollowElement.ChangeOffset(new Vector3(0f - followData.Offset.x, followData.Offset.y, followData.Offset.z));
		}
		UpdateAnchor();
		base.DisplayTooltip();
	}

	public override void HideTooltip()
	{
		targetTooltip.RectTransform.pivot = oldPivot;
		targetTooltip.FollowElement.ChangeFollowDatas(oldFollowData);
		base.HideTooltip();
	}

	protected void Init(TooltipBase tooltip = null, bool newIsFromLightShop = false)
	{
		if ((object)targetTooltip == null)
		{
			targetTooltip = tooltip;
		}
		isFromLightShop = newIsFromLightShop;
	}

	protected virtual void UpdateAnchor()
	{
		targetTooltip.RectTransform.pivot = (isFromLightShop ? Vector2.one : Vector2.up);
	}
}
