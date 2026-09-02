using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheLastStand.ScriptableObjects;

[CreateAssetMenu(fileName = "NewSpritesLocalizedDictionnary", menuName = "TLS/Sprites Localized Dictionnary", order = -800)]
public class SpritesLocalizedDictionnary : SerializedScriptableObject
{
	public struct KeySprite
	{
		public string Key;

		public Sprite Sprite;
	}

	public Dictionary<string, List<KeySprite>> SpritesByLanguages = new Dictionary<string, List<KeySprite>>();
}
