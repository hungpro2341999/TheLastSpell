using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.View.Cursor;
using TheLastStand.View.TileMap;

namespace TheLastStand.Controller;

public class CursorController
{
	public Cursor Cursor { get; }

	public CursorController()
	{
		Cursor = new Cursor(this);
	}

	public static void OnGameStateChange(Game.E_State state)
	{
		switch (state)
		{
		case Game.E_State.CharacterSheet:
		case Game.E_State.Recruitment:
		case Game.E_State.PlaceUnit:
		case Game.E_State.Shopping:
		case Game.E_State.BuildingUpgrade:
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
		case Game.E_State.HowToPlay:
			CursorView.ClearTiles();
			break;
		}
	}

	public void SetTile()
	{
		Cursor.PreviousTile = Cursor.Tile;
		Cursor.PreviousTilePosition = Cursor.TilePosition;
		Cursor.TilePosition = CursorView.GetPositionInTileMap();
		if (!(Cursor.TilePosition != Cursor.PreviousTilePosition) && Cursor.PreviousTile != null)
		{
			return;
		}
		Cursor.Tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(Cursor.TilePosition.x, Cursor.TilePosition.y);
		CursorView.ClearTiles(Cursor.PreviousTile);
		TileObjectSelectionManager.UpdateCursorOrientationFromSelection(Cursor.Tile);
		if (!TPSingleton<SkillManager>.Exist() || SkillManager.SelectedSkill == null || (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill))
		{
			return;
		}
		if (TileObjectSelectionManager.CursorOrientationChanged || Cursor.PreviousTile == null)
		{
			if (SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDialsTilesColorsFrom(SkillManager.SelectedSkill.SkillAction.SkillActionExecution.SkillSourceTile, SkillManager.SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDisplayRangeTilesColors(SkillManager.SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
			}
			if (TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit?.UnitController.LookAtDirection(TileObjectSelectionManager.GetDirectionFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
			}
			TileObjectSelectionManager.CursorOrientationChanged = false;
		}
		if ((SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
		{
			TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, Cursor.Tile, "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_RotationSkill_On");
		}
		else if (Cursor.Tile != null && (SkillManager.SelectedSkill.SkillDefinition.CanFlip || SkillManager.DebugSkillsForceCanFlip))
		{
			TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, Cursor.Tile, TileMapView.GetSkillFlipIconPathFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
		}
	}
}
