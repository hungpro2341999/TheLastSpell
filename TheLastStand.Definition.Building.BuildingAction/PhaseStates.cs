namespace TheLastStand.Definition.Building.BuildingAction;

public class PhaseStates
{
	public enum E_PhaseState
	{
		Available,
		Disable,
		Hidden
	}

	public E_PhaseState DeploymentState { get; set; }

	public E_PhaseState NightState { get; set; }

	public E_PhaseState ProductionState { get; set; }

	public PhaseStates(E_PhaseState deploymentState, E_PhaseState nightState, E_PhaseState productionState)
	{
		DeploymentState = deploymentState;
		NightState = nightState;
		ProductionState = productionState;
	}
}
