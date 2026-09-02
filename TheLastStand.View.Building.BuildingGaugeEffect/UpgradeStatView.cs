using TPLib;
using TPLib.Localization;
using TheLastStand.Manager.Building;

namespace TheLastStand.View.Building.BuildingGaugeEffect;

public class UpgradeStatView : BuildingGaugeEffectView
{
	private static class Constants
	{
		public const string Temple = "Temple";

		public const string ManaWell = "ManaWell";
	}

	public override string GetEffectRewardString()
	{
		string text = base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id switch
		{
			"Temple" => "Temple", 
			"ManaWell" => "ManaWell", 
			_ => string.Empty, 
		};
		if (string.IsNullOrEmpty(text))
		{
			TPSingleton<BuildingManager>.Instance.LogError("No production reward icon id found");
		}
		return Localizer.Format("BuildingGaugeEffectReward_" + text, base.BuildingGaugeEffect.GetProductionValue());
	}

	protected override string GetProductionRewardIconId()
	{
		return base.BuildingGaugeEffect.ProductionBuilding.BuildingParent.BuildingDefinition.Id switch
		{
			"Temple" => "Temple", 
			"ManaWell" => "ManaWell", 
			_ => string.Empty, 
		};
	}
}
