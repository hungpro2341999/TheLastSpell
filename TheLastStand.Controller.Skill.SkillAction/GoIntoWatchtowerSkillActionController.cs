using TPLib;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Building;

namespace TheLastStand.Controller.Skill.SkillAction;

/// <summary>
/// Bộ điều khiển cho hành động Trèo vào tháp canh (Go Into Watchtower Skill Action).
/// <para>Chuyển vị trí tướng từ mặt đất lên đỉnh tháp canh, mở khóa tầm bắn xa và góc nhìn bao quát hơn.</para>
/// </summary>
public class GoIntoWatchtowerSkillActionController : SkillActionController
{
	public GoIntoWatchtowerSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new GoIntoWatchtowerSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new GoIntoWatchtowerSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		if (targetTile.Unit != null && targetTile.CanAffectThroughFog(base.SkillAction.SkillActionExecution.Caster))
		{
			return !targetTile.Unit.IsDead;
		}
		return false;
	}

	/// <summary>
	/// Di chuyển tướng vào ô của tháp canh và cập nhật hiển thị đồ họa.
	/// </summary>
	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		PlayableUnit playableUnit = caster as PlayableUnit;
		playableUnit.PlayableUnitController.LookAt(targetTile, caster.OriginTile);
		// Giải phóng ô đứng cũ
		playableUnit.OriginTile.Unit = null;
		if (playableUnit.OriginTile.Building != null && !(playableUnit.OriginTile.Building.BuildingView is WatchtowerView))
		{
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuilding(playableUnit.OriginTile.Building, playableUnit.OriginTile.Building.OriginTile);
		}
		// Gán ô đứng mới là tháp canh
		playableUnit.OriginTile = targetTile;
		targetTile.Unit = playableUnit;
		playableUnit.PlayableUnitView.UpdatePosition();
		return new SkillActionResultDatas();
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}
}
