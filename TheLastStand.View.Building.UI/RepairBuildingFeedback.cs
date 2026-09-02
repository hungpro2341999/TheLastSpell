using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.View.Building.Construction;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class RepairBuildingFeedback : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI repairTipText;

	public static bool IsDirty { get; set; }

	private void Awake()
	{
		Toggle(toggle: false);
	}

	private void Update()
	{
		if (repairTipText.enabled && TPSingleton<ConstructionManager>.Instance.Construction.State == TheLastStand.Model.Building.Construction.E_State.None)
		{
			repairTipText.enabled = false;
		}
		else
		{
			if (!TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged && !IsDirty)
			{
				return;
			}
			Tile previousTile = TPSingleton<GameManager>.Instance.Game.Cursor.PreviousTile;
			Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
			if (previousTile != null && tile == null)
			{
				repairTipText.enabled = false;
			}
			if (TPSingleton<ConstructionView>.Instance.HoveredRepairCategoryButton == null && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction)
			{
				ConstructionView.ClearRepairTilesFeedback();
			}
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction && TPSingleton<ConstructionManager>.Instance.Construction.State == TheLastStand.Model.Building.Construction.E_State.Repair)
			{
				if (TPSingleton<ConstructionManager>.Instance.Construction.RepairMode != TheLastStand.Model.Building.Construction.E_RepairMode.None)
				{
					if (tile?.Building != null)
					{
						if (tile.Building.ConstructionModule.ConstructionModuleDefinition.IsRepairable)
						{
							switch (TPSingleton<ConstructionManager>.Instance.Construction.RepairMode)
							{
							case TheLastStand.Model.Building.Construction.E_RepairMode.Target:
								UpdateForRepairTarget(tile.Building);
								break;
							case TheLastStand.Model.Building.Construction.E_RepairMode.Id:
								UpdateForRepairId(tile.Building);
								break;
							}
						}
						else
						{
							repairTipText.text = Localizer.Format("ConstructionPanel_NotRepairable", tile.Building.BuildingDefinition.Name);
						}
						repairTipText.enabled = true;
					}
					else
					{
						repairTipText.enabled = false;
					}
				}
				else
				{
					repairTipText.enabled = false;
				}
			}
			IsDirty = false;
		}
	}

	private void UpdateForRepairTarget(TheLastStand.Model.Building.Building building)
	{
		if (building.ConstructionModule.NeedRepair)
		{
			if (building.ConstructionModule.CostsGold)
			{
				int repairCost = building.ConstructionModule.RepairCost;
				repairTipText.text = (ConstructionManager.CanRepairBuilding(building.ConstructionModule) ? Localizer.Format("ConstructionPanel_RepairTipGold", building.BuildingDefinition.Name, repairCost) : Localizer.Format("ConstructionPanel_CantRepairTipGold", building.BuildingDefinition.Name, repairCost));
			}
			else if (building.ConstructionModule.CostsMaterials)
			{
				int repairCost2 = building.ConstructionModule.RepairCost;
				repairTipText.text = (ConstructionManager.CanRepairBuilding(building.ConstructionModule) ? Localizer.Format("ConstructionPanel_RepairTipMaterial", building.BuildingDefinition.Name, repairCost2) : Localizer.Format("ConstructionPanel_CantRepairTipMaterial", building.BuildingDefinition.Name, repairCost2));
			}
		}
		else
		{
			repairTipText.text = Localizer.Format("ConstructionPanel_CantRepairNotDamaged", building.BuildingDefinition.Name);
		}
	}

	private void UpdateForRepairId(TheLastStand.Model.Building.Building building)
	{
		List<TheLastStand.Model.Building.Building> buildingsById = BuildingManager.GetBuildingsById(building.BuildingDefinition.Id);
		int num = 0;
		int num2 = 0;
		foreach (TheLastStand.Model.Building.Building item in buildingsById)
		{
			if (item.ConstructionModule.NeedRepair)
			{
				num2++;
				num += item.ConstructionModule.RepairCost;
			}
		}
		if (num == 0 && num2 == 0)
		{
			repairTipText.text = Localizer.Format("ConstructionPanel_CantRepairNotDamaged", building.BuildingDefinition.Name);
			return;
		}
		if (building.ConstructionModule.CostsGold)
		{
			repairTipText.text = ((TPSingleton<ResourceManager>.Instance.Gold >= num) ? Localizer.Format("ConstructionPanel_RepairAllTipGold", building.BuildingDefinition.Name, num2, num) : Localizer.Format("ConstructionPanel_CantRepairAllTipGold", building.BuildingDefinition.Name, num2, num));
		}
		else if (building.ConstructionModule.CostsMaterials)
		{
			repairTipText.text = ((TPSingleton<ResourceManager>.Instance.Materials >= num) ? Localizer.Format("ConstructionPanel_RepairAllTipMaterial", building.BuildingDefinition.Name, num2, num) : Localizer.Format("ConstructionPanel_CantRepairAllTipMaterial", building.BuildingDefinition.Name, num2, num));
		}
		foreach (TheLastStand.Model.Building.Building item2 in buildingsById)
		{
			if (item2.ConstructionModule.NeedRepair)
			{
				ConstructionView.DisplayTilesFeedback(item2);
			}
		}
	}

	private void Toggle(bool toggle)
	{
		repairTipText.enabled = toggle;
	}
}
