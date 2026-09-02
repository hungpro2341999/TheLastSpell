using System.Collections.Generic;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingManagementPanelGauge : GraduatedGauge
{
	[SerializeField]
	private List<Transform> unitsTransforms;

	[SerializeField]
	private Transform unitsParent;

	[SerializeField]
	private HorizontalLayoutGroup unitsLayoutGroup;

	public override int MaxUnits => unitsTransforms.Count;

	public override void AddUnits(int amount, bool tween = true)
	{
		if (amount <= 0)
		{
			return;
		}
		int num = base.Units;
		for (int i = 0; i < amount; i++)
		{
			if (num > MaxUnits - 1)
			{
				if (!clearOnCapacityExceeded)
				{
					break;
				}
				Clear();
				num = 0;
			}
			unitsTransforms[num].gameObject.SetActive(value: true);
			num++;
		}
		unitsLayoutGroup.enabled = true;
		LayoutRebuilder.ForceRebuildLayoutImmediate(unitsParent as RectTransform);
		unitsLayoutGroup.enabled = false;
		base.Units = num;
	}

	public override void Clear()
	{
		for (int num = MaxUnits - 1; num >= 0; num--)
		{
			unitsTransforms[num].gameObject.SetActive(value: false);
		}
		base.Units = 0;
	}

	public void SetUnitsCount(int count)
	{
		if (MaxUnits == count)
		{
			return;
		}
		if (MaxUnits < count)
		{
			while (MaxUnits < count)
			{
				Transform item = Object.Instantiate(unitsTransforms[0], unitsParent);
				unitsTransforms.Add(item);
			}
			return;
		}
		while (MaxUnits > count)
		{
			Transform transform = unitsTransforms[unitsTransforms.Count - 1];
			unitsTransforms.Remove(transform);
			Object.Destroy(transform.gameObject);
		}
	}
}
