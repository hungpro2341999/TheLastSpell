using UnityEngine;

namespace TheLastStand.View.Generic;

public class CameraAreaOfInterest : MonoBehaviour
{
	[SerializeField]
	private float areaWeight;

	[SerializeField]
	private Collider2D areaCollider;

	public float AreaWeight => areaWeight;

	public Collider2D AreaCollider => areaCollider;

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 1f);
		Gizmos.DrawWireSphere(base.transform.position + (Vector3)areaCollider.offset, areaWeight);
	}
}
