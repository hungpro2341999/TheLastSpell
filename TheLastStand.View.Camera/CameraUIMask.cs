using System.Collections;
using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraUIMask : MonoBehaviour
{
	public class ScreenPointsAngleComparer : IComparer
	{
		private readonly UnityEngine.Camera camera;

		private readonly Vector2 pointsCenter;

		public ScreenPointsAngleComparer(UnityEngine.Camera camera, Vector2 pointsCenterWorld)
		{
			this.camera = camera;
			pointsCenter = pointsCenterWorld;
		}

		public int Compare(object a, object b)
		{
			Transform transform = (Transform)a;
			Transform transform2 = (Transform)b;
			float num = Vector2.SignedAngle(pointsCenter - (Vector2)camera.ScreenToWorldPoint(transform.position), pointsCenter);
			float value = Vector2.SignedAngle(pointsCenter - (Vector2)camera.ScreenToWorldPoint(transform2.position), pointsCenter);
			return num.CompareTo(value);
		}
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Transform[] vertices;

	public Canvas Canvas => canvas;

	public Transform[] Vertices => vertices;

	private void Start()
	{
		CameraView.CameraUIMasksHandler.RegisterMask(this);
	}

	private void Reset()
	{
		_ = Canvas == null;
	}
}
