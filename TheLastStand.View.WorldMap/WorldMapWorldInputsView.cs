using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.WorldMap;

public class WorldMapWorldInputsView : WorldInputsView
{
	public override void LateUpdate()
	{
		if (camRefOrthoSize != ACameraView.MainCam.orthographicSize)
		{
			camRefOrthoSize = ACameraView.MainCam.orthographicSize;
			camRefAspect = ACameraView.MainCam.aspect;
			Vector2 size = new Vector2(camRefOrthoSize * 2f * camRefAspect, camRefOrthoSize * 2f);
			size *= 1.1f;
			worldCollider.size = size;
		}
	}
}
