using UnityEngine;

namespace TheLastStand.View.Building;

public class WatchtowerView : BuildingView
{
	[SerializeField]
	private Transform towerHeight;

	public Transform TowerHeight => towerHeight;
}
