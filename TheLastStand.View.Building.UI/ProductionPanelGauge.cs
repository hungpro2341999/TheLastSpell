using System.Collections.Generic;
using DG.Tweening;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class ProductionPanelGauge : GraduatedGauge
{
	[SerializeField]
	private Transform unitsParent;

	[SerializeField]
	private RectTransform unitsBox;

	[SerializeField]
	private HorizontalLayoutGroup unitsLayoutGroup;

	[SerializeField]
	private List<Transform> unitsTransforms;

	private Dictionary<Transform, Image> unitsImages = new Dictionary<Transform, Image>();

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
			Transform transform = unitsTransforms[num];
			transform.gameObject.SetActive(value: true);
			if (tween)
			{
				unitsImages[transform].color = new Color(1f, 1f, 1f, 0f);
				unitsImages[transform].DOFade(1f, 0.15f).SetEase(Ease.InQuint);
				transform.localScale = Vector3.one * ((num == MaxUnits - 1) ? 7 : 3);
				transform.DOScale(1f, 0.2f).SetEase(Ease.InQuint);
			}
			else
			{
				unitsImages[transform].color = new Color(1f, 1f, 1f, 1f);
				transform.localScale = Vector3.one;
			}
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
		float x = ((unitsTransforms[0] as RectTransform).rect.width + unitsLayoutGroup.spacing) * (float)(count - MaxUnits);
		unitsBox.sizeDelta += new Vector2(x, 0f);
		if (MaxUnits < count)
		{
			while (MaxUnits < count)
			{
				Transform item = Object.Instantiate(unitsTransforms[0], unitsParent);
				unitsTransforms.Add(item);
			}
		}
		else
		{
			while (MaxUnits > count)
			{
				Transform transform = unitsTransforms[unitsTransforms.Count - 1];
				unitsTransforms.Remove(transform);
				Object.Destroy(transform.gameObject);
			}
		}
		RefreshUnitsImagesDictionary();
	}

	protected override void Awake()
	{
		base.Awake();
		RefreshUnitsImagesDictionary();
	}

	private void RefreshUnitsImagesDictionary()
	{
		unitsImages.Clear();
		for (int num = unitsTransforms.Count - 1; num >= 0; num--)
		{
			unitsImages.Add(unitsTransforms[num], unitsTransforms[num].GetComponent<Image>());
			if (unitsImages[unitsTransforms[num]] == null)
			{
				TPSingleton<BuildingManager>.Instance.LogError("ProductionUnitsGauge ERROR: There is no UnityEngine.UI.Image attached to " + unitsTransforms[num].name + "!", base.gameObject);
			}
		}
	}
}
