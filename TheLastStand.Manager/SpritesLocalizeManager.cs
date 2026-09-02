using System;
using TPLib.Localization;
using TheLastStand.Framework.Extensions;
using TheLastStand.ScriptableObjects;
using UnityEngine;

namespace TheLastStand.Manager;

public class SpritesLocalizeManager : Manager<SpritesLocalizeManager>
{
	[SerializeField]
	private SpritesLocalizedDictionnary spritesLocalizedDictionnary;

	private string CurrentLanguage => Localizer.language;

	public Sprite Get(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		if (spritesLocalizedDictionnary.SpritesByLanguages.TryGetValue(CurrentLanguage, out var value))
		{
			if (value.TryFind((SpritesLocalizedDictionnary.KeySprite x) => x.Key == key, out var value2))
			{
				return value2.Sprite;
			}
			return null;
		}
		if (string.IsNullOrEmpty(CurrentLanguage))
		{
			LogError("CurrentLanguage has no value !");
		}
		else
		{
			LogError(CurrentLanguage + " : this language isn't present in dictonnary !");
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
	}
}
