using System.Collections;
using TPLib.Yield;
using TheLastStand.Model.Building;
using UnityEngine;

namespace TheLastStand.View.Building;

public class DestructionAnimationView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private int animationFrameRate = 12;

	private Sprite[] destructionAnimationSprites;

	private float delay;

	private TheLastStand.Model.Building.Building building;

	public void ChangeBuilding(TheLastStand.Model.Building.Building newBuilding)
	{
		if (building != null && building.BuildingView != null)
		{
			building.BuildingView.ActiveDestructionAnimationViewNb--;
		}
		building = newBuilding;
		if (newBuilding != null && newBuilding.BuildingView != null)
		{
			newBuilding.BuildingView.ActiveDestructionAnimationViewNb++;
		}
	}

	public void Init(Vector3 worldPosition, Sprite[] sprites, float delay)
	{
		base.gameObject.SetActive(value: true);
		base.transform.position = worldPosition;
		destructionAnimationSprites = sprites;
		this.delay = delay;
	}

	public void PlayDestructionAnimation()
	{
		StartCoroutine(PlayDestructionAnimationCoroutine());
	}

	private IEnumerator PlayDestructionAnimationCoroutine()
	{
		spriteRenderer.sprite = destructionAnimationSprites[0];
		float duration = delay;
		yield return SharedYields.WaitForSeconds(duration);
		float framesStep = 1f / (float)animationFrameRate;
		int i = 0;
		while (i < destructionAnimationSprites.Length)
		{
			spriteRenderer.sprite = destructionAnimationSprites[i];
			yield return SharedYields.WaitForSeconds(framesStep);
			int num = i + 1;
			i = num;
		}
		base.gameObject.SetActive(value: false);
		ChangeBuilding(null);
	}
}
