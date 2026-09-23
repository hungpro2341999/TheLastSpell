using System.Collections.Generic;
using System.Linq;
using TPLib;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Skill.SkillAction;

/// <summary>
/// Bộ điều khiển cho hành động kỹ năng Xây dựng (Build Skill Action).
/// <para>Chịu trách nhiệm thực thi các kỹ năng tạo ra hoặc dựng công trình/rào chắn trên bản đồ trong quá trình chiến đấu (ví dụ: tạo Barricade, cọc chướng ngại vật...).</para>
/// </summary>
public class BuildSkillActionController : SkillActionController
{
	/// <summary>
	/// Tham chiếu tới Model hành vi xây dựng (BuildSkillAction).
	/// </summary>
	public BuildSkillAction BuildSkillAction => base.SkillAction as BuildSkillAction;

	/// <summary>
	/// Khởi tạo BuildSkillActionController và gắn bộ thực thi BuildSkillActionExecutionController tương ứng.
	/// </summary>
	public BuildSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new BuildSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new BuildSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	/// <summary>
	/// Áp dụng hiệu ứng xây dựng lên các ô bị ảnh hưởng bởi kỹ năng.
	/// </summary>
	/// <param name="caster">Đối tượng thi triển kỹ năng.</param>
	/// <param name="affectedTiles">Danh sách các ô thuộc vùng ảnh hưởng chính.</param>
	/// <param name="surroundingTiles">Danh sách các ô xung quanh.</param>
	public override List<SkillActionResultDatas> ApplyEffect(ISkillCaster caster, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		BuildSkillAction buildSkillAction = base.SkillAction as BuildSkillAction;
		List<SkillActionResultDatas> list = new List<SkillActionResultDatas>();

		// Kỹ năng xây dựng bắt buộc phải thi triển lên các ô trên bản đồ, không áp dụng lên chính bản thân Caster
		if (!BuildSkillAction.BuildSkillActionDefinition.ApplyOnCaster)
		{
			foreach (Tile affectedTile in affectedTiles)
			{
				list.Add(ApplyActionOnTile(affectedTile, caster));
			}
			if (buildSkillAction != null)
			{
				TPSingleton<SkillManager>.Instance.Log($"Amount of buildings spawned : {affectedTiles.Count}.");
			}
			return list;
		}
		TPSingleton<SkillManager>.Instance.LogError("A Build Skill Action should target his caster !");
		return list;
	}

	/// <summary>
	/// Kỹ năng xây dựng không gây tổn hại hay tác động lên công trình hiện có theo cơ chế thông thường.
	/// </summary>
	public override bool IsBuildingAffected(Tile targetTile)
	{
		return false;
	}

	/// <summary>
	/// Kỹ năng xây dựng không tác động lên đơn vị Unit.
	/// </summary>
	public override bool IsUnitAffected(Tile targetTile)
	{
		return false;
	}

	/// <summary>
	/// Thực thi việc tạo công trình lên ô mục tiêu.
	/// </summary>
	/// <param name="targetTile">Ô mục tiêu cần đặt công trình.</param>
	/// <param name="caster">Chủ thể thi triển kỹ năng.</param>
	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas result = ApplyActionOnSurroundingTile(targetTile, caster);
		// Không cho phép xây đè lên ô đang có Tướng đồng minh (PlayableUnit)
		if (targetTile.Unit is PlayableUnit)
		{
			return result;
		}

		// Lấy ngẫu nhiên bản thiết kế công trình dựa theo bảng trọng số trong định nghĩa kỹ năng
		BuildingDefinition buildingDefinition = GetBuildingDefinition();

		// Kiểm tra tính hợp lệ của vị trí đặt công trình
		if (!TileMapController.CanPlaceBuilding(buildingDefinition, targetTile, ignoreUnit: true, ignoreBuilding: true, ignoreFog: true))
		{
			return result;
		}

		// Nếu ô đã có công trình cũ, phá hủy công trình cũ trước khi tạo mới
		if (targetTile.Building != null)
		{
			BuildingManager.DestroyBuilding(targetTile, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: false);
		}

		// Khởi tạo công trình mới trên ô mục tiêu
		BuildingManager.CreateBuilding(buildingDefinition, targetTile);
		return result;
	}

	/// <summary>
	/// Hành động trên ô phụ xung quanh (mặc định trả về dữ liệu rỗng cho kỹ năng xây dựng).
	/// </summary>
	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}

	/// <summary>
	/// Chọn ngẫu nhiên loại công trình sẽ sinh ra dựa trên trọng số (Weight) được cấu hình trong XML.
	/// </summary>
	private BuildingDefinition GetBuildingDefinition()
	{
		int index = 0;
		int num = 0;
		// Tính tổng trọng số của tất cả các công trình có thể xây
		for (int i = 0; i < BuildSkillAction.BuildSkillActionDefinition.Buildings.Count; i++)
		{
			num += BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(i).Value;
		}

		// Quay số ngẫu nhiên trong khoảng tổng trọng số
		int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, num);
		int num2 = 0;
		for (int j = 0; j < BuildSkillAction.BuildSkillActionDefinition.Buildings.Count; j++)
		{
			if (randomRange >= num2 && randomRange < BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(j).Value + num2)
			{
				index = j;
				break;
			}
			num2 += BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(j).Value;
		}
		// Trả về bản thiết kế công trình tương ứng
		return BuildingDatabase.BuildingDefinitions[BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(index).Key];
	}
}
