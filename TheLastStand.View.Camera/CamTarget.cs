using UnityEngine;

namespace TheLastStand.View.Camera;

public class CamTarget : MonoBehaviour
{
	public Transform TargetTransform { get; set; }

	private void Update()
	{
		if (!(TargetTransform == null))
		{
			base.transform.position = TargetTransform.position;
		}
	}
}
