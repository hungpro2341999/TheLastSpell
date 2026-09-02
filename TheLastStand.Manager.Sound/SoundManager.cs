using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Framework.Utils;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.ScriptableObjects;
using TheLastStand.View.Sound;
using UnityEngine;
using UnityEngine.Audio;

namespace TheLastStand.Manager.Sound;

public class SoundManager : Manager<SoundManager>
{
	public static class Constants
	{
		public static class AudioMixer
		{
			public const string SnapshotNormal = "Day";

			public const string SnapshotSettings = "Menu_Settings";

			public const string SnapshotInn = "Amb_Tavern";

			public const string SnapshotNight = "Night";

			public const string ParamMasterVolume = "MasterVolume";

			public const string ParamMusicVolume = "MusicVolume";

			public const string ParamUiVolume = "UIVolume";

			public const string ParamAmbientVolume = "AmbientVolume";
		}

		public static class Volume
		{
			public const float DefaultLogScale = 30f;

			public const float MaxDecibelValue = 0f;

			public const float MinDecibelValue = -80f;
		}
	}

	[Serializable]
	public class Playlist
	{
		[SerializeField]
		private AudioClip productionPhaseAudioClip;

		[SerializeField]
		private AudioClip deploymentPhaseAudioClip;

		[SerializeField]
		private AudioClip nightAudioClip;

		[SerializeField]
		private int desiredDay = -1;

		public AudioClip ProductionPhaseAudioClip => productionPhaseAudioClip;

		public AudioClip DeploymentPhaseAudioClip => deploymentPhaseAudioClip;

		public int DesiredDay => desiredDay;

		public AudioClip NightAudioClip => nightAudioClip;
	}

	[SerializeField]
	protected AudioMixer masterMixer;

	[SerializeField]
	private Playlist[] playlists = new Playlist[2];

	[SerializeField]
	private List<SerializablePair<string, Playlist>> specificPlaylists = new List<SerializablePair<string, Playlist>>();

	[SerializeField]
	private Playlist lastDayPlaylist;

	[SerializeField]
	private AudioClip creditsMusic;

	[SerializeField]
	private AudioClip menuMusic;

	[SerializeField]
	private AudioClip metaShopsMusic;

	[SerializeField]
	private AudioClip metaShopsNoGoddessesMusic;

	[SerializeField]
	private AudioClip worldMapMusic;

	[SerializeField]
	private float fadeOutTime = 2f;

	[SerializeField]
	private Ease fadeOutEasing = Ease.OutCubic;

	[SerializeField]
	private float fadeInTime = 2f;

	[SerializeField]
	private Ease fadeInEasing = Ease.InCubic;

	[SerializeField]
	private AudioSource[] musicAudioSources = new AudioSource[2];

	[SerializeField]
	private OneShotSound buildingSkillSFXPrefab;

	[SerializeField]
	private OneShotSound buildingSkillSpatializedSFXPrefab;

	[SerializeField]
	private OneShotSound enemySkillSFXPrefab;

	[SerializeField]
	private OneShotSound enemySkillSpatializedSFXPrefab;

	[SerializeField]
	private float dayTransitionDuration;

	[SerializeField]
	private float nightTransitionDuration;

	private int activeAudioSourceIndex;

	private Tween[] musicTransitionTween = new Tween[2];

	private float masterVolumeInternal = 1f;

	private float uiVolumeInternal = 1f;

	private float musicVolumeInternal = 1f;

	private float ambientVolumeInternal = 1f;

	private int currentPlaylistIndex;

	private List<int> alreadyPlayedPlaylists = new List<int>();

	[SerializeField]
	private bool debugChangeMusicAfterReportPopup;

	public static OneShotSound BuildingSkillSFXPrefab => TPSingleton<SoundManager>.Instance.buildingSkillSFXPrefab;

	public static OneShotSound BuildingSkillSpatializedSFXPrefab => TPSingleton<SoundManager>.Instance.buildingSkillSpatializedSFXPrefab;

	public static OneShotSound EnemySkillSFXPrefab => TPSingleton<SoundManager>.Instance.enemySkillSFXPrefab;

	public static OneShotSound EnemySkillSpatializedSFXPrefab => TPSingleton<SoundManager>.Instance.enemySkillSpatializedSFXPrefab;

	public static float DayTransitionDuration => TPSingleton<SoundManager>.Instance.dayTransitionDuration;

	public static float NightTransitionDuration => TPSingleton<SoundManager>.Instance.nightTransitionDuration;

