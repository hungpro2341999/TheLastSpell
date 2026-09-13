using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.View.Cursor;
using TheLastStand.View.TileMap;

namespace TheLastStand.Controller;

/// <summary>
/// Bộ điều khiển con trỏ chuột trên bản đồ lưới TileMap:
/// Xử lý cập nhật vị trí ô Tile được trỏ tới, làm mới chỉ báo định hướng (Orientation)
/// và hiển thị vùng ảnh hưởng xoay/lật (Rotation/Flip) của kỹ năng đang chọn.
/// </summary>
public class CursorController
{
	#region Properties & Constructor

	/// <summary>
	/// Model con trỏ lưu trữ vị trí ô hiện tại và ô trước đó.
	/// </summary>
	public Cursor Cursor { get; }

	/// <summary>
	/// Khởi tạo CursorController và model Cursor tương ứng.
	/// </summary>
	public CursorController()
	{
		Cursor = new Cursor(this);
	}

	#endregion

	#region Game State Handlers

	/// <summary>
	/// Xử lý khi trạng thái trò chơi (Game State) thay đổi.
	/// Tự động xóa các chỉ báo ô khi mở các giao diện toàn màn hình (Bảng nhân vật, Chiêu mộ, Nâng cấp nhà...).
	/// </summary>
	/// <param name="state">Trạng thái mới của Game.</param>
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
			// Xóa các ô chỉ báo con trỏ trên TileMap khi ở các màn hình menu/UI
			CursorView.ClearTiles();
			break;
		}
	}

	#endregion

	#region Tile Selection & Indicator Orientation

	/// <summary>
	/// Cập nhật vị trí ô Tile mà con trỏ chuột đang trỏ tới.
	/// Đồng thời cập nhật hướng nhìn của Hero, hiển thị thước đo xoay kỹ năng (Dials) hoặc icon lật chiêu (Flip).
	/// </summary>
	public void SetTile()
	{
		Cursor.PreviousTile = Cursor.Tile;
		Cursor.PreviousTilePosition = Cursor.TilePosition;
		Cursor.TilePosition = CursorView.GetPositionInTileMap();
		
		// Nếu con trỏ chuột không di chuyển sang ô mới, bỏ qua
		if (!(Cursor.TilePosition != Cursor.PreviousTilePosition) && Cursor.PreviousTile != null)
		{
			return;
		}
		
		// Lấy ô Tile mới từ tọa độ chuột
		Cursor.Tile = TPSingleton<TileMapManager>.Instance.TileMap.GetTile(Cursor.TilePosition.x, Cursor.TilePosition.y);
		CursorView.ClearTiles(Cursor.PreviousTile);
		
		// Cập nhật hướng con trỏ dựa trên đối tượng đang chọn
		TileObjectSelectionManager.UpdateCursorOrientationFromSelection(Cursor.Tile);
		
		// Kiểm tra nếu đang không ở trạng thái chuẩn bị thi triển kỹ năng thì dừng lại
		if (!TPSingleton<SkillManager>.Exist() || SkillManager.SelectedSkill == null || (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitPreparingSkill && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.BuildingPreparingSkill))
		{
			return;
		}
		
		// Nếu hướng con trỏ thay đổi hoặc lần đầu trỏ vào ô
		if (TileObjectSelectionManager.CursorOrientationChanged || Cursor.PreviousTile == null)
		{
			// Cập nhật màu hiển thị thước đo xoay kỹ năng nếu chiêu cho phép xoay hướng
			if (SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate)
			{
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDialsTilesColorsFrom(SkillManager.SelectedSkill.SkillAction.SkillActionExecution.SkillSourceTile, SkillManager.SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
				TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.UpdateDisplayRangeTilesColors(SkillManager.SelectedSkill.SkillAction.SkillActionExecution.InRangeTiles.Range);
			}
			// Xoay hướng nhìn của Hero theo hướng con trỏ chuột
			if (TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit?.UnitController.LookAtDirection(TileObjectSelectionManager.GetDirectionFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
			}
			TileObjectSelectionManager.CursorOrientationChanged = false;
		}
		
		// Hiển thị feedback xoay hoặc lật kỹ năng trên TileMap
		if ((SkillManager.SelectedSkill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
		{
			TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, Cursor.Tile, "View/Tiles/Feedbacks/Skill/Dials/Tiles_Cadrans_RotationSkill_On");
		}
		else if (Cursor.Tile != null && (SkillManager.SelectedSkill.SkillDefinition.CanFlip || SkillManager.DebugSkillsForceCanFlip))
		{
			TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, Cursor.Tile, TileMapView.GetSkillFlipIconPathFromOrientation(TileObjectSelectionManager.GuaranteedValidCursorOrientationFromSelection));
		}
	}

	#endregion
}
