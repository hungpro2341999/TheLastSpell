using TPLib;
using UnityEngine;

namespace Dev;

public class TestAtlasUVs : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private MaterialPropertyBlock materialPropBlock;

	[ContextMenu("Set Atlas UV")]
	public void SetAtlasUv()
	{
		if (spriteRenderer == null)
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
		}
		if (spriteRenderer == null)
		{
			TPDebug.LogError("Can't work without a renderer. Aborting...", this);
			return;
		}
		if (materialPropBlock == null)
		{
			materialPropBlock = new MaterialPropertyBlock();
		}
		Sprite sprite = spriteRenderer.sprite;
		if (sprite == null)
		{
			TPDebug.LogError("Can't work without a sprite. Aborting...", this);
			return;
		}
		spriteRenderer.GetPropertyBlock(materialPropBlock);
		Vector4 value = new Vector4(sprite.textureRect.position.x, sprite.textureRect.position.y, sprite.textureRect.size.x, sprite.textureRect.size.y);
		materialPropBlock.SetVector("_AtlasRect", value);
		spriteRenderer.SetPropertyBlock(materialPropBlock);
	}

	private void Awake()
	{
		SetAtlasUv();
	}
}
