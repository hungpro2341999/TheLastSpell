using System.Collections.Generic;
using UnityEngine;

namespace TheLastStand.Definition.Skill;

public class AreaOfEffectDefinition
{
	public Vector2Int Origin { get; set; }

	public List<List<char>> Pattern { get; set; }

	public bool IsSingleTarget { get; set; }
}
