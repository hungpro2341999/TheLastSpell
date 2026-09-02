using TheLastStand.Framework;
using UnityEngine;

namespace TheLastStand.View;

public class ItemView
{
	public static Sprite GetIngameSprite(string itemDefinitionId)
	{
		if (string.IsNullOrEmpty(itemDefinitionId))
		{
			return null;
		}
		Sprite sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Items/Weapon_" + itemDefinitionId);
		if (sprite != null)
		{
			return sprite;
		}
		return null;
	}

	public static Sprite GetUiSprite(string itemDefinitionId, bool isBG = false)
	{
		if (string.IsNullOrEmpty(itemDefinitionId))
		{
			return null;
		}
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Items/Icons/" + (isBG ? "Background" : "Foreground") + "/UI_Icon_Items_" + itemDefinitionId + "_" + (isBG ? "BG" : "FG"));
	}
}
