using System.Collections.Generic;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller;

/// <summary>
/// Interface quy định khả năng thi triển kỹ năng (Skill Casting) của đơn vị hoặc công trình (Hero, Enemy, Turret...).
/// </summary>
public interface ISkillCasterController
{
	#region Properties

	/// <summary>
	/// Tham chiếu tới Model của thực thể thi triển kỹ năng.
	/// </summary>
	ISkillCaster SkillCaster { get; }

	#endregion

	#region Skill Targeting & Cost

	/// <summary>
	/// Lọc danh sách các ô Tile nằm trong tầm thi triển của kỹ năng dựa trên vị trí nguồn xuất phát chiêu.
	/// </summary>
	/// <param name="tilesInRangeInfos">Thông tin các ô trong tầm bắn được tính toán.</param>
	/// <param name="skillSourceTiles">Danh sách các ô Tile nguồn phát động kỹ năng.</param>
	void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles);

	/// <summary>
	/// Khấu trừ chi phí thi triển kỹ năng (Mana, Điểm hành động AP, Lượt dùng tối đa mỗi lượt...).
	/// </summary>
	/// <param name="skill">Kỹ năng vừa được thi triển.</param>
	void PaySkillCost(TheLastStand.Model.Skill.Skill skill);

	#endregion
}
