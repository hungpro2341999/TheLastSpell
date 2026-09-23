namespace TheLastStand.Definition.Building.BuildingAction;

/// <summary>
/// Quản lý trạng thái khả dụng của hành động công trình (Available, Disable, Hidden) theo từng giai đoạn trong game (Deployment, Night, Production).
/// </summary>
public class PhaseStates
{
	#region Enums

	/// <summary>
	/// Trạng thái khả dụng của hành động trong một giai đoạn.
	/// </summary>
	public enum E_PhaseState
	{
		Available,
		Disable,
		Hidden
	}

	#endregion

	#region Properties

	/// <summary>
	/// Trạng thái hành động trong giai đoạn Dàn trận (Deployment Phase).
	/// </summary>
	public E_PhaseState DeploymentState { get; set; }

	/// <summary>
	/// Trạng thái hành động trong giai đoạn Ban đêm / Phòng thủ (Night Phase).
	/// </summary>
	public E_PhaseState NightState { get; set; }

	/// <summary>
	/// Trạng thái hành động trong giai đoạn Sản xuất / Ban ngày (Production Phase).
	/// </summary>
	public E_PhaseState ProductionState { get; set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo trạng thái PhaseStates cho cả 3 giai đoạn chơi.
	/// </summary>
	public PhaseStates(E_PhaseState deploymentState, E_PhaseState nightState, E_PhaseState productionState)
	{
		DeploymentState = deploymentState;
		NightState = nightState;
		ProductionState = productionState;
	}

	#endregion
}