	private float AmbientVolume
	{
		get
		{
			return ambientVolumeInternal;
		}
		set
		{
			ambientVolumeInternal = Mathf.Clamp01(value);
			if (masterMixer != null)
			{
				masterMixer.SetFloat("AmbientVolume", ConvertNormalizedFloatToDecibels(ambientVolumeInternal));
			}
		}
	}

	private float MasterVolume
	{
		get
		{
			return masterVolumeInternal;
		}
		set
		{
			masterVolumeInternal = Mathf.Clamp01(value);
			if (masterMixer != null)
			{
				masterMixer.SetFloat("MasterVolume", ConvertNormalizedFloatToDecibels(masterVolumeInternal));
			}
		}
	}

	private float MusicVolume
	{
		get
		{
			return musicVolumeInternal;
		}
		set
		{
			musicVolumeInternal = Mathf.Clamp01(value);
			if (masterMixer != null)
			{
				masterMixer.SetFloat("MusicVolume", ConvertNormalizedFloatToDecibels(musicVolumeInternal));
			}
		}
	}

	private float UiVolume
	{
		get
		{
			return uiVolumeInternal;
		}
		set
		{
			uiVolumeInternal = Mathf.Clamp01(value);
			if (masterMixer != null)
			{
				masterMixer.SetFloat("UIVolume", ConvertNormalizedFloatToDecibels(uiVolumeInternal));
			}
		}
	}

	public AudioClip CreditsMusic => creditsMusic;

	public AudioClip MenuMusic => menuMusic;

	public AudioClip MetaShopsMusic => metaShopsMusic;

	public AudioClip MetaShopsNoGoddessesMusic => metaShopsNoGoddessesMusic;

	public AudioClip WorldMapMusic => worldMapMusic;

	public bool DebugChangeMusicAfterReportPopup => debugChangeMusicAfterReportPopup;

	public static bool CanPlayAudioClip(AudioSource audioSource, bool doNotInterrupt = false)
	{
		if (audioSource != null)
		{
			if (doNotInterrupt)
			{
				return !audioSource.isPlaying;
			}
			return true;
		}
		return false;
	}

	public static void FadeOutAudioSource(AudioSource audioSource, ref Tween fadeOutTween, float fadeOutDuration)
	{
		float duration = fadeOutDuration * Mathf.Clamp01(audioSource.volume);
		fadeOutTween?.Kill();
		fadeOutTween = DOTween.To(() => audioSource.volume, delegate(float x)
		{
			audioSource.volume = x;
		}, 0f, duration).SetEase(Ease.Linear).OnComplete(delegate
		{
			ResetAudioSource(audioSource);
		})
			.OnKill(delegate
			{
				ResetAudioSource(audioSource);
			});
	}

	public static void FadeOutAudioSource(AudioSource audioSource, float fadeOutDuration)
	{
		float duration = fadeOutDuration * Mathf.Clamp01(audioSource.volume);
		DOTween.To(() => audioSource.volume, delegate(float x)
		{
			audioSource.volume = x;
		}, 0f, duration).SetEase(Ease.Linear).OnComplete(delegate
		{
			ResetAudioSource(audioSource);
		})
			.OnKill(delegate
			{
				ResetAudioSource(audioSource);
			});
	}

	public static void ResetAudioSource(AudioSource audioSource)
	{
		audioSource.Stop();
		audioSource.volume = 1f;
	}

	public static void PlayFadeInAudioClip(AudioSource audioSource, ref Tween fadeInTween, AudioClip audioClip, float fadeInDuration, float delay = 0f, bool doNotInterrupt = false)
	{
		if (audioSource == null)
		{
			TPSingleton<SoundManager>.Instance.LogError("AudioSource is null");
		}
		else if (CanPlayAudioClip(audioSource, doNotInterrupt))
		{
			audioSource.clip = audioClip;
			if (delay > 0f)
			{
				TPSingleton<SoundManager>.Instance.StartCoroutine(TPSingleton<SoundManager>.Instance.PlayFadeInDelayed(audioSource, fadeInTween, fadeInDuration, delay));
			}
			else
			{
				TPSingleton<SoundManager>.Instance.PlayFadeIn(audioSource, ref fadeInTween, fadeInDuration);
			}
		}
	}

