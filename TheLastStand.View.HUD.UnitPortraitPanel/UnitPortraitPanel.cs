using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitPortraitPanel;

public class UnitPortraitPanel : UnitPortraitView
{
	[SerializeField]
	private UnitPortraitStatGauge actionPointsGauge;

	[SerializeField]
	private UnitPortraitStatGauge movePointsGauge;

	[SerializeField]
	private UnitPortraitStatGauge manaPointsGauge;

	[SerializeField]
	private RectTransform containerRectTransform;

	[SerializeField]
	private Vector2 containerDisablePosition = new Vector2(0f, 14f);

	[SerializeField]
	private Image box;

	[SerializeField]
	private Sprite defaultBox;

	[SerializeField]
	private Sprite hoveredBox;

	public override void DisplayUnitPortraitBoxHovered(bool value)
	{
		box.sprite = (value ? hoveredBox : defaultBox);
		actionPointsGauge.Hover(value);
		movePointsGauge.Hover(value);
		manaPointsGauge.Hover(value);
		TPSingleton<TileObjectSelectionManager>.Instance.UpdateUnitInfoPanel(value ? PlayableUnit : null);
	}

	public void ToggleSkillTargeting(bool value)
	{
		if (value)
		{
			skillTargetingMark.OnShow?.Invoke();
		}
		else
		{
			skillTargetingMark.OnHide?.Invoke();
		}
	}

	public override void RefreshStats()
	{
		float clampedStatValue = PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPoints);
		float clampedStatValue2 = PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.MovePoints);
		actionPointsGauge.Refresh(clampedStatValue, PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ActionPointsTotal));
		movePointsGauge.Refresh(clampedStatValue2, PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.MovePointsTotal));
		manaPointsGauge.Refresh(PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana), PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.ManaTotal));
		if (clampedStatValue == 0f && clampedStatValue2 == 0f)
		{
			containerRectTransform.anchoredPosition = containerDisablePosition;
			Color color = Color.white * PlayableUnit.PortraitColor._Color.grayscale;
			color.a = 1f;
			unitPortraitBGImage.color = color;
		}
		else
		{
			containerRectTransform.anchoredPosition = Vector2.zero;
			unitPortraitBGImage.color = PlayableUnit.PortraitColor._Color;
		}
	}

	private void OnDestroy()
	{
		base.UnitPortraitToggle.OnPointerClickEvent.RemoveAllListeners();
		base.UnitPortraitToggle.OnPointerEnterEvent.RemoveAllListeners();
	}
}
