using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheLastStand.ScriptableObjects;

[CreateAssetMenu(fileName = "SkillAreaOfEffectResourcesByGridSize", menuName = "TLS/SkillAreaOfEffectResources By GridSize", order = -800)]
public class SkillAreaOfEffectResourcesByGridSize : SerializedScriptableObject
{
	public enum GridSizes
	{
		Seven
	}

	[Serializable]
	public class ResourcesForSkillAreaOfEffectGrid
	{
		[SerializeField]
		private Sprite areaOfEffectSprite;

		[SerializeField]
		private Sprite maneuverSprite;

		[SerializeField]
		private Sprite surroundingSprite;

		[SerializeField]
		private Sprite gridSprite;

		[SerializeField]
		private Vector2 gridCenter = Vector2.zero;

		[SerializeField]
		private int cellSize;

		public Sprite AreaOfEffectSprite => areaOfEffectSprite;

		public Sprite ManeuverSprite => maneuverSprite;

		public Sprite SurroundingSprite => surroundingSprite;

		public Sprite GridSprite => gridSprite;

		public Vector2 GridCenter => gridCenter;

		public int CellSize => cellSize;
	}

	public Dictionary<GridSizes, ResourcesForSkillAreaOfEffectGrid> ResourcesByGridSize = new Dictionary<GridSizes, ResourcesForSkillAreaOfEffectGrid>();
}