	public static void PlayAudioClip(AudioSource audioSource, AudioClip audioClip, float delay = 0f, bool doNotInterrupt = false, bool isPooled = false, PooledAudioSourceData pooledAudioSourceData = null)
	{
		if (audioSource == null)
		{
			TPSingleton<SoundManager>.Instance.LogError("AudioSource is null");
		}
		else if (CanPlayAudioClip(audioSource, doNotInterrupt))
		{
			if (isPooled)
			{
				audioSource.name = audioClip.name;
			}
			audioSource.clip = audioClip;
			if (delay > 0f)
			{
				TPSingleton<SoundManager>.Instance.StartCoroutine(TPSingleton<SoundManager>.Instance.PlayDelayed(audioSource, delay));
			}
			else
			{
				audioSource.Play();
			}
			if (isPooled)
			{
				TPSingleton<PooledAudioSourcesManager>.Instance.IncreaseUsedPooledAudioSources(pooledAudioSourceData, 1);
				TPSingleton<SoundManager>.Instance.StartCoroutine(DisablePooledAudioSourceAfterDelay(audioSource, pooledAudioSourceData, delay + audioClip.length));
			}
		}
	}

	public static void PlayAudioClip(AudioSource audioSource, AudioClip[] audioClips, float delay = 0f, bool doNotInterrupt = false, bool isPooled = false, PooledAudioSourceData pooledAudioSourceData = null)
	{
		if (audioClips != null && audioClips.Length != 0)
		{
			PlayAudioClip(audioSource, audioClips[RandomManager.GetRandomRange(TPSingleton<SoundManager>.Instance, 0, audioClips.Length)], delay, doNotInterrupt, isPooled, pooledAudioSourceData);
		}
	}

	public static AudioSource PlayAudioClip(AudioClip audioClip, PooledAudioSourceData pooledAudioSourceData = null, float delay = 0f, bool doNotInterrupt = false)
	{
		if (pooledAudioSourceData == null)
		{
			pooledAudioSourceData = PooledAudioSourcesManager.DefaultPooledAudioSourceData;
		}
		if (TPSingleton<PooledAudioSourcesManager>.Instance.IsPoolFull(pooledAudioSourceData))
		{
			return null;
		}
		AudioSource availablePooledAudioSource = PooledAudioSourcesManager.GetAvailablePooledAudioSource(pooledAudioSourceData);
		PlayAudioClip(availablePooledAudioSource, audioClip, delay, doNotInterrupt, isPooled: true, pooledAudioSourceData);
		return availablePooledAudioSource;
	}

	public static AudioSource PlayAudioClip(AudioClip[] audioClips, PooledAudioSourceData pooledAudioSourceData = null, float delay = 0f, bool doNotInterrupt = false)
	{
		if (pooledAudioSourceData == null)
		{
			pooledAudioSourceData = PooledAudioSourcesManager.DefaultPooledAudioSourceData;
		}
		if (TPSingleton<PooledAudioSourcesManager>.Instance.IsPoolFull(pooledAudioSourceData))
		{
			return null;
		}
		AudioSource availablePooledAudioSource = PooledAudioSourcesManager.GetAvailablePooledAudioSource(pooledAudioSourceData);
		PlayAudioClip(availablePooledAudioSource, audioClips, delay, doNotInterrupt, isPooled: true, pooledAudioSourceData);
		return availablePooledAudioSource;
	}

