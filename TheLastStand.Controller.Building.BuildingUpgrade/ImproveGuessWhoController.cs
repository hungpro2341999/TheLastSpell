using TPLib;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Building.BuildingUpgrade;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

/// <summary>
/// Controller xử lý hiệu ứng nâng cấp "Guess Who" - mở khóa thông tin chi tiết về đợt tấn công của kẻ địch (hiển thị bậc kẻ địch và số lượng kẻ địch).
/// </summary>
public class ImproveGuessWhoController : BuildingUpgradeEffectController
{
	#region Properties

	/// <summary>
	/// Model dữ liệu nâng cấp Guess Who.
	/// </summary>
	public ImproveGuessWho ImproveGuessWho => base.BuildingUpgradeEffect as ImproveGuessWho;

	#endregion

	#region Constructors

	/// <summary>
	/// Khởi tạo controller nâng cấp Guess Who.
	/// </summary>
	public ImproveGuessWhoController(ImproveGuessWhoDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new ImproveGuessWho(definition, this, buildingUpgrade);
	}

	#endregion

	#region Overridden Methods

	/// <summary>
	/// Kích hoạt hiển thị thông tin chi tiết đợt quái (Tier & Số lượng kẻ địch) trong SpawnWaveManager.
	/// </summary>
	public override void TriggerEffect(bool onLoad = false)
	{
		TPSingleton<SpawnWaveManager>.Instance.SetDisplayAllEnemyTiers(state: true);
		TPSingleton<SpawnWaveManager>.Instance.SetDisplayQuantities(state: true);
	}

	#endregion
}

