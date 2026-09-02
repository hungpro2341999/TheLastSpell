using TheLastStand.Controller;
using TheLastStand.Controller.Building.BuildingPassive;
using TheLastStand.Database;
using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Model.Building.BuildingPassive;

public class CreateRosterItem
{
	public CreateRosterItemDefinition CreateRosterItemDefinition { get; private set; }

	public LevelProbabilitiesTreeController GenerationProbabilitiesTree { get; private set; }

	public CreateRosterItem(ProductionModule buildingProductionModule, CreateRosterItemDefinition createRosterItemDefinition, CreateRosterItemController createRosterItemController)
	{
		CreateRosterItemDefinition = createRosterItemDefinition;
		GenerationProbabilitiesTree = new LevelProbabilitiesTreeController(buildingProductionModule, ItemDatabase.ItemGenerationModifierListDefinitions[createRosterItemDefinition.BuildingLevelModifiersListId]);
	}
}
