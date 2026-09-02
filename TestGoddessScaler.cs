using UnityEngine;
using UnityEngine.UI;

public class TestGoddessScaler : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler refScaler;

	private void Update()
	{
		base.transform.localScale = new Vector3(1f / refScaler.scaleFactor, 1f / refScaler.scaleFactor);
	}
}
