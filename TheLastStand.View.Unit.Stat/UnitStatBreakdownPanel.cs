using System;
using TMPro;
using TPLib.Localization;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Stat;
using UnityEngine;

namespace TheLastStand.View.Unit.Stat;

public class UnitStatBreakdownPanel : MonoBehaviour
{
	[SerializeField]
	private UnitStatBreakdownDisplay unitStatBaseValue;

	[SerializeField]
	private UnitStatBreakdownDisplay unitStatRaceValue;

	[SerializeField]
	private UnitStatBreakdownDisplay unitStatTraitsValue;

	[SerializeField]
	private UnitStatBreakdownDisplay unitStatEquipmentValue;

	[SerializeField]
	private UnitStatBreakdownDisplay unitStatPerksValue;

	[SerializeField]
	private UnitStatBreakdownDisplay unitStatStatusValue;

	[SerializeField]
	private UnitStatBreakdownDisplay unitStatInjuriesValue;

	[SerializeField]
	private UnitStatBreakdownDisplay unitStatApocalypseValue;

	[SerializeField]
	[Tooltip("Used to show stat result value when it's shown as addition of all bonuses (ex: Stat tooltip)")]
	private UnitStatBreakdownDisplay unitStatBreakdownFinalValue;

	[SerializeField]
	private TextMeshProUGUI unitStatCapValues;

	protected float baseValue;

	private float raceValue;

	private float traitsValue;

	private float equipmentValue;

	private float perksValue;

	private float statusValue;

	private float injuriesValue;

	private float injuriesMultiplierLoss;

	private float finalValue;

	private float apocalypseValue;

	public TheLastStand.Model.Unit.Unit TargetUnit { get; set; }

	public UnitStatDefinition UnitStatDefinition { get; set; }

	public void Refresh(UnitStatDefinition unitStatDefinition, TheLastStand.Model.Unit.Unit unit)
	{
		UnitStatDefinition = unitStatDefinition;
		TargetUnit = unit;
		SetValues();
		bool statIsPercentage = UnitStatDefinition.Id.ShownAsPercentage();
		bool showValue = unitStatBreakdownFinalValue == null || raceValue != 0f || traitsValue != 0f || equipmentValue != 0f || perksValue != 0f || statusValue != 0f || injuriesValue != 0f;
		if (unitStatBaseValue != null)
		{
			unitStatBaseValue.Refresh(unitStatBaseValue.FormatStatValue(baseValue, statIsPercentage), "UnitStatTooltip_BaseValue", showValue);
		}
		unitStatRaceValue?.Refresh(unitStatBaseValue.FormatStatValue(raceValue, statIsPercentage), "UnitStatTooltip_RaceValue", raceValue != 0f);
		unitStatTraitsValue?.Refresh(unitStatBaseValue.FormatStatValue(traitsValue, statIsPercentage), "UnitStatTooltip_TraitsValue", traitsValue != 0f);
		unitStatEquipmentValue?.Refresh(unitStatBaseValue.FormatStatValue(equipmentValue, statIsPercentage), "UnitStatTooltip_EquipmentValue", equipmentValue != 0f);
		unitStatPerksValue?.Refresh(unitStatBaseValue.FormatStatValue(perksValue, statIsPercentage), "UnitStatTooltip_PerksValue", perksValue != 0f);
		unitStatStatusValue?.Refresh(unitStatBaseValue.FormatStatValue(statusValue, statIsPercentage), "UnitStatTooltip_StatusValue", statusValue != 0f);
		unitStatApocalypseValue?.Refresh(unitStatBaseValue.FormatStatValue(apocalypseValue, statIsPercentage), "WorldMap_ApocalypseDifficulty_Apocalypse", apocalypseValue != 0f);
		if (injuriesMultiplierLoss != 0f)
		{
			unitStatInjuriesValue?.Refresh(unitStatBaseValue.FormatStatValue(injuriesMultiplierLoss, statIsPercentage), "UnitStatTooltip_InjuriesValue");
		}
		else
		{
			unitStatInjuriesValue?.Refresh(unitStatBaseValue.FormatStatValue(injuriesValue, statIsPercentage), "UnitStatTooltip_InjuriesValue", injuriesValue != 0f);
		}
		string locaKeyOverride = ((UnitStatDefinition.Id == UnitStatDefinition.E_Stat.HealthTotal && !TargetUnit.CanBeDamaged()) ? "UnitStatTooltip_InfiniteValue" : null);
		unitStatBreakdownFinalValue?.Refresh(unitStatBaseValue.FormatStatValue(finalValue, statIsPercentage, locaKeyOverride), "UnitStatTooltip_TotalValue");
		if (TargetUnit is PlayableUnit)
		{
			unitStatCapValues.enabled = true;
			string text = (UnitStatDefinition.Id.ShownAsPercentage() ? "%" : string.Empty);
			Vector2 startingBoundaries = TargetUnit.UnitStatsController.GetStat(UnitStatDefinition.Id).StartingBoundaries;
			Vector2 boundaries = TargetUnit.UnitStatsController.GetStat(UnitStatDefinition.Id).Boundaries;
			string contentToFormat = boundaries.x + text;
			string contentToFormat2 = boundaries.y + text;
			contentToFormat = FormatStatBoundariesValues(startingBoundaries.x, boundaries.x, contentToFormat);
			contentToFormat2 = FormatStatBoundariesValues(startingBoundaries.y, boundaries.y, contentToFormat2);
			unitStatCapValues.text = ((UnitStatDefinition.Boundaries[TargetUnit.UnitTemplateDefinition.UnitType].x != 0f) ? Localizer.Format("UnitStatTooltip_MinAndMaxCapValues", contentToFormat, contentToFormat2) : Localizer.Format("UnitStatTooltip_MaxCapValue", contentToFormat2));
		}
		else
		{
			unitStatCapValues.enabled = false;
		}
	}

	private void SetValues()
	{
		UnitStat stat = TargetUnit.UnitStatsController.GetStat(UnitStatDefinition.Id);
		finalValue = Mathf.Floor(stat.FinalClamped);
		baseValue = Mathf.Floor(stat.Base);
		statusValue = Mathf.Floor(stat.Status + stat.InjuriesStatuses);
		injuriesValue = Mathf.Floor(stat.Injuries);
		injuriesMultiplierLoss = Mathf.Floor(stat.InjuryMultiplierLoss);
		apocalypseValue = 0f;
		if (stat is PlayableUnitStat playableUnitStat)
		{
			raceValue = Mathf.Floor(playableUnitStat.Race);
			traitsValue = Mathf.Floor(playableUnitStat.Traits);
			equipmentValue = Mathf.Floor(playableUnitStat.Equipment);
			perksValue = Mathf.Floor(playableUnitStat.Perks);
			return;
		}
		raceValue = 0f;
		traitsValue = 0f;
		equipmentValue = 0f;
		perksValue = 0f;
		if (stat is EnemyUnitStat enemyUnitStat)
		{
			apocalypseValue = Mathf.Floor(enemyUnitStat.Apocalypse);
		}
	}

	private string FormatStatBoundariesValues(float boundaryBaseValue, float boundaryFinalValue, string contentToFormat)
	{
		if (Math.Abs(boundaryBaseValue - boundaryFinalValue) <= Mathf.Epsilon)
		{
			return contentToFormat;
		}
		return "<color=#" + ColorUtility.ToHtmlStringRGBA((boundaryBaseValue < boundaryFinalValue) ? GameView.PositiveColor : GameView.NegativeColor) + ">" + contentToFormat + "</color>";
	}
}
