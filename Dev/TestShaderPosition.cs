using UnityEngine;

namespace Dev;

public class TestShaderPosition : MonoBehaviour
{
	[SerializeField]
	private Vector3 offset = Vector2.zero;

	private void Update()
	{
		Shader.SetGlobalVector("_Position", base.transform.position + offset);
	}
}
