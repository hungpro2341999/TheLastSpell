using System.Collections;
using TPLib.Yield;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Sound;

public class OneShotSound : MonoBehaviour
{
	[SerializeField]
	private AudioSource audioSource;

	public void Play(AudioClip clip, float delay = 0f)
	{
		SoundManager.PlayAudioClip(audioSource, clip, delay, doNotInterrupt: true);
		StartCoroutine(Hide(delay + clip.length));
	}

	public void PlaySpatialized(AudioClip clip, Vector3 position, float delay = 0f)
	{
		if (audioSource.spatialBlend == 0f && audioSource.GetCustomCurve(AudioSourceCurveType.SpatialBlend) == null)
		{
			Debug.LogWarning("Calling the PlaySpatialized method on an AudioSource with no 3D blending.");
		}
		base.transform.position = position;
		SoundManager.PlayAudioClip(audioSource, clip, delay, doNotInterrupt: true);
		StartCoroutine(Hide(delay + clip.length));
	}

	public void PlaySpatialized(AudioClip clip, Tile tile, float delay = 0f)
	{
		if (tile == null)
		{
			Play(clip, delay);
		}
		else
		{
			PlaySpatialized(clip, TileMapView.GetTileCenter(tile), delay);
		}
	}

	private IEnumerator Hide(float delay)
	{
		yield return SharedYields.WaitForSeconds(delay);
		base.gameObject.SetActive(value: false);
	}
}
