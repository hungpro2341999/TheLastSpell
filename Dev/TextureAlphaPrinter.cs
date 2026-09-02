using UnityEngine;

namespace Dev;

public class TextureAlphaPrinter : MonoBehaviour
{
	[SerializeField]
	private Texture2D targetTexture;

	private void Awake()
	{
		if (targetTexture == null)
		{
			targetTexture = GetComponent<SpriteRenderer>()?.sprite?.texture;
		}
	}
}
