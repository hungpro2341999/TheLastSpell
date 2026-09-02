using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Lib.Scripts.UI;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class LayoutNavigationInitializer : MonoBehaviour
{
	private struct Edge
	{
		public bool Left;

		public bool Right;

		public bool Top;

		public bool Bottom;
	}

	private struct GridInfo
	{
		public List<Selectable> Selectables;

		public Vector2Int GridSize;

		public bool IsHorizontal;

		public int ChildCount;
	}

	[SerializeField]
	private bool initOnStart = true;

	private LayoutGroup layoutGroup;

	[ContextMenu("Init Navigation")]
	public void InitNavigation(bool reset = false)
	{
		if (this.layoutGroup == null && !TryGetComponent<LayoutGroup>(out this.layoutGroup))
		{
			TPSingleton<UIManager>.Instance.LogWarning("No layout found on " + base.transform.name + "!");
			return;
		}
		LayoutGroup layoutGroup = this.layoutGroup;
		if (!(layoutGroup is GridLayoutGroup gridLayoutGroup))
		{
			if (!(layoutGroup is HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup))
			{
				if (layoutGroup is AlternatingGridLayoutGroup alternatingGridLayoutGroup)
				{
					InitAlternatingGridNavigation(alternatingGridLayoutGroup, reset);
				}
				else
				{
					TPSingleton<UIManager>.Instance.LogWarning("Unhandled layout type " + this.layoutGroup.GetType().Name + "!");
				}
			}
			else
			{
				InitHorizontalOrVerticalNavigation(horizontalOrVerticalLayoutGroup, reset);
			}
		}
		else
		{
			InitGridNavigation(gridLayoutGroup, reset);
		}
	}

	private void InitGridNavigation(GridLayoutGroup gridLayoutGroup, bool reset)
	{
		GridInfo gridInfo = default(GridInfo);
		Transform obj = gridLayoutGroup.transform;
		gridInfo.GridSize = gridLayoutGroup.GetColumnAndRow();
		gridInfo.IsHorizontal = gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal;
		gridInfo.Selectables = new List<Selectable>();
		foreach (Transform item in obj)
		{
			if (item.TryGetComponent<Selectable>(out var component))
			{
				if (reset)
				{
					component.ClearNavigation();
				}
				if (item.gameObject.activeInHierarchy)
				{
					gridInfo.Selectables.Add(component);
				}
			}
		}
		gridInfo.ChildCount = gridInfo.Selectables.Count;
		for (int i = 0; i < gridInfo.ChildCount; i++)
		{
			Selectable selectable = gridInfo.Selectables[i];
			selectable.SetMode(Navigation.Mode.Explicit);
			SetGridEdgeSelection(gridInfo, selectable, i);
		}
	}

	private void InitHorizontalOrVerticalNavigation(HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup, bool reset)
	{
		Transform obj = horizontalOrVerticalLayoutGroup.transform;
		List<Selectable> list = new List<Selectable>();
		foreach (Transform item in obj)
		{
			if (item.TryGetComponent<Selectable>(out var component))
			{
				if (reset)
				{
					component.ClearNavigation();
				}
				if (item.gameObject.activeInHierarchy)
				{
					list.Add(component);
				}
			}
		}
		bool flag = horizontalOrVerticalLayoutGroup is VerticalLayoutGroup;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Selectable selectable = list[i];
			selectable.SetMode(Navigation.Mode.Explicit);
			if (i > 0)
			{
				if (flag)
				{
					selectable.SetSelectOnUp(list[i - 1]);
				}
				else
				{
					selectable.SetSelectOnLeft(list[i - 1]);
				}
			}
			if (i < count - 1)
			{
				if (flag)
				{
					selectable.SetSelectOnDown(list[i + 1]);
				}
				else
				{
					selectable.SetSelectOnRight(list[i + 1]);
				}
			}
		}
	}

	private void InitAlternatingGridNavigation(AlternatingGridLayoutGroup alternatingGridLayoutGroup, bool reset)
	{
		Transform obj = alternatingGridLayoutGroup.transform;
		Vector2Int columnAndRow = alternatingGridLayoutGroup.GetColumnAndRow();
		List<Selectable> list = new List<Selectable>();
		foreach (Transform item in obj)
		{
			if (item.gameObject.activeInHierarchy && item.TryGetComponent<Selectable>(out var component))
			{
				if (reset)
				{
					component.ClearNavigation();
				}
				list.Add(component);
				component.SetMode(Navigation.Mode.Explicit);
			}
		}
		for (int i = 0; i < columnAndRow.y; i++)
		{
			int num = Mathf.CeilToInt((float)i / 2f) * columnAndRow.x + Mathf.FloorToInt((float)i / 2f) * (columnAndRow.x - 1);
			int num2 = Mathf.CeilToInt((float)(i + 1) / 2f) * columnAndRow.x + Mathf.FloorToInt((float)(i + 1) / 2f) * (columnAndRow.x - 1) - 1;
			for (int j = 0; j <= num2 - num; j++)
			{
				int num3 = j + num;
				if (list.Count <= num3)
				{
					continue;
				}
				Selectable selectable = list[num3];
				if (j > 0 && list.Count > num3 - 1)
				{
					selectable.SetSelectOnLeft(list[num3 - 1]);
				}
				if (j < num2 - num && list.Count > num3 + 1)
				{
					selectable.SetSelectOnRight(list[num3 + 1]);
				}
				if (i < columnAndRow.y - 1)
				{
					if (i % 2 == 0)
					{
						int num4 = ((j == num2 - num) ? (num3 + columnAndRow.x - 1) : (num3 + columnAndRow.x));
						if (list.Count > num4)
						{
							selectable.SetSelectOnDown(list[num4]);
						}
					}
					else if (list.Count > num3 + columnAndRow.x - 1)
					{
						selectable.SetSelectOnDown(list[num3 + columnAndRow.x - 1]);
					}
				}
				if (i <= 0)
				{
					continue;
				}
				if (i % 2 == 0)
				{
					int num5 = num3 - columnAndRow.x + 1;
					if (list.Count > num5)
					{
						selectable.SetSelectOnUp(list[num5]);
					}
				}
				else if (list.Count > num3 - columnAndRow.x)
				{
					selectable.SetSelectOnUp(list[num3 - columnAndRow.x]);
				}
			}
		}
	}

	private void SetGridEdgeSelection(GridInfo gridInfo, Selectable selectable, int value)
	{
		if (gridInfo.IsHorizontal)
		{
			Edge edge = new Edge
			{
				Left = (value % gridInfo.GridSize.x == 0),
				Right = ((value + 1) % gridInfo.GridSize.x == 0),
				Top = (value < gridInfo.GridSize.x),
				Bottom = (value >= gridInfo.ChildCount - gridInfo.GridSize.x)
			};
			if (gridInfo.GridSize.x > 1)
			{
				if (!edge.Left)
				{
					selectable.SetSelectOnLeft(gridInfo.Selectables[value - 1]);
				}
				if (!edge.Right && value + 1 < gridInfo.ChildCount)
				{
					selectable.SetSelectOnRight(gridInfo.Selectables[value + 1]);
				}
			}
			if (gridInfo.GridSize.y > 1)
			{
				if (!edge.Top)
				{
					selectable.SetSelectOnUp(gridInfo.Selectables[value - gridInfo.GridSize.x]);
				}
				if (!edge.Bottom && value + gridInfo.GridSize.x < gridInfo.ChildCount)
				{
					selectable.SetSelectOnDown(gridInfo.Selectables[value + gridInfo.GridSize.x]);
				}
			}
			return;
		}
		Edge edge2 = new Edge
		{
			Left = (value < gridInfo.GridSize.y),
			Right = (value >= gridInfo.ChildCount - gridInfo.GridSize.y),
			Top = (value % gridInfo.GridSize.y == 0),
			Bottom = ((value + 1) % gridInfo.GridSize.y == 0)
		};
		if (gridInfo.GridSize.x > 1)
		{
			if (!edge2.Left)
			{
				selectable.SetSelectOnLeft(gridInfo.Selectables[value - gridInfo.GridSize.y]);
			}
			if (!edge2.Right && value + gridInfo.GridSize.y < gridInfo.ChildCount)
			{
				selectable.SetSelectOnRight(gridInfo.Selectables[value + gridInfo.GridSize.y]);
			}
		}
		if (gridInfo.GridSize.y > 1)
		{
			if (!edge2.Top)
			{
				selectable.SetSelectOnUp(gridInfo.Selectables[value - 1]);
			}
			if (!edge2.Bottom && value + 1 < gridInfo.ChildCount)
			{
				selectable.SetSelectOnDown(gridInfo.Selectables[value + 1]);
			}
		}
	}

	private IEnumerator Start()
	{
		if (initOnStart)
		{
			yield return null;
			InitNavigation();
		}
	}
}
