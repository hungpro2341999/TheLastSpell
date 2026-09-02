using TPLib.Log;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Unit.Trait;

public class TraitTooltipDisplayer : TooltipDisplayer, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Tooltip("If null, will try to automatically find a UnitTraitView its gameObject")]
	[SerializeField]
	private UnitTraitDisplay unitTraitDisplay;

	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	public TraitTooltip TraitTooltip => targetTooltip as TraitTooltip;

	public override void DisplayTooltip()
	{
		DisplayTooltip(display: true);
	}

	public void DisplayTooltip(bool display)
	{
		if (unitTraitDisplay.UnitTraitDefinition == null)
		{
			CLoggerManager.Log("UnitTraitDefinition can't be null! Aborting...", this, LogType.Error);
			base.enabled = false;
			return;
		}
		if (targetTooltip == null)
		{
			targetTooltip = PlayableUnitManager.TraitTooltip;
		}
		if (display)
		{
			TraitTooltip.FollowElement.ChangeFollowDatas(followDatas);
			TraitTooltip.SetContent(unitTraitDisplay.UnitTraitDefinition);
			TraitTooltip.Display();
		}
		else
		{
			TraitTooltip.Hide();
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		DisplayTooltip(display: true);
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		DisplayTooltip(display: false);
	}

	private void Awake()
	{
		if (unitTraitDisplay == null)
		{
			unitTraitDisplay = GetComponent<UnitTraitDisplay>();
		}
	}
}
