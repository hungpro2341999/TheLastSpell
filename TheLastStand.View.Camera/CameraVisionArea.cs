using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraVisionArea : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D cameraBoxCollider;

	[SerializeField]
	[Range(0.1f, 1f)]
	private float ratio = 0.5f;

	private float camRefAspect = -1f;

	private float camRefOrthoSize = -1f;

	private float lastRatioUsed;

	public Vector2 ColliderSize => cameraBoxCollider.size;

	private void LateUpdate()
	{
		if (camRefOrthoSize != ACameraView.MainCam.orthographicSize || lastRatioUsed != ratio)
		{
			camRefOrthoSize = ACameraView.MainCam.orthographicSize;
			camRefAspect = ACameraView.MainCam.aspect;
			lastRatioUsed = ratio;
			Vector2 size = new Vector2(camRefOrthoSize * 2f * camRefAspect, camRefOrthoSize * 2f);
			size *= ratio;
			cameraBoxCollider.size = size;
		}
	}
}
