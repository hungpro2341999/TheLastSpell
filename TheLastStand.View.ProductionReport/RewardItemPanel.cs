using TPLib;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

[RequireComponent(typeof(Button))]
public class RewardItemPanel : MonoBehaviour
{
	public TheLastStand.Model.Item.Item Item { get; set; }

	public ProductionItems ProductionItem { get; set; }

	public void OnClick()
	{
		ProductionItem.ChosenItem = Item;
		ProductionItem.ProductionObjectController.ObtainContent();
		TPSingleton<ChooseRewardPanel>.Instance.Close();
		TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.RemoveProductionObject(ProductionItem);
		ProductionItem.ProductionObjectView.Disable();
	}
}
