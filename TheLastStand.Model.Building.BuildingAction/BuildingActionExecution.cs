using TheLastStand.Controller.Building.BuildingAction;

namespace TheLastStand.Model.Building.BuildingAction;

public class BuildingActionExecution
{
	public BuildingActionExecutionController BuildingActionExecutionController { get; private set; }

	public BuildingActionExecution(BuildingActionExecutionController controller)
	{
		BuildingActionExecutionController = controller;
	}
}
