using System.Collections.Generic;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Controller;

public interface ISkillCasterController
{
	ISkillCaster SkillCaster { get; }

	void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles);

	void PaySkillCost(TheLastStand.Model.Skill.Skill skill);
}
