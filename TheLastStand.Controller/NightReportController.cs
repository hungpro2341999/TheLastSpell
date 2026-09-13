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

/// <summary>
/// Bộ điều khiển tổng kết đêm (Night Report): tính toán xếp hạng chiến đấu (Battle Rank),
/// mức độ hoảng loạn (Panic Rank) và xếp hạng tổng thể (Tonight Rank - S, A, B, C, D) sau mỗi đêm phòng thủ.
/// </summary>
public class NightReportController
{
	#region Constants & Properties

	/// <summary>
	/// Các hằng số hiển thị nhãn xếp hạng.
	/// </summary>
	public static class Constants
	{
		/// <summary>Nhãn xếp hạng tương ứng với chỉ số (0 -> S, 1 -> A, 2 -> B, 3 -> C, 4 -> D).</summary>
		public static readonly string[] IndexToRankLabels = new string[5] { "S", "A", "B", "C", "D" };
	}

	/// <summary>
	/// Model lưu trữ dữ liệu báo cáo đêm hiện tại.
	/// </summary>
	public NightReport NightReport { get; private set; }

	#endregion

	#region Constructor

	/// <summary>
	/// Khởi tạo NightReportController và tạo Model NightReport tương ứng.
	/// </summary>
	public NightReportController()
	{
		NightReport = new NightReport(this);
	}

	#endregion

	#region Rank Computation & UI Management

	/// <summary>
	/// Tính toán xếp hạng cho đêm vừa kết thúc:
	/// 1. Tỷ lệ máu mất của các Hero còn sống và đã chết so với tổng máu tối đa.
	/// 2. Phạt tụt hạng cho mỗi Hero bị tử trận trong đêm.
	/// 3. Lấy chỉ số Panic Rank dựa trên cấp độ hoảng loạn của thành phố.
	/// 4. Tính điểm TonightRank trung bình cộng giữa BattleRank và PanicRank.
	/// </summary>
	public void GetTonightRanks()
	{
		// Lấy danh sách tướng đã tử trận trong ngày hôm nay
		TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.TryGetValue(TPSingleton<GameManager>.Instance.DayNumber, out var value);
		
		// Tính tổng lượng máu tối đa của toàn đội hình
		float tonightHpMax = 0f;
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
		{
			tonightHpMax += o.HealthTotal;
		});
		value?.ForEach(delegate(PlayableUnit o)
		{
			tonightHpMax += o.HealthTotal;
		});
		
		// Tính tỷ lệ phần trăm máu bị tổn thất trong đêm
		float num = NightReport.TonightHpLost / Mathf.Max(tonightHpMax, 1f);
		int num2 = 0;
		int count = GameDatabase.NightReportRankDefinitions.Count;
		
		// Tra cứu tỷ lệ mất máu vào bảng định nghĩa xếp hạng (S, A, B, C, D)
		for (int num3 = 0; num3 < count; num3++)
		{
			if (num3 == count - 1 || num < GameDatabase.NightReportRankDefinitions[num3].MaxHPsLostRatio * 0.01f)
			{
				num2 = num3;
				break;
			}
		}
		
		// PanicRank lấy trực tiếp từ cấp độ hoảng loạn của PanicManager
		NightReport.PanicRank = PanicManager.Panic.Level;
		
		// BattleRank = Hạng mất máu + số tướng bị chết (càng chết nhiều hạng càng tụt)
		NightReport.BattleRank = Mathf.Min(num2 + (value?.Count ?? 0), count - 1);
		
		// TonightRank = Trung bình cộng làm tròn lên giữa BattleRank và PanicRank
		NightReport.TonightRank = Mathf.CeilToInt((float)(NightReport.BattleRank + NightReport.PanicRank) * 0.5f);
		
		CLoggerManager.Log("Health ratio ranks: " + string.Join(",", GameDatabase.NightReportRankDefinitions.Select((NightReportRankDefinition o) => o.MaxHPsLostRatio)), LogType.Log, CLogLevel.DETAILED);
		CLoggerManager.Log("NightReport Rank computations results:" + $"\n- HP Lost: {NightReport.TonightHpLost} / HP Max: {tonightHpMax}" + "\n- Health lost ratio: " + num.ToString("f2") + "% => resulting in a HP rank of " + Constants.IndexToRankLabels[num2] + $"\n- Adding {value?.Count ?? 0} dead unit(s) => resulting in a battle rank of {Constants.IndexToRankLabels[NightReport.BattleRank]}" + "\n- Panic rank: " + Constants.IndexToRankLabels[NightReport.PanicRank] + "\n- Tonight rank: (" + Constants.IndexToRankLabels[NightReport.PanicRank] + "+" + Constants.IndexToRankLabels[NightReport.BattleRank] + ")/2 = " + Constants.IndexToRankLabels[NightReport.TonightRank], LogType.Log, CLogLevel.DETAILED);
	}

	/// <summary>
	/// Đóng bảng tổng kết đêm, hiển thị lại HUD công trình và chuyển sang ban ngày (Production phase).
	/// </summary>
	public void CloseNightReportPanel()
	{
		BuildingManager.DisplayBuildingsHudsIfNeeded();
		TPSingleton<NightReportPanel>.Instance.Close();
		TPSingleton<ToDoListView>.Instance.SwitchRaycastTargetState(state: false);
		TPSingleton<GameManager>.Instance.FinalizeDayTransition();
	}

	#endregion
}
