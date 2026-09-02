using UnityEngine;

namespace TheLastStand.View;

public class DisableSelf : MonoBehaviour
{
	public void Disable()
	{
		base.gameObject.SetActive(value: false);
	}
}
