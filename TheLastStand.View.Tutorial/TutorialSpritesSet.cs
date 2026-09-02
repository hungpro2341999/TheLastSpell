using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TheLastStand.View.HUD;
using UnityEngine;

namespace TheLastStand.View.Tutorial;

[CreateAssetMenu(fileName = "New Tutorial Sprites Set", menuName = "TLS/Tutorial/Tutorial Sprites Set", order = 100)]
public class TutorialSpritesSet : SerializedScriptableObject
{
	[Serializable]
	private struct SpriteList
	{
		public List<SpriteToDisplay> Sprites;
	}

	[Serializable]
	private struct SpriteToDisplay
	{
		public Sprite Sprite;

		public GamepadButtonsSet controllerIcon;
	}

	[SerializeField]
	private List<SpriteList> joystickSpritesRows;

	public List<Sprite> GetSprites(int row)
	{
		if (joystickSpritesRows.Count <= row)
		{
			return null;
		}
		return joystickSpritesRows[row].Sprites.Select((SpriteToDisplay sprite) => (!(sprite.Sprite != null)) ? sprite.controllerIcon.GetIcon() : sprite.Sprite).ToList();
	}
}
