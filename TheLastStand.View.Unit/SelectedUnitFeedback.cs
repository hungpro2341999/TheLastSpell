using TPLib;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.View.Unit;

public class SelectedUnitFeedback : MonoBehaviour
{
	[SerializeField]
	private Color playableUnitColorFeedback;

	[SerializeField]
	private Color enemyUnitColorFeedback;

	[SerializeField]
	private FollowPosition followPosition;

	[SerializeField]
	private SpriteRenderer groundFxRenderer;

	[SerializeField]
	private SpriteRenderer haloFxRenderer;

	[SerializeField]
	private Transform feedbackTransform;

	private TheLastStand.Model.Unit.Unit unit;

	private int? groundFxRendererInitOrder;

	private int? haloFxRendererInitOrder;

	public TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			unit = value;
			Refresh();
		}
	}

	public void Display(bool display)
	{
		if (display)
		{
			followPosition.SetPosition();
		}
		followPosition.gameObject.SetActive(display);
	}

	public void Refresh()
	{
		followPosition.Target = this.unit.UnitView.transform;
		int valueOrDefault = groundFxRendererInitOrder.GetValueOrDefault();
		if (!groundFxRendererInitOrder.HasValue)
		{
			valueOrDefault = groundFxRenderer.sortingOrder;
			groundFxRendererInitOrder = valueOrDefault;
		}
		valueOrDefault = haloFxRendererInitOrder.GetValueOrDefault();
		if (!haloFxRendererInitOrder.HasValue)
		{
			valueOrDefault = haloFxRenderer.sortingOrder;
			haloFxRendererInitOrder = valueOrDefault;
		}
		TheLastStand.Model.Unit.Unit unit = this.unit;
		if (!(unit is PlayableUnit playableUnit))
		{
			if (unit is EnemyUnit enemyUnit)
			{
				int? sortingOrderOverride = enemyUnit.EnemyUnitTemplateDefinition.SortingOrderOverride;
				if (sortingOrderOverride.HasValue)
				{
					groundFxRenderer.sortingOrder = groundFxRendererInitOrder.Value + sortingOrderOverride.Value - 30;
					haloFxRenderer.sortingOrder = haloFxRendererInitOrder.Value + sortingOrderOverride.Value - 30;
				}
				else
				{
					groundFxRenderer.sortingOrder = groundFxRendererInitOrder.Value;
					haloFxRenderer.sortingOrder = haloFxRendererInitOrder.Value;
				}
				groundFxRenderer.color = enemyUnitColorFeedback;
				haloFxRenderer.color = enemyUnitColorFeedback;
			}
		}
		else
		{
			groundFxRenderer.sortingOrder = groundFxRendererInitOrder.Value;
			haloFxRenderer.sortingOrder = haloFxRendererInitOrder.Value;
			groundFxRenderer.color = playableUnit.PortraitColor._Color;
			haloFxRenderer.color = playableUnitColorFeedback;
		}
		feedbackTransform.localScale = this.unit.UnitTemplateDefinition.SelectedFeedbackSize;
	}
}
