using TheLastStand.Manager;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Generic;

public class GenericTitledTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	protected string titleLocaKey;

	[SerializeField]
	protected bool overrideTitleColor;

	[SerializeField]
	protected Color titleColor = Color.white;

	[SerializeField]
	protected Sprite icon;

	[SerializeField]
	protected string descriptionLocaKey;

	protected bool HasFocus { get; private set; }

	public override void OnPointerEnter(PointerEventData eventData)
	{
		HasFocus = true;
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		HasFocus = false;
		base.OnPointerExit(eventData);
	}

	public override void DisplayTooltip()
	{
		GenericTitledTooltip genericTitledTooltip = (targetTooltip as GenericTitledTooltip) ?? UIManager.GenericTitledTooltip;
		genericTitledTooltip.SetContent(titleLocaKey, descriptionLocaKey, icon);
		if (overrideTitleColor)
		{
			genericTitledTooltip.SetTitleColor(titleColor);
		}
		genericTitledTooltip.FollowElement.ChangeTarget(base.transform);
		genericTitledTooltip.Display();
	}

	public override void HideTooltip()
	{
		((targetTooltip as GenericTitledTooltip) ?? UIManager.GenericTitledTooltip).Hide();
	}
}
