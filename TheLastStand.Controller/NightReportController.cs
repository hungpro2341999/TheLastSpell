using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Definition.Night;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.NightReport;
using TheLastStand.View.ToDoList;
using UnityEngine;

namespace TheLastStand.Controller;

public class NightReportController
{
	public static class Constants
	{
		public static readonly string[] IndexToRankLabels = new string[5] { "S", "A", "B", "C", "D" };
	}

	public NightReport NightReport { get; private set; }

	public NightReportController()
	{
		NightReport = new NightReport(this);
	}

	public void GetTonightRanks()
	{
		TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.TryGetValue(TPSingleton<GameManager>.Instance.DayNumber, out var value);
		float tonightHpMax = 0f;
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
		{
			tonightHpMax += o.HealthTotal;
		});
		value?.ForEach(delegate(PlayableUnit o)
		{
			tonightHpMax += o.HealthTotal;
		});
		float num = NightReport.TonightHpLost / Mathf.Max(tonightHpMax, 1f);
		int num2 = 0;
		int count = GameDatabase.NightReportRankDefinitions.Count;
		for (int num3 = 0; num3 < count; num3++)
		{
			if (num3 == count - 1 || num < GameDatabase.NightReportRankDefinitions[num3].MaxHPsLostRatio * 0.01f)
			{
				num2 = num3;
				break;
			}
		}
		NightReport.PanicRank = PanicManager.Panic.Level;
		NightReport.BattleRank = Mathf.Min(num2 + (value?.Count ?? 0), count - 1);
		NightReport.TonightRank = Mathf.CeilToInt((float)(NightReport.BattleRank + NightReport.PanicRank) * 0.5f);
		CLoggerManager.Log("Health ratio ranks: " + string.Join(",", GameDatabase.NightReportRankDefinitions.Select((NightReportRankDefinition o) => o.MaxHPsLostRatio)), LogType.Log, CLogLevel.DETAILED);
		CLoggerManager.Log("NightReport Rank computations results:" + $"\n- HP Lost: {NightReport.TonightHpLost} / HP Max: {tonightHpMax}" + "\n- Health lost ratio: " + num.ToString("f2") + "% => resulting in a HP rank of " + Constants.IndexToRankLabels[num2] + $"\n- Adding {value?.Count ?? 0} dead unit(s) => resulting in a battle rank of {Constants.IndexToRankLabels[NightReport.BattleRank]}" + "\n- Panic rank: " + Constants.IndexToRankLabels[NightReport.PanicRank] + "\n- Tonight rank: (" + Constants.IndexToRankLabels[NightReport.PanicRank] + "+" + Constants.IndexToRankLabels[NightReport.BattleRank] + ")/2 = " + Constants.IndexToRankLabels[NightReport.TonightRank], LogType.Log, CLogLevel.DETAILED);
	}

	public void CloseNightReportPanel()
	{
		BuildingManager.DisplayBuildingsHudsIfNeeded();
		TPSingleton<NightReportPanel>.Instance.Close();
		TPSingleton<ToDoListView>.Instance.SwitchRaycastTargetState(state: false);
		TPSingleton<GameManager>.Instance.FinalizeDayTransition();
	}
}
