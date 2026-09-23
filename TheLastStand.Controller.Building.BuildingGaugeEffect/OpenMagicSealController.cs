using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.BuildingGaugeEffect;
using TheLastStand.Model.Building.Module;
using TheLastStand.Serialization;
using TheLastStand.View.Building.BuildingGaugeEffect;

namespace TheLastStand.Controller.Building.BuildingGaugeEffect;

/// <summary>
/// Controller xử lý hiệu ứng tiến trình "Mở Phong Ấn Ma Thuật" (Open Magic Seal) gắn liền với Vòng tròn ma thuật (Magic Circle).
/// Trong The Last Spell, các pháp sư tại Magic Circle niệm phép qua từng đêm để mở các lớp phong ấn chiến thắng map.
/// Lớp này ghi đè CanTriggerEffect() luôn trả về false vì việc mở phong ấn được điều phối bởi hệ thống sự kiện cốt truyện
/// và chu kỳ đêm riêng của MagicCircleManager chứ không tự động kích hoạt như các công trình sản xuất tài nguyên thông thường.
/// </summary>
public class OpenMagicSealController : BuildingGaugeEffectController
{
	#region Constructors

	/// <summary>
	/// Khởi tạo controller từ dữ liệu đã lưu (Save game).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa trạng thái phong ấn.</param>
	/// <param name="productionBuilding">Module sản xuất của Magic Circle liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng mở phong ấn.</param>
	public OpenMagicSealController(SerializedGaugeEffect container, ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new OpenMagicSeal(productionBuilding, definition, this, new OpenMagicSealView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
		base.BuildingGaugeEffect.Deserialize(container);
	}

	/// <summary>
	/// Khởi tạo mới controller khi bắt đầu màn chơi có Magic Circle.
	/// </summary>
	/// <param name="productionBuilding">Module sản xuất của Magic Circle liên kết.</param>
	/// <param name="definition">Định nghĩa cấu hình hiệu ứng mở phong ấn.</param>
	public OpenMagicSealController(ProductionModule productionBuilding, BuildingGaugeEffectDefinition definition)
	{
		base.BuildingGaugeEffect = new OpenMagicSeal(productionBuilding, definition, this, new OpenMagicSealView());
		base.BuildingGaugeEffect.BuildingGaugeEffectView.BuildingGaugeEffect = base.BuildingGaugeEffect;
	}

	#endregion

	#region Trigger Verification

	/// <summary>
	/// Kiểm tra điều kiện tự kích hoạt hiệu ứng thanh tiến độ.
	/// Luôn trả về False vì tiến trình mở phong ấn ma thuật được kích hoạt có chủ đích bởi quy trình kết thúc đêm của MagicCircle thay vì tự phát.
	/// </summary>
	/// <returns>Luôn là False.</returns>
	public override bool CanTriggerEffect()
	{
		return false;
	}

	#endregion
}

