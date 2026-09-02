using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Sound;

public class PlayUISound : MonoBehaviour
{
	public void PlayAudioClip(AudioClip audioClip)
	{
		if (TPSingleton<UIManager>.Exist())
		{
			TPSingleton<UIManager>.Instance.PlayAudioClip(audioClip);
		}
		else if (TPSingleton<WorldMapUIManager>.Exist())
		{
			TPSingleton<WorldMapUIManager>.Instance.PlayAudioClip(audioClip);
		}
		else if (TPSingleton<MetaShopsManager>.Exist())
		{
			TPSingleton<MetaShopsManager>.Instance.PlayAudioClip(audioClip);
		}
	}

	public void PlayAudioClipWithoutInterrupting(AudioClip audioClip)
	{
		if (TPSingleton<UIManager>.Exist())
		{
			TPSingleton<UIManager>.Instance.PlayAudioClipWithoutInterrupting(audioClip);
		}
		else if (TPSingleton<WorldMapUIManager>.Exist())
		{
			TPSingleton<WorldMapUIManager>.Instance.PlayAudioClipWithoutInterrupting(audioClip);
		}
		else if (TPSingleton<MetaShopsManager>.Exist())
		{
			TPSingleton<MetaShopsManager>.Instance.PlayAudioClipWithoutInterrupting(audioClip);
		}
	}

	public void PlayAudioClipAndForget(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(audioClip);
	}
}
