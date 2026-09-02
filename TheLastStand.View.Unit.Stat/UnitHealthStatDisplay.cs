using TheLastStand.Definition.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Stat;

public class UnitHealthStatDisplay : UnitStatWithRegenDisplay
{
	[SerializeField]
	private Image poisonGaugeImage;

	protected override void RefreshGaugeValue()
	{
		base.RefreshGaugeValue();
		poisonGaugeImage.fillAmount = gaugeImage.fillAmount;
		float num = base.TargetUnit.GetNextTurnPoisonDamage() / base.TargetUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.HealthTotal);
		gaugeImage.fillAmount = Mathf.Max(poisonGaugeImage.fillAmount - num, 0f);
	}
}
