using TPLib.Localization;
using TheLastStand.Model.Building.BuildingGaugeEffect;

namespace TheLastStand.View.Building.BuildingGaugeEffect;

public class GainGoldView : BuildingGaugeEffectView
{
	public GainGold GainGold => base.BuildingGaugeEffect as GainGold;

	public override string GetEffectRewardString()
	{
		return Localizer.Format("BuildingGaugeEffectReward_GainGold", base.BuildingGaugeEffect.GetProductionValue());
	}

	protected override string GetProductionRewardIconId()
	{
		return "Gold";
	}
}
