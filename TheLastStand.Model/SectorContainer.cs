using System.Collections.Generic;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.Model;

public class SectorContainer : MonoBehaviour
{
	[SerializeField]
	private List<CameraAreaOfInterest> sectors = new List<CameraAreaOfInterest>();

	public List<CameraAreaOfInterest> Sectors => sectors;
}
