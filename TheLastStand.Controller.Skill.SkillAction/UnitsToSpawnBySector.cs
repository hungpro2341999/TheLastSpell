using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Skill.SkillAction;

/// <summary>
/// Cấu trúc dữ liệu lưu danh sách các đơn vị quái vật chờ được triệu hồi phân bổ theo từng khu vực (Sector) của bản đồ.
/// <para>Mỗi phần tử trong danh sách tương ứng với một Sector, chứa một Dictionary ánh xạ từ Kỹ năng triệu hồi tới cặp (Chủ thể cast, Danh sách ô sinh quái theo loại quái).</para>
/// </summary>
public class UnitsToSpawnBySector : List<Dictionary<TheLastStand.Model.Skill.Skill, Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>>>
{
	public UnitsToSpawnBySector()
	{
		Clear();
	}

	/// <summary>
	/// Xóa sạch dữ liệu và khởi tạo lại các danh mục theo số lượng Sector hiện có trên chiến trường.
	/// </summary>
	public new void Clear()
	{
		base.Clear();
		for (int i = 0; i < TPSingleton<SectorManager>.Instance.SectorsCount + 1; i++)
		{
			Add(new Dictionary<TheLastStand.Model.Skill.Skill, Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>>());
		}
	}
}
