using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.Building;
using TheLastStand.View.Building;

namespace TheLastStand.Controller.Building;

public class BuildingController
{
	public TheLastStand.Model.Building.Building Building { get; private set; }

	public BuildingView BuildingView => Building.BuildingView;

	public BlueprintModuleController BlueprintModuleController { get; private set; }

	public ConstructionModuleController ConstructionModuleController { get; private set; }

	public DamageableModuleController DamageableModuleController { get; private set; }

	public UpgradeModuleController UpgradeModuleController { get; private set; }

	public PassivesModuleController PassivesModuleController { get; private set; }

	public ProductionModuleController ProductionModuleController { get; private set; }

	public BattleModuleController BattleModuleController { get; private set; }

	public BuildingController(SerializedBuilding container, BuildingDefinition buildingDefinition, BuildingView buildingView, Tile tile, int saveVersion)
	{
		Building = ((buildingDefinition is MagicCircleDefinition) ? new MagicCircle(container, this, buildingView as MagicCircleView) : new TheLastStand.Model.Building.Building(container, this, buildingView)
		{
			OriginTile = tile
		});
		Building.Init(container);
		if (buildingView != null)
		{
			buildingView.name = Building.UniqueIdentifier;
		}
		ReferenceModules();
		DeserializeModules(container, saveVersion);
	}

	public BuildingController(BuildingDefinition buildingDefinition, BuildingView buildingView, Tile tile)
	{
		Building = ((buildingDefinition is MagicCircleDefinition magicCircleDefinition) ? new MagicCircle(magicCircleDefinition, this, buildingView as MagicCircleView, tile) : new TheLastStand.Model.Building.Building(buildingDefinition, this, buildingView, tile));
		Building.Init();
		if (buildingView != null)
		{
			buildingView.name = Building.UniqueIdentifier;
		}
		ReferenceModules();
		InitializeModules();
	}

	private void ReferenceModules()
	{
		BlueprintModuleController = Building.BlueprintModule.BlueprintModuleController;
		ConstructionModuleController = Building.ConstructionModule.ConstructionModuleController;
		DamageableModuleController = Building.DamageableModule?.DamageableModuleController;
		UpgradeModuleController = Building.UpgradeModule?.UpgradeModuleController;
		PassivesModuleController = Building.PassivesModule?.PassivesModuleController;
		ProductionModuleController = Building.ProductionModule?.ProductionModuleController;
		BattleModuleController = Building.BattleModule?.BattleModuleController;
	}

	private void InitializeModules()
	{
		if (ProductionModuleController != null)
		{
			ProductionModuleController.CreateGaugeEffect();
			ProductionModuleController.CreateActions();
		}
		PassivesModuleController?.CreatePassives();
		UpgradeModuleController?.CreateUpgrades();
		BattleModuleController?.CreateGoals();
		BattleModuleController?.HookToModifyingDamagePerks();
	}

	private void DeserializeModules(SerializedBuilding container, int saveVersion)
	{
		if (ProductionModuleController != null)
		{
			ProductionModuleController.CreateActions();
			ProductionModuleController.DeserializeGaugeEffect(container.GaugeEffect);
		}
		PassivesModuleController?.DeserializePassive(container.Passives, saveVersion);
		UpgradeModuleController?.DeserializeUpgrades(container.Upgrades);
		UpgradeModuleController?.DeserializeGlobalUpgrades(container.GlobalUpgrades);
		ProductionModuleController?.DeserializeUsedActions(container.Actions);
	}

	public void StartTurn()
	{
		BattleModuleController?.StartTurn();
		ProductionModuleController?.StartTurn();
		PassivesModuleController?.StartTurn();
	}

	public void EndTurn()
	{
		PassivesModuleController?.EndTurn();
	}
}
