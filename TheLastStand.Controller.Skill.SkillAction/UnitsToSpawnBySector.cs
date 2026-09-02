using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Skill.SkillAction;

public class UnitsToSpawnBySector : List<Dictionary<TheLastStand.Model.Skill.Skill, Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>>>
{
	public UnitsToSpawnBySector()
	{
		Clear();
	}

	public new void Clear()
	{
		base.Clear();
		for (int i = 0; i < TPSingleton<SectorManager>.Instance.SectorsCount + 1; i++)
		{
			Add(new Dictionary<TheLastStand.Model.Skill.Skill, Tuple<ISkillCaster, Dictionary<(string, UnitCreationSettings), List<Tile>>>>());
		}
	}
}
