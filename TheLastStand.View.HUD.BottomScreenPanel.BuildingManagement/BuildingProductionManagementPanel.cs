using TMPro;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingProductionManagementPanel : MonoBehaviour
{
	[SerializeField]
	private Image buildingProductionEffectRewardIconImage;

	[SerializeField]
	private TextMeshProUGUI buildingProductionEffectRewardText;

	public void Refresh(TheLastStand.Model.Building.Building building)
	{
		if (building.ProductionModule?.BuildingGaugeEffect == null || building is MagicCircle)
		{
			Disable();
			return;
		}
		base.gameObject.SetActive(value: true);
		buildingProductionEffectRewardIconImage.sprite = building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetProductionRewardIconSpriteBig();
		buildingProductionEffectRewardIconImage.enabled = buildingProductionEffectRewardIconImage.sprite != null;
		buildingProductionEffectRewardText.enabled = true;
		RefreshLocalizedTexts(building);
	}

	public void RefreshLocalizedTexts(TheLastStand.Model.Building.Building building)
	{
		if (building.ProductionModule?.BuildingGaugeEffect != null && !(building is MagicCircle))
		{
			buildingProductionEffectRewardText.text = building.ProductionModule.BuildingGaugeEffect.BuildingGaugeEffectView.GetEffectRewardString();
		}
	}

	private void Disable()
	{
		base.gameObject.SetActive(value: false);
	}
}
