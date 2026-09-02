using System.Collections.Generic;
using TPLib;
using UnityEngine;

namespace TheLastStand.View;

public class EvolutiveLevelArtObject : TPSingleton<EvolutiveLevelArtObject>
{
	public enum ActiveMode
	{
		DEFAULT,
		ACTIVE,
		DISABLED
	}

	[SerializeField]
	private int initialStageIndex;

	[SerializeField]
	private ActiveMode defaultStageMode;

	[SerializeField]
	private List<GameObject> stages = new List<GameObject>();

	private int stageIndex;

	public int StageIndex
	{
		get
		{
			return stageIndex;
		}
		private set
		{
			if (stageIndex == value)
			{
				SetActiveCurrentStageObject(value: true);
				return;
			}
			OnEvolutionStageChange(stageIndex, value);
			stageIndex = value;
		}
	}

	private void Start()
	{
		base.Awake();
		stageIndex = initialStageIndex;
		for (int i = 0; i < stages.Count; i++)
		{
			if (i == initialStageIndex)
			{
				switch (defaultStageMode)
				{
				case ActiveMode.ACTIVE:
					SetActiveCurrentStageObject(value: true);
					break;
				case ActiveMode.DISABLED:
					SetActiveCurrentStageObject(value: false);
					break;
				}
			}
			else
			{
				stages[i].SetActive(value: false);
			}
		}
	}

	public void OnEvolutionStageChange(int oldValue, int newValue)
	{
		stages[oldValue].SetActive(value: false);
		stages[newValue].SetActive(value: true);
	}

	public void SetActiveCurrentStageObject(bool value)
	{
		stages[StageIndex].SetActive(value);
	}

	public int SetEvolutionStage(int index)
	{
		StageIndex = Mathf.Clamp(index, 0, stages.Count);
		return StageIndex;
	}
}
