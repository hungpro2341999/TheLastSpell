using UnityEngine;

namespace TheLastStand.View.Generic;

public class WorldUiScaler : MonoBehaviour
{
	[SerializeField]
	private int worldPpu = 28;

	[ContextMenu("Scale")]
	public void DoScale()
	{
		float num = 1f / (float)worldPpu;
		base.transform.localScale = new Vector3(num, num, num);
	}

	private void Start()
	{
		DoScale();
	}
}
