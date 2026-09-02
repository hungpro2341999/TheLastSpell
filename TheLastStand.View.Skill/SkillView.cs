using TheLastStand.Framework;
using UnityEngine;

namespace TheLastStand.View.Skill;

public class SkillView
{
	public static class Constants
	{
		public const string UIPlaceholderSpritePath = "View/Sprites/UI/Skills/Tmp/PatternPlaceholder";
	}

	public static Sprite GetIconSprite(string skillDefinitionId)
	{
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/Icons/" + skillDefinitionId);
	}

	public static Sprite GetPatternSprite(string skillDefinitionId, int level)
	{
		Sprite sprite = ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Skills/Patterns/{skillDefinitionId}_Lvl{level}");
		if (sprite != null)
		{
			return sprite;
		}
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/Tmp/PatternPlaceholder");
	}
}
