using System.Collections.Generic;
using Sirenix.OdinInspector;
using TheLastStand.Controller;
using TheLastStand.Definition.Item;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand;

public class TestProbabilitiesTree : SerializedMonoBehaviour
{
	private class SomeLevelOwner : ILevelOwner
	{
		public int Level { get; set; }

		public string Name => "SomeTestLevelOwner";

		public SomeLevelOwner(int level)
		{
			Level = level;
		}
	}

	[SerializeField]
	private int initLevel = 1;

	[SerializeField]
	private Dictionary<int, int> modifiers = new Dictionary<int, int>();

	[SerializeField]
	private Dictionary<int, int> rarityModifiers = new Dictionary<int, int>();

	[Space(15f)]
	[SerializeField]
	private int levelUpsBeforeGen;

	[SerializeField]
	private int generationCount = 10000;

	[ContextMenu("Generate")]
	private void Generate()
	{
		SomeLevelOwner someLevelOwner = new SomeLevelOwner(initLevel);
		LevelProbabilitiesTreeController levelProbabilitiesTreeController = new LevelProbabilitiesTreeController(someLevelOwner, modifiers);
		for (int num = levelUpsBeforeGen - 1; num >= 0; num--)
		{
			someLevelOwner.Level++;
		}
		SortedDictionary<int, int> sortedDictionary = new SortedDictionary<int, int>();
		for (int i = 0; i < generationCount; i++)
		{
			int key = levelProbabilitiesTreeController.GenerateLevel();
			if (!sortedDictionary.ContainsKey(key))
			{
				sortedDictionary.Add(key, 0);
			}
			sortedDictionary[key]++;
		}
		levelProbabilitiesTreeController.Log();
		string text = $"<b>#--- GENERATION OF {generationCount} ITEMS ---#</b>\n";
		foreach (KeyValuePair<int, int> item in sortedDictionary)
		{
			text = text + $"Generated <b>{item.Value}</b> items of level <b>{item.Key}</b> " + $"(~{Mathf.RoundToInt((float)item.Value / (float)generationCount * 100f)}%).\n";
		}
		Debug.Log(text);
	}

	[ContextMenu("Generate rarity")]
	private void GenerateRarity()
	{
		SortedDictionary<ItemDefinition.E_Rarity, int> sortedDictionary = new SortedDictionary<ItemDefinition.E_Rarity, int>();
		for (int i = 0; i < generationCount; i++)
		{
			ItemDefinition.E_Rarity key = RarityProbabilitiesTreeController.GenerateRarity(rarityModifiers);
			if (!sortedDictionary.ContainsKey(key))
			{
				sortedDictionary.Add(key, 0);
			}
			sortedDictionary[key]++;
		}
		foreach (KeyValuePair<ItemDefinition.E_Rarity, int> item in sortedDictionary)
		{
			Debug.Log($"{item.Value} {item.Key.ToString()}");
		}
	}

	private void Start()
	{
		Generate();
	}
}
