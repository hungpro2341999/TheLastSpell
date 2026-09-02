using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Building.Construction;

public class ConstructionAnimationView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private SpriteRenderer spriteRendererLUT;

	[SerializeField]
	private Sprite[] shockwaveSprites;

	private int animationFrameRate;

	private int shockwaveFrame;

	private Transform animationTransform;

	private Sprite[] spritesDiffuse;

	private Sprite[] spritesLUT;

	private TheLastStand.Model.Building.Building building;

	public void ChangeBuilding(TheLastStand.Model.Building.Building newBuilding)
	{
		if (building != null && building.BuildingView != null)
		{
			building.BuildingView.ActiveConstructionAnimationViewNb--;
		}
		building = newBuilding;
		if (newBuilding != null && newBuilding.BuildingView != null)
		{
			newBuilding.BuildingView.ActiveConstructionAnimationViewNb++;
		}
	}

	public int GetAnimationFrameRate()
	{
		return animationFrameRate;
	}

	public void Init(int sortingOrder, Sprite[] sprites, int animationFrameRate, int shockwaveFrame, Sprite[] spritesLUT = null)
	{
		this.animationFrameRate = animationFrameRate;
		this.shockwaveFrame = shockwaveFrame;
		spriteRenderer.sortingOrder = sortingOrder;
		spriteRendererLUT.sortingOrder = sortingOrder;
		spritesDiffuse = sprites;
		this.spritesLUT = spritesLUT;
	}

	public void PlayConstructionAnimation()
	{
		StartCoroutine(PlayConstructionAnimationCoroutine());
	}

	private Vector3 GetAnimationPosition()
	{
		if (animationTransform == null)
		{
			animationTransform = base.transform;
		}
		return animationTransform.position;
	}

	private IEnumerator PlayConstructionAnimationCoroutine()
	{
		int spritesLength = spritesDiffuse.Length;
		if (shockwaveFrame > spritesLength - 1)
		{
			TPSingleton<BuildingManager>.Instance.LogWarning("Shockwave frame number is higher than the actual frames count and won't appear.");
		}
		if (spritesLUT != null && spritesLUT.Length != spritesDiffuse.Length)
		{
			TPSingleton<BuildingManager>.Instance.LogWarning("lut frame count is different than the actual frames count. Since this can lead to errors, we hide the lut anim :" + $" {spritesLUT.Length} != {spritesDiffuse.Length}");
			spritesLUT = null;
		}
		bool isLUTActive = spritesLUT != null;
		spriteRendererLUT.enabled = isLUTActive;
		float framesStep = 1f / (float)animationFrameRate;
		int spriteIndex = 0;
		while (spriteIndex < spritesLength)
		{
			spriteRenderer.sprite = spritesDiffuse[spriteIndex];
			if (isLUTActive)
			{
				spriteRendererLUT.sprite = spritesLUT[spriteIndex];
			}
			if (spriteIndex == shockwaveFrame)
			{
				TileMapView.SpawnConstructionAnimation(GetAnimationPosition(), shockwaveSprites, spriteRenderer.sortingOrder - 1, animationFrameRate, -1);
			}
			yield return SharedYields.WaitForSeconds(framesStep);
			int num = spriteIndex + 1;
			spriteIndex = num;
		}
		base.gameObject.SetActive(value: false);
		ChangeBuilding(null);
	}
}
