using TMPro;
using TheLastStand.Controller.Settings;
using TheLastStand.Framework.UI;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class VolumeSlider : MonoBehaviour
{
	public enum E_VolumeType
	{
		Master,
		Music,
		UI,
		Ambient
	}

	[SerializeField]
	private E_VolumeType volumeType;

	[SerializeField]
	private BetterSlider slider;

	[SerializeField]
	private TextMeshProUGUI progressText;

	private bool hasBeenInitialized;

	private float volume;

	public float Volume
	{
		get
		{
			return volume;
		}
		set
		{
			volume = value;
			switch (volumeType)
			{
			case E_VolumeType.Master:
				SettingsController.SetMasterVolume(volume);
				break;
			case E_VolumeType.Music:
				SettingsController.SetMusicVolume(volume);
				break;
			case E_VolumeType.UI:
				SettingsController.SetUIVolume(volume);
				break;
			case E_VolumeType.Ambient:
				SettingsController.SetAmbientVolume(volume);
				break;
			}
		}
	}

	public void OnVolumeSliderChange()
	{
		if (hasBeenInitialized)
		{
			Volume = slider.value;
			RefreshProgressText();
		}
	}

	public void RefreshVolume(float volume)
	{
		hasBeenInitialized = true;
		slider.value = volume;
	}

	private void Awake()
	{
		volume = slider.value;
	}

	private void RefreshProgressText()
	{
		progressText.text = $"{Mathf.RoundToInt(volume * 100f)}%";
	}
}