	public void ChangeMusic(bool instant = false)
	{
		int victoryDaysCount = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.VictoryDaysCount;
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;
		Playlist playlist = null;
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		if (currentSpawnWave != null && currentSpawnWave.SpawnWaveDefinition.IsBossWave)
		{
			playlist = specificPlaylists.FirstOrDefault((SerializablePair<string, Playlist> x) => x.Key == SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.BossWaveSettings.SpecificPlaylistId).Value;
		}
		if (playlist == null)
		{
			playlist = ((flag ? (victoryDaysCount == TPSingleton<GameManager>.Instance.Game.DayNumber) : (victoryDaysCount == TPSingleton<GameManager>.Instance.Game.DayNumber + 1)) ? lastDayPlaylist : playlists[currentPlaylistIndex]);
		}
		if (flag)
		{
			FadeMusic(playlist.NightAudioClip, instant);
		}
		else if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			FadeMusic(playlist.ProductionPhaseAudioClip, instant);
		}
		else
		{
			FadeMusic(playlist.DeploymentPhaseAudioClip, instant);
		}
	}

	public void ChangePlaylist()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int i = 0; i < playlists.Length; i++)
		{
			if (playlists[i].DesiredDay == TPSingleton<GameManager>.Instance.Game.DayNumber)
			{
				list.Add(i);
			}
			else if (playlists[i].DesiredDay == -1)
			{
				list2.Add(i);
			}
		}
		if (alreadyPlayedPlaylists.Count == list2.Count)
		{
			alreadyPlayedPlaylists.Clear();
		}
		if (list.Count > 0)
		{
			currentPlaylistIndex = RandomManager.GetRandomElement(this, list);
			return;
		}
		foreach (int alreadyPlayedPlaylist in alreadyPlayedPlaylists)
		{
			if (list2.Contains(alreadyPlayedPlaylist))
			{
				list2.Remove(alreadyPlayedPlaylist);
			}
		}
		if (list2.Contains(currentPlaylistIndex))
		{
			list2.Remove(currentPlaylistIndex);
		}
		if (list2.Count > 0)
		{
			currentPlaylistIndex = RandomManager.GetRandomElement(this, list2);
		}
		else
		{
			TPSingleton<SoundManager>.Instance.LogError($"No Playlist has been validated for day {TPSingleton<GameManager>.Instance.Game.DayNumber}! Picking a random one though it could be the same as the previous one.");
			Playlist randomElement = RandomManager.GetRandomElement(this, playlists.Where((Playlist o) => o.DesiredDay == -1));
			currentPlaylistIndex = Array.IndexOf(playlists, randomElement);
		}
		alreadyPlayedPlaylists.Add(currentPlaylistIndex);
	}

	public void FadeMusic(AudioClip nextMusic, bool instant = false)
	{
		for (int i = 0; i < musicTransitionTween.Length; i++)
		{
			if (musicTransitionTween[i] != null)
			{
				musicTransitionTween[i].Kill();
			}
		}
		if (nextMusic == musicAudioSources[activeAudioSourceIndex].clip)
		{
			for (int j = 0; j < musicAudioSources.Length; j++)
			{
				if (j == activeAudioSourceIndex)
				{
					musicTransitionTween[j] = musicAudioSources[j].DOFade(1f, instant ? 0f : fadeInTime).SetEase(fadeInEasing);
				}
				else
				{
					musicTransitionTween[j] = musicAudioSources[j].DOFade(0f, instant ? 0f : fadeOutTime).SetEase(fadeOutEasing);
				}
			}
			return;
		}
		if (instant)
		{
			for (int k = 0; k < musicAudioSources.Length; k++)
			{
				if (k == activeAudioSourceIndex)
				{
					musicAudioSources[k].clip = nextMusic;
					musicAudioSources[k].Play();
				}
				else
				{
					musicAudioSources[k].clip = null;
					musicAudioSources[k].Stop();
				}
			}
			return;
		}
		for (int l = 0; l < musicAudioSources.Length; l++)
		{
			if (l == activeAudioSourceIndex)
			{
				int audioSourceStoppedIndex = l;
				musicTransitionTween[l] = musicAudioSources[l].DOFade(0f, fadeOutTime).SetEase(fadeOutEasing).OnComplete(delegate
				{
					musicAudioSources[audioSourceStoppedIndex].clip = null;
				});
				continue;
			}
			int newActiveAudioSouceIndex = l;
			musicTransitionTween[l] = musicAudioSources[l].DOFade(1f, fadeInTime).SetEase(fadeInEasing).OnComplete(delegate
			{
				activeAudioSourceIndex = newActiveAudioSouceIndex;
			});
			musicAudioSources[l].clip = nextMusic;
			musicAudioSources[l].Play();
		}
	}

	public void StopMusic()
	{
		for (int i = 0; i < musicAudioSources.Length; i++)
		{
			musicAudioSources[i].clip = null;
			musicAudioSources[i].Stop();
		}
	}

	public void TransitionToNormalSnapshot(float duration = 0f)
	{
		if (!TPSingleton<GameManager>.Exist() || (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production))
		{
			TransitionToDefaultSnapshot(duration);
		}
		else
		{
			TransitionToAudioMixerSnapshot("Night", duration);
		}
	}

	public void TransitionToDefaultSnapshot(float duration = 0f)
	{
		TransitionToAudioMixerSnapshot("Day", duration);
	}

	public void TransitionToSettingsSnapshot(float duration = 0f)
	{
		TransitionToAudioMixerSnapshot("Menu_Settings", duration);
	}

	public void TransitionToInnSnapshot(float duration = 0f)
	{
		TransitionToAudioMixerSnapshot("Amb_Tavern", duration);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.MasterVolumeSettingChangeEvent.RemoveListener(OnMasterVolumeSettingChanged);
			TPSingleton<SettingsManager>.Instance.MusicVolumeSettingChangeEvent.RemoveListener(OnMusicVolumeSettingChanged);
			TPSingleton<SettingsManager>.Instance.AmbientVolumeSettingChangeEvent.RemoveListener(OnAmbientVolumeSettingChanged);
			TPSingleton<SettingsManager>.Instance.UiVolumeSettingChangeEvent.RemoveListener(OnUiVolumeSettingChanged);
		}
	}

	private static IEnumerator DisablePooledAudioSourceAfterDelay(AudioSource audioSource, PooledAudioSourceData pooledAudioSourceData, float delay)
	{
		yield return new WaitForSeconds(delay);
		audioSource.gameObject.SetActive(value: false);
		TPSingleton<PooledAudioSourcesManager>.Instance.DecreaseUsedPooledAudioSources(pooledAudioSourceData, 1);
	}

	private void TransitionToAudioMixerSnapshot(string snapshotId, float duration = 0f)
	{
		if (masterMixer == null)
		{
			LogError("AudioMixer reference is missing! Aborting snapshot transition...", CLogLevel.DETAILED);
			return;
		}
		AudioMixerSnapshot audioMixerSnapshot = masterMixer.FindSnapshot(snapshotId);
		if (audioMixerSnapshot == null)
		{
			LogError("AudioMixer snapshot '" + snapshotId + "' not found! Aborting transition...", CLogLevel.DETAILED);
		}
		else
		{
			audioMixerSnapshot.TransitionTo(duration);
		}
	}

	private float ConvertNormalizedFloatToDecibels(float factor, float logScale = 30f)
	{
		if (factor <= 0f)
		{
			return -80f;
		}
		if (factor >= 1f)
		{
			return 0f;
		}
		return Mathf.Clamp(logScale * Mathf.Log10(factor), -80f, 0f);
	}

	private void OnAmbientVolumeSettingChanged(float volume)
	{
		AmbientVolume = volume;
	}

	private void OnMasterVolumeSettingChanged(float volume)
	{
		MasterVolume = volume;
	}

	private void OnMusicVolumeSettingChanged(float volume)
	{
		MusicVolume = volume;
	}

	private void OnUiVolumeSettingChanged(float volume)
	{
		UiVolume = volume;
	}

	private IEnumerator PlayDelayed(AudioSource audioSource, float delay)
	{
		yield return SharedYields.WaitForSeconds(delay);
		audioSource.Play();
	}

	private void PlayFadeIn(AudioSource audioSource, ref Tween fadeInTween, float fadeInDuration)
	{
		fadeInTween?.Kill();
		audioSource.volume = 0f;
		audioSource.Play();
		fadeInTween = DOTween.To(() => audioSource.volume, delegate(float x)
		{
			audioSource.volume = x;
		}, 1f, fadeInDuration).SetEase(Ease.Linear);
	}

	private IEnumerator PlayFadeInDelayed(AudioSource audioSource, Tween fadeInTween, float fadeInDuration, float delay)
	{
		yield return SharedYields.WaitForSeconds(delay);
		PlayFadeIn(audioSource, ref fadeInTween, fadeInDuration);
	}

	private void Start()
	{
		if (TPSingleton<SettingsManager>.Exist())
		{
			TPSingleton<SettingsManager>.Instance.MasterVolumeSettingChangeEvent.AddListener(OnMasterVolumeSettingChanged);
			TPSingleton<SettingsManager>.Instance.MusicVolumeSettingChangeEvent.AddListener(OnMusicVolumeSettingChanged);
			TPSingleton<SettingsManager>.Instance.AmbientVolumeSettingChangeEvent.AddListener(OnAmbientVolumeSettingChanged);
			TPSingleton<SettingsManager>.Instance.UiVolumeSettingChangeEvent.AddListener(OnUiVolumeSettingChanged);
			MasterVolume = TPSingleton<SettingsManager>.Instance.Settings.MasterVolume;
			MusicVolume = TPSingleton<SettingsManager>.Instance.Settings.MusicVolume;
			AmbientVolume = TPSingleton<SettingsManager>.Instance.Settings.AmbientVolume;
			UiVolume = TPSingleton<SettingsManager>.Instance.Settings.UiVolume;
		}
		if (TPSingleton<RandomManager>.Exist())
		{
			ChangePlaylist();
			return;
		}
		List<Playlist> list = playlists.Where((Playlist x) => x.DesiredDay == -1).ToList();
		currentPlaylistIndex = UnityEngine.Random.Range(0, list.Count);
	}

	[DevConsoleCommand("ChangePlaylist")]
	public static void DebugChangePlaylist()
	{
		TPSingleton<SoundManager>.Instance.ChangePlaylist();
		TPSingleton<SoundManager>.Instance.ChangeMusic();
	}
}
