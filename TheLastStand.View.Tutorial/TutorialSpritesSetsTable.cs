using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TheLastStand.View.Tutorial;

[CreateAssetMenu(fileName = "New Tutorial Sprite Sets Table", menuName = "TLS/Tutorial/Tutorial Sprite Sets Table", order = 100)]
public class TutorialSpritesSetsTable : SerializedScriptableObject
{
	[SerializeField]
	private Dictionary<string, TutorialSpritesSet> table;

	public Dictionary<string, TutorialSpritesSet> Table => table;
}
