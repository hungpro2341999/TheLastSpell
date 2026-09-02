using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Dev;

public class TestHashedRandomRange : MonoBehaviour
{
	[SerializeField]
	private int mapSize = 10;

	[SerializeField]
	private float minValue;

	[SerializeField]
	private float maxValue = 1f;

	[ContextMenu("Generate")]
	private void Generate()
	{
		string text = $"<b>#--- GENERATION OF {mapSize}x{mapSize} values ---#</b>\n";
		new SortedDictionary<int, int>();
		for (int i = 0; i < mapSize; i++)
		{
			for (int j = 0; j < mapSize; j++)
			{
				text += $"[{i};{j}]: {RandomManager.GetPositionBasedRandomRange(new Vector3Int(i, j, 0), minValue, maxValue)} ";
			}
			text += "\n";
		}
		Debug.Log(text);
	}

	private void Start()
	{
		TPSingleton<RandomManager>.Instance.Deserialize();
		Generate();
	}
}
