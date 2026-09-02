using System.Collections.Generic;
using TPLib;
using TheLastStand.Framework;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Skill;
using TheLastStand.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class SkillAreaOfEffectGrid : MonoBehaviour
{
	public class Constants
	{
		public const string SkillAreaOfEffectCellPoolId = "SkillAoeCellPrefab";
	}

	[SerializeField]
	private Image cellPrefab;

	[SerializeField]
	private SkillAreaOfEffectResourcesByGridSize skillAreaOfEffectResourcesByGridSize;

	private Canvas canvas;

	private SkillAreaOfEffectResourcesByGridSize.GridSizes gridSize;

	private bool usePooler;

	public RectTransform RectTransform => base.transform as RectTransform;

	public bool Displayed { get; private set; }

	public Canvas Canvas
	{
		get
		{
			if (canvas == null)
			{
				canvas = GetComponent<Canvas>();
			}
			return canvas;
		}
	}

	public void Refresh(TheLastStand.Model.Skill.Skill skill)
	{
		int count = skill.SkillDefinition.AreaOfEffectDefinition.Pattern[0].Count;
		int count2 = skill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count;
		Displayed = ShouldDisplayAreaOfEffectGrid(skill.SkillDefinition.AreaOfEffectDefinition.Pattern);
		base.gameObject.SetActive(Displayed);
		Vector2 vector = new Vector2(Mathf.Floor((float)count / 2f), Mathf.Floor((float)count2 / 2f));
		if (count % 2 != 0 && count2 % 2 == 0)
		{
			vector.y -= 1f;
		}
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			if (usePooler)
			{
				base.transform.GetChild(num).gameObject.SetActive(value: false);
			}
			else
			{
				Object.Destroy(base.transform.GetChild(num).gameObject);
			}
		}
		if (!skillAreaOfEffectResourcesByGridSize.ResourcesByGridSize.TryGetValue(gridSize, out var value))
		{
			TPSingleton<SkillManager>.Instance.LogError($"Can't find Resources for this grid size : {gridSize} !");
			return;
		}
		for (int i = 0; i < count; i++)
		{
			for (int j = 0; j < count2; j++)
			{
				Sprite sprite = skill.SkillDefinition.AreaOfEffectDefinition.Pattern[count2 - 1 - j][i] switch
				{
					'X' => value.AreaOfEffectSprite, 
					'M' => value.ManeuverSprite, 
					'e' => value.SurroundingSprite, 
					_ => null, 
				};
				if (sprite != null)
				{
					Vector2 vector2 = new Vector2(value.GridCenter.x + ((float)i - vector.x) * (float)value.CellSize, value.GridCenter.y - ((float)j - vector.y) * (float)value.CellSize);
					Image obj = (usePooler ? ObjectPooler.GetPooledComponent("SkillAoeCellPrefab", cellPrefab, base.transform) : Object.Instantiate(cellPrefab, base.transform));
					obj.transform.localPosition = vector2;
					obj.sprite = sprite;
				}
			}
		}
	}

	private void Awake()
	{
		usePooler = SingletonBehaviour<ObjectPooler>.Instance != null;
	}

	private bool ShouldDisplayAreaOfEffectGrid(List<List<char>> pattern)
	{
		int num = 0;
		for (int i = 0; i < pattern.Count; i++)
		{
			for (int j = 0; j < pattern[i].Count; j++)
			{
				if (pattern[i][j] != '_')
				{
					num++;
				}
			}
		}
		return num > 1;
	}
}
