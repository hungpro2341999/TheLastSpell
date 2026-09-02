using System;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListWavesNotificationView : ToDoListNotificationView
{
	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn != Game.E_DayTurn.Undefined && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			return;
		}
		for (int num = toDoTexts.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(toDoTexts[num].gameObject);
		}
		toDoTexts.Clear();
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		foreach (SpawnDirectionsDefinition.E_Direction eDirection in SpawnDirectionsDefinition.OrderedDirections)
		{
			SpawnWaveView.SpawnWaveArrowPair spawnWaveArrowPair = SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks.FirstOrDefault((SpawnWaveView.SpawnWaveArrowPair x) => x.SpawnWaveDetailedZone.IsCentralZone() && x.CentralSpawnDirection == eDirection);
			if (currentSpawnWave.RotatedProportionPerDirection.ContainsKey(spawnWaveArrowPair.CentralSpawnDirection))
			{
				TextMeshProUGUI textMeshProUGUI = UnityEngine.Object.Instantiate(toDoTextPrefab, notificationGroupRectTransform);
				textMeshProUGUI.text = "- " + SpawnWave.GetLocalizedDirectionName(spawnWaveArrowPair.CentralSpawnDirection);
				toDoTexts.Add(textMeshProUGUI);
			}
		}
		CheckDisplay();
	}

	private void Awake()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Combine(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		Localizer.onLocalize = (Localizer.OnLocalizeNotification)Delegate.Remove(Localizer.onLocalize, new Localizer.OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		Refresh();
	}
}
