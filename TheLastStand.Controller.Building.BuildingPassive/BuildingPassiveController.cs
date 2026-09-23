using TheLastStand.Definition.Building.BuildingPassive;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Building.BuildingPassive;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization.Building;

namespace TheLastStand.Controller.Building.BuildingPassive;

/// <summary>
/// Bộ điều khiển cho một hiệu ứng bị động / nội tại (Building Passive) của công trình.
/// Quản lý vòng đời kích hoạt các Trigger (OnCreation, OnDeath, StartTurn, EndTurn...) 
/// và điều phối thực thi các PassiveEffect con tương ứng trên công trình hoặc toàn cục.
/// </summary>
public class BuildingPassiveController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu của hiệu ứng nội tại công trình.
	/// </summary>
	public TheLastStand.Model.Building.BuildingPassive.BuildingPassive BuildingPassive { get; protected set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo BuildingPassiveController từ dữ liệu lưu trữ (Save Game Deserialization).
	/// </summary>
	/// <param name="serializedPassive">Dữ liệu đã tuần tự hóa của Passive.</param>
	/// <param name="buildingPassivesModule">Module nội tại của công trình cha sở hữu.</param>
	/// <param name="saveVersion">Phiên bản save để đảm bảo tương thích ngược.</param>
	public BuildingPassiveController(SerializedBuildingPassive serializedPassive, PassivesModule buildingPassivesModule, int saveVersion)
	{
		BuildingPassive = new TheLastStand.Model.Building.BuildingPassive.BuildingPassive(serializedPassive, buildingPassivesModule, this, saveVersion);
	}

	/// <summary>
	/// Khởi tạo BuildingPassiveController khi công trình được tạo mới trong trận đấu.
	/// Tự động kích hoạt ngay các trigger tại thời điểm tạo (OnCreation).
	/// </summary>
	/// <param name="buildingPassivesModule">Module nội tại của công trình cha sở hữu.</param>
	/// <param name="buildingPassiveDefinition">Định nghĩa cấu hình gốc của Passive.</param>
	public BuildingPassiveController(PassivesModule buildingPassivesModule, BuildingPassiveDefinition buildingPassiveDefinition)
	{
		BuildingPassive = new TheLastStand.Model.Building.BuildingPassive.BuildingPassive(buildingPassivesModule, buildingPassiveDefinition, this);
		Trigger(E_EffectTime.OnCreation);
	}

	#endregion

	#region Effect Management & Execution

	/// <summary>
	/// Cải thiện / tăng cường giá trị hiệu lực cho tất cả các PassiveEffect con (thường khi nâng cấp công trình).
	/// </summary>
	/// <param name="bonus">Giá trị cộng thêm.</param>
	public void ImproveEffects(int bonus)
	{
		for (int i = 0; i < BuildingPassive.PassiveEffects.Count; i++)
		{
			BuildingPassive.PassiveEffects[i].BuildingPassiveEffectController.ImproveEffect(bonus);
		}
	}

	/// <summary>
	/// Kích hoạt chuỗi hiệu ứng nội tại theo thời điểm kích hoạt chỉ định (EffectTime).
	/// </summary>
	/// <param name="effectTime">Thời điểm xảy ra sự kiện (OnCreation, StartTurn, EndTurn, OnDeath...).</param>
	/// <param name="force">Cờ ép buộc kích hoạt bỏ qua việc kiểm tra điều kiện trigger.</param>
	/// <param name="OnLoad">Cờ đánh dấu có phải đang trong quá trình tải lại ván chơi hay không.</param>
	public void Trigger(E_EffectTime effectTime, bool force = false, bool OnLoad = false)
	{
		// Không thực thi nội tại công trình khi đang ở chế độ biên tập màn chơi (Level Editor)
		if (ApplicationManager.Application.State.GetName() == "LevelEditor")
		{
			return;
		}

		// Duyệt qua tất cả các Trigger đã đăng ký cho Passive này
		for (int i = 0; i < BuildingPassive.PassiveTriggers.Count; i++)
		{
			// Kiểm tra nếu được ép buộc hoặc khớp thời điểm và thỏa mãn điều kiện kích hoạt
			if (force || (effectTime == BuildingPassive.PassiveTriggers[i].PassiveTriggerDefinition.EffectTime && BuildingPassive.PassiveTriggers[i].PassiveTriggerController.UpdateAndCheckTrigger(OnLoad)))
			{
				// Áp dụng lần lượt từng hiệu ứng con tương ứng
				for (int j = 0; j < BuildingPassive.PassiveEffects.Count; j++)
				{
					BuildingPassive.PassiveEffects[j].BuildingPassiveEffectController.Apply();
				}
			}
		}

		// Xử lý sự kiện dọn dẹp riêng khi công trình bị phá hủy (OnDeath)
		if (effectTime != E_EffectTime.OnDeath)
		{
			return;
		}

		for (int k = 0; k < BuildingPassive.PassiveTriggers.Count; k++)
		{
			for (int l = 0; l < BuildingPassive.PassiveEffects.Count; l++)
			{
				BuildingPassive.PassiveEffects[l].BuildingPassiveEffectController.OnDeath();
			}
		}
	}

	/// <summary>
	/// Hoàn tác / hủy bỏ các hiệu ứng nội tại vĩnh viễn (Permanent) khi công trình bị dỡ bỏ hoặc bán.
	/// </summary>
	public void UndoPermanentPassiveEffects()
	{
		for (int i = 0; i < BuildingPassive.PassiveTriggers.Count; i++)
		{
			if (BuildingPassive.PassiveTriggers[i].PassiveTriggerDefinition.EffectTime == E_EffectTime.Permanent)
			{
				for (int j = 0; j < BuildingPassive.PassiveEffects.Count; j++)
				{
					BuildingPassive.PassiveEffects[j].BuildingPassiveEffectController.Unapply();
				}
			}
		}
	}

	#endregion
}
