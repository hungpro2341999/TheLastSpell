using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Building;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.View.Building;

public class BuildingCorpseView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer deadBuildingSpriteRenderer;

	[SerializeField]
	private Vector2 waitRandomRange = new Vector2(1f, 300f);

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeTime = 0.5f;

	[SerializeField]
	private Ease fadeEasing = Ease.InSine;

	private Coroutine waitCoroutine;

	public Tile Tile { get; set; }

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

	public void Init(BuildingDefinition building)
	{
		Sprite sprite = ResourcePooler<Sprite>.LoadOnce(string.Format("View\\Sprites\\Buildings\\Destroyed\\{0}\\TLS_Buildings_{0}Remains", building.Id) ?? "");
		if (sprite == null)
		{
			List<Sprite> list = new List<Sprite>();
			for (int i = 1; i < 99; i++)
			{
				string text = i.ToString("00");
				sprite = ResourcePooler<Sprite>.LoadOnce(string.Format("View\\Sprites\\Buildings\\Destroyed\\{0}\\TLS_Buildings_{0}Remains", building.Id) + text);
				if (sprite == null)
				{
					break;
				}
				list.Add(sprite);
			}
			if (list.Count <= 0)
			{
				TPSingleton<BuildingManager>.Instance?.LogWarning("No dead building sprite specified for " + building.Id + " - the building may simply 'pop off' once dead, which may look odd.", CLogLevel.DETAILED);
			}
			else
			{
				sprite = RandomManager.GetRandomElement(this, list);
			}
		}
		deadBuildingSpriteRenderer.sprite = sprite;
		_ = deadBuildingSpriteRenderer.sprite == null;
		deadBuildingSpriteRenderer.sortingOrder = Tile.DeadBuildingViews.Count;
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && waitCoroutine == null)
		{
			waitCoroutine = StartCoroutine(WaitDisappear());
		}
	}

	private void Disappear()
	{
		Tile.DeadBuildingViews.Remove(this);
		deadBuildingSpriteRenderer.DOFade(0f, fadeTime).SetEase(fadeEasing).OnComplete(delegate
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
