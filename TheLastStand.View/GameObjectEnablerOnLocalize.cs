using System;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View;

public class GameObjectEnablerOnLocalize : MonoBehaviour
{
	[SerializeField]
	private GameObject gameObjectToEnable;

	[SerializeField]
	private string[] targetLanguages;

	private void Start()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
		OnLocalize();
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		gameObjectToEnable.SetActive(value: false);
		for (int i = 0; i < targetLanguages.Length; i++)
		{
			if (targetLanguages[i] == Localizer.language)
			{
				gameObjectToEnable.SetActive(value: true);
				break;
			}
		}
	}
}
