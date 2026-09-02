using TheLastStand.Model.Building.BuildingAction;

namespace TheLastStand.Controller.Building.BuildingAction;

public class BuildingActionExecutionController
{
	public BuildingActionExecution BuildingActionExecution { get; private set; }

	public BuildingActionExecutionController()
	{
		BuildingActionExecution = new BuildingActionExecution(this);
	}
}
