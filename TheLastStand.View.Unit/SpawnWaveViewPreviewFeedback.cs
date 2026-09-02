using System.Collections;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class SpawnWaveViewPreviewFeedback : SerializedMonoBehaviour
{
	[SerializeField]
	private Vector2Int offsetFromFogLine = Vector2Int.zero;

	[SerializeField]
	private GameObject arrowDisplay;

	[SerializeField]
	private Transform dangerIndicatorTransform;

	[SerializeField]
	private Transform camTarget;

	private Animator arrowDisplayAnimator;

	private Image arrowDisplayImage;

	private Animator dangerIndicatorAnimator;

	private Image dangerIndicatorImage;

	public Transform CamTarget => camTarget;

	public Animator ArrowDisplayAnimator
	{
		get
		{
			if ((object)arrowDisplayAnimator == null)
			{
				arrowDisplayAnimator = arrowDisplay.GetComponent<Animator>();
			}
			return arrowDisplayAnimator;
		}
	}

	public Image ArrowDisplayImage
	{
		get
		{
			if ((object)arrowDisplayImage == null)
			{
				arrowDisplayImage = arrowDisplay.GetComponent<Image>();
			}
			return arrowDisplayImage;
		}
	}

	public Animator DangerIndicatorAnimator
	{
		get
		{
			if ((object)dangerIndicatorAnimator == null)
			{
				dangerIndicatorAnimator = dangerIndicatorTransform.GetComponent<Animator>();
			}
			return dangerIndicatorAnimator;
		}
	}

	public Image DangerIndicatorImage
	{
		get
		{
			if ((object)dangerIndicatorImage == null)
			{
				dangerIndicatorImage = dangerIndicatorTransform.GetComponent<Image>();
			}
			return dangerIndicatorImage;
		}
	}

	public void Clear()
	{
		base.gameObject.SetActive(value: false);
		if (dangerIndicatorTransform != null)
		{
			DangerIndicatorImage.enabled = false;
		}
		ArrowDisplayImage.enabled = false;
	}

	public void RefreshPosition(Tile tile)
	{
		base.gameObject.SetActive(value: true);
		base.transform.position = TileMapView.GetWorldPosition(tile.Position + offsetFromFogLine);
	}

	public void Refresh(SpawnWaveView.SpawnWaveViewRefreshInfo spawnWaveViewRefreshInfo, bool forceDisplay = false)
	{
		Clear();
		if (forceDisplay || (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying))
		{
			base.gameObject.SetActive(value: true);
			if (spawnWaveViewRefreshInfo.Tile != null)
			{
				RefreshPosition(spawnWaveViewRefreshInfo.Tile);
			}
			if (spawnWaveViewRefreshInfo.LocalProportionPercentage > 0 && arrowDisplay != null)
			{
				StartCoroutine(UpdateArrowAnimationCoroutine(spawnWaveViewRefreshInfo));
			}
			if (dangerIndicatorTransform != null && spawnWaveViewRefreshInfo.DirectionDangerLevel > 0)
			{
				DangerIndicatorAnimator.SetInteger("DangerLevel", spawnWaveViewRefreshInfo.DirectionDangerLevel);
				StartCoroutine(DisplayDangerLevelCoroutine());
			}
		}
	}

	private IEnumerator DisplayDangerLevelCoroutine()
	{
		yield return SharedYields.WaitForEndOfFrame;
		DangerIndicatorImage.enabled = true;
	}

	private IEnumerator UpdateArrowAnimationCoroutine(SpawnWaveView.SpawnWaveViewRefreshInfo spawnWaveViewRefreshInfo)
	{
		if (SpawnWaveManager.SpawnWaveView.GetArrowAssetId(spawnWaveViewRefreshInfo.Zone, spawnWaveViewRefreshInfo.LocalProportionPercentage, out var animatorController))
		{
			ArrowDisplayAnimator.runtimeAnimatorController = animatorController;
			yield return SharedYields.WaitForEndOfFrame;
			ArrowDisplayImage.enabled = true;
			arrowDisplayImage.SetNativeSize();
		}
	}
}
