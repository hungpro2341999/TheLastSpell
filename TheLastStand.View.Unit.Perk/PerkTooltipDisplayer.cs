using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Unit.Perk;

public class PerkTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	private UnitPerkDisplay unitPerkDisplay;

	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private RectTransform perkRectTransform;

	private float xOffset;

	public override void DisplayTooltip()
	{
		DisplayTooltip(display: true);
	}

	public override void HideTooltip()
	{
		DisplayTooltip(display: false);
	}

	public void DisplayTooltip(bool display)
	{
		if (!display || unitPerkDisplay.Perk != null || unitPerkDisplay.PerkDefinition != null)
		{
			PerkTooltip perkTooltip = PlayableUnitManager.PerkTooltip;
			if (display)
			{
				perkTooltip.SetContent(unitPerkDisplay.Perk);
				perkTooltip.Display();
				PlaceTooltip();
			}
			else
			{
				perkTooltip.Hide();
			}
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		UnitPerkTreeView.HoveredPerkTooltipDisplayer = this;
		if (unitPerkDisplay.Perk != null)
		{
			DisplayTooltip(display: true);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		if (UnitPerkTreeView.HoveredPerkTooltipDisplayer == this)
		{
			UnitPerkTreeView.HoveredPerkTooltipDisplayer = null;
		}
		if (unitPerkDisplay.Perk != null)
		{
			DisplayTooltip(display: false);
		}
	}

	private void Awake()
	{
		if (unitPerkDisplay == null)
		{
			unitPerkDisplay = GetComponent<UnitPerkDisplay>();
		}
		xOffset = followDatas.Offset.x;
	}

	private void PlaceTooltip()
	{
		PerkTooltip perkTooltip = PlayableUnitManager.PerkTooltip;
		float num = perkTooltip.TooltipPanel.rect.size.x;
		if (perkTooltip.CompendiumPanel.CompendiumEntries.Count > 0 && !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium)
		{
			num += perkTooltip.CompendiumPanel.RectTransform.rect.size.x;
		}
		num *= TPSingleton<SettingsManager>.Instance.Settings.UiSizeScale;
		bool flag = ACameraView.MainCam.ScreenToViewportPoint(new Vector3(perkRectTransform.position.x + num, 0f, 0f)).x <= 1f;
		followDatas.Offset = (flag ? new Vector3(xOffset, followDatas.Offset.y, followDatas.Offset.z) : new Vector3(0f - xOffset, followDatas.Offset.y, followDatas.Offset.z));
		perkTooltip.UpdateAnchors(flag);
		perkTooltip.FollowElement.ChangeFollowDatas(followDatas);
	}
}
