using TheLastStand.Controller.Building.Module;
using TheLastStand.Definition.Building;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.Building;
using TheLastStand.View.Building;

namespace TheLastStand.Controller.Building;

/// <summary>
/// Bộ điều khiển trung tâm cho một thực thể công trình (Building / Magic Circle).
/// Quản lý vòng đời khởi tạo, tải dữ liệu lưu (deserialization), điều phối các Module chức năng 
/// (Blueprint, Construction, Damageable, Upgrade, Passives, Production, Battle) và các pha theo lượt (StartTurn, EndTurn).
/// </summary>
public class BuildingController
{
	#region Properties & Module Controllers

	/// <summary>
	/// Model dữ liệu đại diện cho công trình (hoặc MagicCircle nếu là Vòng phép trung tâm).
	/// </summary>
	public TheLastStand.Model.Building.Building Building { get; private set; }

	/// <summary>
	/// View hiển thị tương ứng của công trình trên scene Unity.
	/// </summary>
	public BuildingView BuildingView => Building.BuildingView;

	/// <summary>
	/// Bộ điều khiển module bản vẽ / sơ đồ (quản lý kích thước, ô tile chiếm dụng trên bản đồ).
	/// </summary>
	public BlueprintModuleController BlueprintModuleController { get; private set; }

	/// <summary>
	/// Bộ điều khiển module thi công / xây dựng (quản lý việc xây, sửa chữa, dỡ bỏ công trình).
	/// </summary>
	public ConstructionModuleController ConstructionModuleController { get; private set; }

	/// <summary>
	/// Bộ điều khiển module chịu sát thương (quản lý máu, giáp, giảm trừ sát thương và bị phá hủy).
	/// </summary>
	public DamageableModuleController DamageableModuleController { get; private set; }

	/// <summary>
	/// Bộ điều khiển module nâng cấp (quản lý các nâng cấp cục bộ của công trình và nâng cấp toàn cục).
	/// </summary>
	public UpgradeModuleController UpgradeModuleController { get; private set; }

	/// <summary>
	/// Bộ điều khiển module nội tại / bị động (quản lý các aura, buff hỗ trợ hoặc debuff xung quanh).
	/// </summary>
	public PassivesModuleController PassivesModuleController { get; private set; }

	/// <summary>
	/// Bộ điều khiển module sản xuất (quản lý sinh tài nguyên: vàng, vật liệu, trang bị, thanh tích lũy gauge).
	/// </summary>
	public ProductionModuleController ProductionModuleController { get; private set; }

	/// <summary>
	/// Bộ điều khiển module chiến đấu (quản lý hành vi tấn công của tháp canh, bẫy, mục tiêu và sát thương).
	/// </summary>
	public BattleModuleController BattleModuleController { get; private set; }

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo BuildingController từ dữ liệu lưu trữ (Save Game Deserialization).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của công trình từ tệp lưu.</param>
	/// <param name="buildingDefinition">Định nghĩa cấu hình gốc của công trình.</param>
	/// <param name="buildingView">View tương ứng trên Unity scene.</param>
	/// <param name="tile">Ô Tile gốc tọa độ đặt công trình.</param>
	/// <param name="saveVersion">Phiên bản của bản lưu để đảm bảo tính tương thích ngược.</param>
	public BuildingController(SerializedBuilding container, BuildingDefinition buildingDefinition, BuildingView buildingView, Tile tile, int saveVersion)
	{
		// Kiểm tra nếu là Vòng tròn phép thuật (Magic Circle) thì khởi tạo model MagicCircle riêng biệt
		Building = ((buildingDefinition is MagicCircleDefinition) ? new MagicCircle(container, this, buildingView as MagicCircleView) : new TheLastStand.Model.Building.Building(container, this, buildingView)
		{
			OriginTile = tile
		});
		Building.Init(container);
		if (buildingView != null)
		{
			buildingView.name = Building.UniqueIdentifier;
		}
		ReferenceModules();
		DeserializeModules(container, saveVersion);
	}

