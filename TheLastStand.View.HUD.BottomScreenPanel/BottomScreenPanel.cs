using TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;
using TheLastStand.View.HUD.BottomScreenPanel.GroundManagement;
using UnityEngine;

namespace TheLastStand.View.HUD.BottomScreenPanel;

public class BottomScreenPanel : MonoBehaviour
{
	[SerializeField]
	private BottomLeftPanel bottomLeftPanel;

	[SerializeField]
	private BuildingManagementPanel buildingManagementPanel;

	[SerializeField]
	private GroundManagementPanel groundManagementPanel;

	public BottomLeftPanel BottomLeftPanel => bottomLeftPanel;

	public BuildingManagementPanel BuildingManagementPanel => buildingManagementPanel;

	public GroundManagementPanel GroundManagementPanel => groundManagementPanel;

	public void Refresh()
	{
		bottomLeftPanel.Refresh();
	}
}
