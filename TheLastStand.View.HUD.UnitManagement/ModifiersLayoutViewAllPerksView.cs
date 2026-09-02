using System.Collections.Generic;
using System.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class ModifiersLayoutViewAllPerksView : MonoBehaviour
{
	[SerializeField]
	private ModifiersLayoutViewPerksView perksTreeView;

	[SerializeField]
	private ModifiersLayoutViewPerksView perksItemView;

	[SerializeField]
	private ModifiersLayoutViewPerksView perksOmenView;

	[SerializeField]
	private List<ModifiersLayoutViewPerksView> perksViews = new List<ModifiersLayoutViewPerksView>();

	public ModifiersLayoutViewPerksView PerksTreeView => perksTreeView;

	public ModifiersLayoutViewPerksView PerksItemView => perksItemView;

	public ModifiersLayoutViewPerksView PerksOmenView => perksOmenView;

	public bool IsDisplayed()
	{
		if (!perksTreeView.IsDisplayed() && !perksItemView.IsDisplayed())
		{
			return perksOmenView.IsDisplayed();
		}
		return true;
	}

	public List<Selectable> GetAllSelectables()
	{
		List<Selectable> list = new List<Selectable>();
		foreach (ModifiersLayoutViewPerksView perksView in perksViews)
		{
			list.AddRange(perksView.GetSelectables());
		}
		return list;
	}

	public bool HasTwoPerksLines()
	{
		if (PerksTreeView.IsDisplayed() && (PerksItemView.IsDisplayed() || perksOmenView.IsDisplayed()))
		{
			return true;
		}
		return false;
	}

	public void HideAllPerkDisplays()
	{
		for (int i = 0; i < perksViews.Count; i++)
		{
			perksViews[i].HideAllPerkDisplays();
		}
	}

	public PerkIconDisplay GetBottomLeftPerkIconDisplay()
	{
		if (perksTreeView.IsDisplayed())
		{
			return perksTreeView.PerkIconDisplays.FirstOrDefault();
		}
		if (perksItemView.IsDisplayed())
		{
			return perksItemView.PerkIconDisplays.FirstOrDefault();
		}
		if (perksOmenView.IsDisplayed())
		{
			return perksOmenView.PerkIconDisplays.FirstOrDefault();
		}
		return null;
	}

	public PerkIconDisplay GetFarRightPerkIconDisplay()
	{
		PerkIconDisplay perkIconDisplay = null;
		for (int i = 0; i < perksViews.Count; i++)
		{
			PerkIconDisplay farRightPerkIconDisplay = perksViews[i].GetFarRightPerkIconDisplay();
			if (farRightPerkIconDisplay != null && (perkIconDisplay == null || farRightPerkIconDisplay.transform.position.x > perkIconDisplay.transform.position.x))
			{
				perkIconDisplay = farRightPerkIconDisplay;
			}
		}
		return perkIconDisplay;
	}

	public void RefreshJoystickNavigation()
	{
		foreach (ModifiersLayoutViewPerksView perksView in perksViews)
		{
			perksView.RefreshJoystickNavigation();
		}
		bool num = perksTreeView.IsDisplayed();
		bool flag = perksItemView.IsDisplayed();
		bool flag2 = perksOmenView.IsDisplayed();
		List<Selectable> list = new List<Selectable>();
		List<Selectable> list2 = new List<Selectable>();
		List<Selectable> list3 = new List<Selectable>();
		Selectable selectable = null;
		if (num)
		{
			list = perksTreeView.GetSelectables();
		}
		if (flag)
		{
			list2 = perksItemView.GetSelectables();
			selectable = list2.FirstOrDefault();
		}
		if (flag2)
		{
			list3 = perksOmenView.GetSelectables();
			if (selectable == null)
			{
				selectable = list3.FirstOrDefault();
			}
		}
		if (flag && flag2)
		{
			list2.LastOrDefault()?.SetSelectOnRight(list3.FirstOrDefault());
			list3.FirstOrDefault()?.SetSelectOnLeft(list2.LastOrDefault());
		}
		if (!HasTwoPerksLines())
		{
			return;
		}
		Selectable selectOnDown = list.FirstOrDefault();
		foreach (Selectable item in list)
		{
			item.SetSelectOnUp(selectable);
		}
		foreach (Selectable item2 in list2)
		{
			item2.SetSelectOnDown(selectOnDown);
		}
		foreach (Selectable item3 in list3)
		{
			item3.SetSelectOnDown(selectOnDown);
		}
	}
}
