using System;
using TMPro;
using TPLib;
using TheLastStand.Database;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Dev.View;

public class MetaUpgradeDebugConditionLineView : MonoBehaviour
{
	[SerializeField]
	private Slider completionSlider;

	[SerializeField]
	private Sprite unfilfilledStatusIcon;

	[SerializeField]
	private Sprite fulfilledStatusIcon;

	[SerializeField]
	private Image statusLogo;

	[SerializeField]
	private TextMeshProUGUI contentTextMesh;

	public MetaCondition MetaCondition { get; private set; }

	[ContextMenu("Refresh")]
	public void Refresh()
	{
		MetaConditionsDatabase.ProgressionDatas progressionValues = MetaCondition.MetaConditionController.GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary);
		contentTextMesh.text = $"{MetaCondition.MetaConditionDefinition}\r\nStatus: {progressionValues.ProgressionValueToString()}/{progressionValues.GoalValueToString()}\r\nOccurence progression: {MetaCondition.OccurenceProgression}/{MetaCondition.MetaConditionDefinition.Occurences}";
		statusLogo.sprite = (MetaCondition.MetaConditionController.IsComplete() ? fulfilledStatusIcon : unfilfilledStatusIcon);
		completionSlider.value = (float)(Convert.ToDouble(progressionValues.ProgressionValueToString()) / Convert.ToDouble(progressionValues.GoalValueToString()));
	}

	public void Set(MetaCondition condition)
	{
		MetaCondition = condition;
		base.name = MetaCondition.ToString();
	}
}
