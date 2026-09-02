using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;

namespace TheLastStand.Controller.Building.BuildingPassive;

public class CreateRosterItemController
{
	public CreateRosterItem CreateRosterItem { get; protected set; }

	public CreateRosterItemController(ProductionModule buildingProductionModule, CreateRosterItemDefinition createRosterItemDefinition)
	{
		CreateRosterItem = new CreateRosterItem(buildingProductionModule, createRosterItemDefinition, this);
	}
}