	/// <summary>
	/// Khởi tạo BuildingController khi tạo mới công trình trong quá trình chơi (New Construction).
	/// </summary>
	/// <param name="buildingDefinition">Định nghĩa cấu hình gốc của công trình.</param>
	/// <param name="buildingView">View tương ứng trên Unity scene.</param>
	/// <param name="tile">Ô Tile gốc tọa độ đặt công trình.</param>
	public BuildingController(BuildingDefinition buildingDefinition, BuildingView buildingView, Tile tile)
	{
		// Kiểm tra nếu là Vòng tròn phép thuật thì khởi tạo model MagicCircle, ngược lại tạo Building tiêu chuẩn
		Building = ((buildingDefinition is MagicCircleDefinition magicCircleDefinition) ? new MagicCircle(magicCircleDefinition, this, buildingView as MagicCircleView, tile) : new TheLastStand.Model.Building.Building(buildingDefinition, this, buildingView, tile));
		Building.Init();
		if (buildingView != null)
		{
			buildingView.name = Building.UniqueIdentifier;
		}
		ReferenceModules();
		InitializeModules();
	}

	#endregion

	#region Module Management & Deserialization

	/// <summary>
	/// Liên kết và lưu trữ tham chiếu đến các Controller của từng Module thành phần từ model Building.
	/// </summary>
	private void ReferenceModules()
	{
		BlueprintModuleController = Building.BlueprintModule.BlueprintModuleController;
		ConstructionModuleController = Building.ConstructionModule.ConstructionModuleController;
		DamageableModuleController = Building.DamageableModule?.DamageableModuleController;
		UpgradeModuleController = Building.UpgradeModule?.UpgradeModuleController;
		PassivesModuleController = Building.PassivesModule?.PassivesModuleController;
		ProductionModuleController = Building.ProductionModule?.ProductionModuleController;
		BattleModuleController = Building.BattleModule?.BattleModuleController;
	}

	/// <summary>
	/// Khởi tạo dữ liệu và trạng thái ban đầu cho các module khi công trình được xây mới.
	/// </summary>
	private void InitializeModules()
	{
		if (ProductionModuleController != null)
		{
			ProductionModuleController.CreateGaugeEffect();
			ProductionModuleController.CreateActions();
		}
		PassivesModuleController?.CreatePassives();
		UpgradeModuleController?.CreateUpgrades();
		BattleModuleController?.CreateGoals();
		BattleModuleController?.HookToModifyingDamagePerks();
	}

	/// <summary>
	/// Phục hồi lại dữ liệu và trạng thái của từng module từ dữ liệu tuần tự hóa (Save Container).
	/// </summary>
	/// <param name="container">Dữ liệu tuần tự hóa của công trình.</param>
	/// <param name="saveVersion">Phiên bản save để xử lý tương thích.</param>
	private void DeserializeModules(SerializedBuilding container, int saveVersion)
	{
		if (ProductionModuleController != null)
		{
			ProductionModuleController.CreateActions();
			ProductionModuleController.DeserializeGaugeEffect(container.GaugeEffect);
		}
		PassivesModuleController?.DeserializePassive(container.Passives, saveVersion);
		UpgradeModuleController?.DeserializeUpgrades(container.Upgrades);
		UpgradeModuleController?.DeserializeGlobalUpgrades(container.GlobalUpgrades);
		ProductionModuleController?.DeserializeUsedActions(container.Actions);
	}

	#endregion

	#region Turn Lifecycle Methods

	/// <summary>
	/// Kích hoạt khi bắt đầu lượt mới: Thông báo tới các module chiến đấu, sản xuất và nội tại để làm mới trạng thái.
	/// </summary>
	public void StartTurn()
	{
		BattleModuleController?.StartTurn();
		ProductionModuleController?.StartTurn();
		PassivesModuleController?.StartTurn();
	}

	/// <summary>
	/// Kích hoạt khi kết thúc lượt: Thông báo tới module nội tại để cập nhật hoặc gỡ bỏ các hiệu ứng hết hạn.
	/// </summary>
	public void EndTurn()
	{
		PassivesModuleController?.EndTurn();
	}

	#endregion
}
