using UnityEngine;

namespace Dev;

public class TestShaderPositionArrayTarget : MonoBehaviour
{
	[SerializeField]
	private Vector2 offset = Vector2.zero;

	public Vector4 ShaderPosition => new Vector4(base.transform.position.x + offset.x, base.transform.position.y + offset.y, base.transform.position.z, 0f);
}
