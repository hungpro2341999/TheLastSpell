using System.Collections.Generic;
using TheLastStand.Controller.CastFx;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.CastFx;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.SkillActionExecution;

namespace TheLastStand.Model.Skill.SkillAction.SkillActionExecution;

public abstract class SkillActionExecution
{
	private ITileObject skillSourceTileObject;

	public HashSet<TheLastStand.Model.Unit.Unit> AllAttackedUnits
	{
		get
		{
			if (AllResultData == null)
			{
				return new HashSet<TheLastStand.Model.Unit.Unit>();
			}
			HashSet<TheLastStand.Model.Unit.Unit> hashSet = new HashSet<TheLastStand.Model.Unit.Unit>();
			foreach (List<SkillActionResultDatas> allResultDatum in AllResultData)
			{
				foreach (SkillActionResultDatas item in allResultDatum)
				{
					foreach (TheLastStand.Model.Unit.Unit affectedUnit in item.AffectedUnits)
					{
						if (affectedUnit != null)
						{
							hashSet.Add(affectedUnit);
						}
					}
				}
			}
			return hashSet;
		}
	}

	public List<List<SkillActionResultDatas>> AllResultData { get; set; } = new List<List<SkillActionResultDatas>>();

	public ISkillCaster Caster { get; set; }

	public TheLastStand.Model.CastFx.CastFx CastFx { get; set; }

	public TheLastStand.Model.CastFx.CastFx PreCastFx { get; set; }

	public bool HasBeenPrepared { get; set; }

	public int HitIndex { get; set; }

	public List<List<Tile>> MultiHitTargetedTiles { get; } = new List<List<Tile>>();

	public List<IDamageable> PreviewAffectedDamageables { get; set; } = new List<IDamageable>();

	public Dictionary<int, List<TheLastStand.Model.Unit.Unit>> PropagationAffectedUnits { get; set; }

	public TilesInRangeInfos InRangeTiles { get; private set; } = new TilesInRangeInfos();

	public List<SkillTargetedTileInfo> TargetTiles { get; private set; } = new List<SkillTargetedTileInfo>();

	public List<Tile> TileInaccurates { get; private set; } = new List<Tile>();

	public Skill Skill { get; set; }

	public SkillActionExecutionController SkillExecutionController { get; private set; }

	public SkillActionExecutionView SkillExecutionView { get; private set; }

	public Tile SkillSourceTile => SkillSourceTileObject?.OriginTile;

	public ITileObject SkillSourceTileObject
	{
		get
		{
			if (skillSourceTileObject == null)
			{
				skillSourceTileObject = Caster;
			}
			return skillSourceTileObject;
		}
		set
		{
			skillSourceTileObject = value;
		}
	}

	public List<Tile> SkillSourceTiles => SkillSourceTileObject.OccupiedTiles;

	public SkillActionExecution(SkillActionExecutionController skillExecutionController, SkillActionExecutionView skillExecutionView, Skill skill)
	{
		SkillExecutionController = skillExecutionController;
		SkillExecutionView = skillExecutionView;
		Skill = skill;
		CastFx = new CastFxController(Skill.SkillDefinition.SkillCastFxDefinition).CastFx;
		CastFx.CastFXInterpreterContext = new SkillCastFXInterpreterContext(CastFx);
		CastFx.SourceTile = SkillSourceTile;
		if (Skill.SkillDefinition.PreSkillCastFxDefinition != null)
		{
			PreCastFx = new CastFxController(Skill.SkillDefinition.PreSkillCastFxDefinition).CastFx;
			PreCastFx.CastFXInterpreterContext = new SkillCastFXInterpreterContext(PreCastFx);
			PreCastFx.SourceTile = SkillSourceTile;
		}
	}

	public List<Tile> GetAllValidTargetTiles()
	{
		List<Tile> list = new List<Tile>();
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in InRangeTiles.Range)
		{
			if (item.Value.HasLineOfSight && Skill.SkillController.IsValidatingTargetingConstraints(item.Key))
			{
				list.Add(item.Key);
			}
		}
		return list;
	}
}
