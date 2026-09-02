using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.ScriptableObjects;
using UnityEngine;

namespace TheLastStand.Manager;

public class PooledAudioSourcesManager : Manager<PooledAudioSourcesManager>
{
	[SerializeField]
	private PooledAudioSourceData defaultPooledAudioSourceData;

	[SerializeField]
	[Tooltip("The maximum amount of active pooled audio sources the system can support.")]
	private int maxGlobalPooledAudioSources = 10;

	[SerializeField]
	[Tooltip("The maximum amount of active pooled audio sources the system can support.")]
	private int maxPooledAudioSources = 10;

	private readonly Dictionary<PooledAudioSourceData, List<AudioSource>> pooledAudioSources = new Dictionary<PooledAudioSourceData, List<AudioSource>>();

	private Dictionary<PooledAudioSourceData, int> usedPooledAudioSources = new Dictionary<PooledAudioSourceData, int>();

	private int globalUsedPooledAudioSources;

	public static PooledAudioSourceData DefaultPooledAudioSourceData => TPSingleton<PooledAudioSourcesManager>.Instance.defaultPooledAudioSourceData;

	public static AudioSource GetAvailablePooledAudioSource(PooledAudioSourceData pooledAudioSourceData)
	{
		if (!TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources.ContainsKey(pooledAudioSourceData))
		{
			TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources.Add(pooledAudioSourceData, new List<AudioSource>(1));
			GameObject gameObject = new GameObject(pooledAudioSourceData.name);
			gameObject.transform.parent = TPSingleton<PooledAudioSourcesManager>.Instance.transform;
			pooledAudioSourceData.poolParent = gameObject.transform;
		}
		AudioSource audioSource = TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources[pooledAudioSourceData].FirstOrDefault((AudioSource audioSource2) => !audioSource2.gameObject.activeSelf);
		if (audioSource == null)
		{
			audioSource = Object.Instantiate(pooledAudioSourceData.AudioSourcePrefab, pooledAudioSourceData.poolParent);
			TPSingleton<PooledAudioSourcesManager>.Instance.pooledAudioSources[pooledAudioSourceData].Add(audioSource);
		}
		audioSource.gameObject.SetActive(value: true);
		return audioSource;
	}

	public void IncreaseUsedPooledAudioSources(PooledAudioSourceData pooledAudioSourceData, int value)
	{
		usedPooledAudioSources.AddValueOrCreateKey(pooledAudioSourceData, value, (int a, int b) => a + b);
		globalUsedPooledAudioSources += value;
	}

	public void DecreaseUsedPooledAudioSources(PooledAudioSourceData pooledAudioSourceData, int value)
	{
		usedPooledAudioSources[pooledAudioSourceData] -= value;
		globalUsedPooledAudioSources -= value;
	}

	public bool IsPoolFull(PooledAudioSourceData pooledAudioSourceData)
	{
		if (globalUsedPooledAudioSources < maxGlobalPooledAudioSources)
		{
			if (usedPooledAudioSources.TryGetValue(pooledAudioSourceData, out var value))
			{
				return value >= maxPooledAudioSources;
			}
			return false;
		}
		return true;
	}
}
