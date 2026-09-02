using TPLib.Localization;

namespace TheLastStand.View.Building.BuildingGaugeEffect;

public class OpenMagicSealView : BuildingGaugeEffectView
{
	public override string GetEffectRewardString()
	{
		return Localizer.Get("BuildingGaugeEffectReward_OpenMagicSeal");
	}

	protected override string GetProductionRewardIconId()
	{
		return "Seal";
	}
}
