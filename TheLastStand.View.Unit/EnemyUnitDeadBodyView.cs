using System.Collections;
using DG.Tweening;
using TPLib;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.View.Unit;

[SelectionBase]
public class EnemyUnitDeadBodyView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer deadBodySpriteRend;

	[SerializeField]
	private Vector2 waitRandomRange = new Vector2(1f, 300f);

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeTime = 0.5f;

	[SerializeField]
	private Ease fadeEasing = Ease.InSine;

	[SerializeField]
	[Range(-3f, 0f)]
	private float translateMoveY = -0.15f;

	[SerializeField]
	[Range(0f, 3f)]
	private float translateTime = 0.5f;

	[SerializeField]
	private Ease translateEasing = Ease.InCubic;

	private Coroutine waitCoroutine;

	public Tile Tile { get; set; }

	public void StartWaitDisappear()
	{
		if (waitCoroutine == null)
		{
			waitCoroutine = StartCoroutine(WaitDisappear());
		}
	}

	public void Init(EnemyUnit enemyUnit)
	{
		base.transform.position += (Vector3)enemyUnit.EnemyUnitTemplateDefinition.VisualOffset;
		base.transform.localScale = new Vector3(enemyUnit.EnemyUnitView.OrientationRootTransform.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
		string resourcePath = ((enemyUnit is EliteEnemyUnit) ? ("View/Sprites/Units/" + enemyUnit.EnemyUnitView.GetDefaultSpritesFolder() + "/DeadBodies/" + enemyUnit.SpecificId + "/" + enemyUnit.VariantId + "/" + enemyUnit.SpecificId + "_" + enemyUnit.VariantId + "_DeadBody_Front") : ("View/Sprites/Units/" + enemyUnit.EnemyUnitView.GetDefaultSpritesFolder() + "/DeadBodies/" + enemyUnit.SpecificAssetsId + "/" + enemyUnit.VariantId + "/" + enemyUnit.SpecificAssetsId + "_Lvl1_" + enemyUnit.VariantId + "_DeadBody_Front"));
		deadBodySpriteRend.sprite = ResourcePooler.LoadOnce<Sprite>(resourcePath);
		deadBodySpriteRend.sortingOrder = Tile.EnemyUnitDeadBodyViews.Count;
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && waitCoroutine == null)
		{
			waitCoroutine = StartCoroutine(WaitDisappear());
		}
	}

	[ContextMenu("ForceDisappear")]
	public void ForceDisappear()
	{
		if (waitCoroutine != null)
		{
			StopCoroutine(waitCoroutine);
			waitCoroutine = null;
		}
		Disappear();
	}

	private void Disappear()
	{
		Tile.EnemyUnitDeadBodyViews.Remove(this);
		deadBodySpriteRend.transform.DOLocalMoveY(deadBodySpriteRend.transform.localPosition.y + translateMoveY, translateTime).SetEase(translateEasing);
		deadBodySpriteRend.DOFade(0f, fadeTime).SetEase(fadeEasing).OnComplete(delegate
		{
			Object.Destroy(base.gameObject);
		});
	}

	private IEnumerator WaitDisappear()
	{
		yield return SharedYields.WaitForSeconds(RandomManager.GetRandomRange(this, waitRandomRange.x, waitRandomRange.y));
		Disappear();
		waitCoroutine = null;
	}
}
