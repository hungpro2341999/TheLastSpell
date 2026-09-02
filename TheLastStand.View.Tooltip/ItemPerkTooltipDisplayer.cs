using System.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Item;
using TheLastStand.Model.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.Unit.Perk;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class ItemPerkTooltipDisplayer : TooltipDisplayer
{
	[SerializeField]
	private FollowElement.FollowDatas followDatas = new FollowElement.FollowDatas();

	[SerializeField]
	private PerkTooltip perkTooltip;

	public TheLastStand.Model.Item.Item Item { get; private set; }

	public void ChangeItem(TheLastStand.Model.Item.Item newItem, PlayableUnit playableUnit)
	{
		Item = newItem;
		perkTooltip.SetContent(null);
		if (newItem == null || newItem.PerksId.Count <= 0 || !PlayableUnitDatabase.PerkDefinitions.TryGetValue(Item.PerksId.First(), out var value))
		{
			return;
		}
		if (playableUnit != null)
		{
			if (playableUnit.Perks.ContainsKey(value.Id))
			{
				perkTooltip.SetContent(playableUnit.Perks[value.Id]);
				return;
			}
			Item.Perks[value.Id].PerkController.ChangeOwner(playableUnit);
			perkTooltip.SetContent(Item.Perks[value.Id]);
		}
		else
		{
			perkTooltip.SetContent(null, value);
		}
	}

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
		if (display && (Item == null || Item.PerksId.Count == 0))
		{
			if (base.Displayed)
			{
				HideTooltip();
			}
			return;
		}
		PerkTooltip perkTooltip = GetPerkTooltip();
		if (display)
		{
			perkTooltip.Display();
			PlaceTooltip();
		}
		else
		{
			perkTooltip.Hide();
		}
	}

	private void PlaceTooltip()
	{
		PerkTooltip obj = GetPerkTooltip();
		obj.UpdateAnchors(displayTowardsRight: true, displayTop: true);
		obj.FollowElement.ChangeFollowDatas(followDatas);
	}

	private PerkTooltip GetPerkTooltip()
	{
		if (perkTooltip == null)
		{
			return PlayableUnitManager.PerkTooltip;
		}
		return perkTooltip;
	}
}
