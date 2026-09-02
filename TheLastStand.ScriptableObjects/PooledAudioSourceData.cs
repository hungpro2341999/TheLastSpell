using UnityEngine;

namespace TheLastStand.ScriptableObjects;

[CreateAssetMenu(menuName = "TLS/Sound/PooledAudioSourceData")]
public class PooledAudioSourceData : ScriptableObject
{
	[SerializeField]
	private AudioSource audioSourcePrefab;

	[HideInInspector]
	public Transform poolParent;

	public AudioSource AudioSourcePrefab => audioSourcePrefab;
}
