using System.Collections.Generic;
using DG.Tweening;
using Rewired;
using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Camera;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.View.Cursor;

public class CursorView : TPSingleton<CursorView>
{
	[SerializeField]
	private Tilemap cursorFeedbacksTilemap;

	[SerializeField]
	private Tilemap cursorShapeFeedbacksTilemap;

	[SerializeField]
	private TileBase cursorTile;

	[SerializeField]
	private TileBase buildingShapeTile;

	[SerializeField]
	private JoystickCursorView joystickCursor;

	private float joystickNoMovementTimer;

	private float joystickFastSpeedTimer;

	private Tween joystickCursorSnapTween;

	private bool previousNullJoystickInput;

	public static Tilemap CursorFeedbacksTilemap => TPSingleton<CursorView>.Instance.cursorFeedbacksTilemap;

	public JoystickCursorView JoystickCursorView => joystickCursor;

	public Vector3 JoystickCursorPosition => joystickCursor.transform.position;

	public bool JoystickCursorMoving => joystickNoMovementTimer == 0f;

	public static void ClearTiles()
	{
		ClearTiles(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
	}

	public static void ClearTiles(TheLastStand.Model.TileMap.Tile tile)
	{
		if (tile == null)
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.Game.Cursor.BuildingToFit == null)
		{
			if (tile.Building != null)
			{
				tile.Building.BuildingView.Hovered = false;
				if (TPSingleton<GameManager>.Instance.Game.Cursor.Tile?.Building == null)
				{
					TPSingleton<TileMapView>.Instance.EndHoverOutlineAlphaTilemapTweening();
				}
			}
			if (tile.Unit is EnemyUnit enemyUnit)
			{
				enemyUnit.EnemyUnitView.Hovered = false;
			}
			if (tile.Unit is PlayableUnit playableUnit)
			{
				playableUnit.PlayableUnitView.PortraitPanel.OnPointerExit(null);
			}
			CursorFeedbacksTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), null);
		}
		else
		{
			List<TheLastStand.Model.TileMap.Tile> occupiedTiles = tile.GetOccupiedTiles(TPSingleton<GameManager>.Instance.Game.Cursor.BuildingToFit.BlueprintModuleDefinition);
			for (int i = 0; i < occupiedTiles.Count; i++)
			{
				CursorFeedbacksTilemap.SetTile((Vector3Int)occupiedTiles[i].Position, null);
			}
		}
		TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, tile);
	}

	public static void DisplayBuildingShape(TheLastStand.Model.Building.Building building, bool show)
	{
		for (int i = 0; i < building.BlueprintModule.OccupiedTiles.Count; i++)
		{
			TPSingleton<CursorView>.Instance.cursorShapeFeedbacksTilemap.SetTile((Vector3Int)building.BlueprintModule.OccupiedTiles[i].Position, show ? TPSingleton<CursorView>.Instance.buildingShapeTile : null);
		}
	}

	public static Vector3Int GetPositionInTileMap()
	{
		return CursorFeedbacksTilemap.WorldToCell(TPSingleton<CursorView>.Instance.joystickCursor.transform.position);
	}

	public void SnapJoystickViewToCursorTile()
	{
		if (!(ApplicationManager.Application.State.GetName() != "Game") && TheLastStand.Manager.InputManager.IsLastControllerJoystick && joystickCursorSnapTween == null)
		{
			Vector3 vector = TileMapView.GetTileCenter(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			if (!(Vector3.SqrMagnitude(joystickCursor.transform.position - vector) < 0.01f))
			{
				joystickCursorSnapTween = joystickCursor.transform.DOMove(vector, TheLastStand.Manager.InputManager.JoystickConfig.Cursor.TileSnapDuration).SetEase(TheLastStand.Manager.InputManager.JoystickConfig.Cursor.TileSnapEasing);
			}
		}
	}

	public void SnapJoystickCursorToTile(TheLastStand.Model.TileMap.Tile tile)
	{
		if (!(ApplicationManager.Application.State.GetName() != "Game") && TheLastStand.Manager.InputManager.IsLastControllerJoystick)
		{
			joystickCursorSnapTween?.Kill();
			joystickNoMovementTimer = 0f;
			joystickCursor.SetPosition(TileMapView.GetTileCenter(tile));
		}
	}

	protected override void Awake()
	{
		base.Awake();
		TheLastStand.Manager.InputManager.LastActiveControllerChanged += OnLastActiveControllerChanged;
		if (ApplicationManager.Application.State.GetName() == "Game")
		{
			HUDJoystickNavigationManager.HUDNavigationToggled += OnHUDNavigationToggled;
		}
	}

	private static void ApplyValidationColor(Vector3Int tilePosition)
	{
		switch (TPSingleton<GameManager>.Instance.Game.Cursor.CursorState)
		{
		case TheLastStand.Model.Cursor.E_CursorState.Standard:
			CursorFeedbacksTilemap.SetColor(tilePosition, Color.white);
			break;
		case TheLastStand.Model.Cursor.E_CursorState.Valid:
			CursorFeedbacksTilemap.SetColor(tilePosition, Color.green);
			break;
		case TheLastStand.Model.Cursor.E_CursorState.Invalid:
			CursorFeedbacksTilemap.SetColor(tilePosition, Color.red);
			break;
		case TheLastStand.Model.Cursor.E_CursorState.Undefined:
			break;
		}
	}

	private static Vector3 ClampToScreen(Vector3 position)
	{
		if (!TheLastStand.Manager.InputManager.JoystickConfig.Cursor.ClampToScreen)
		{
			return position;
		}
		Vector3 vector = ACameraView.MainCam.ViewportToWorldPoint(Vector2.zero);
		Vector3 vector2 = ACameraView.MainCam.ViewportToWorldPoint(Vector2.one);
		position.x = Mathf.Clamp(position.x, vector.x, vector2.x);
		position.y = Mathf.Clamp(position.y, vector.y, vector2.y);
		return position;
	}

	private void FocusCameraOnCursorTile()
	{
		if (!(ApplicationManager.Application.State.GetName() != "Game") && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
		{
			ACameraView.MoveTo(TPSingleton<GameManager>.Instance.Game.Cursor.Tile.TileView.transform);
		}
	}

	private void FocusCameraOnJoystickCursor()
	{
		ACameraView.MoveTo(joystickCursor.transform.position);
	}

	private void OnDestroy()
	{
		TheLastStand.Manager.InputManager.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		HUDJoystickNavigationManager.HUDNavigationToggled -= OnHUDNavigationToggled;
	}

	private void OnHUDNavigationToggled(bool state)
	{
		RecenterJoystickCursor();
		ClearTiles();
		CursorFeedbacksTilemap.ClearAllTiles();
		if (!state)
		{
			joystickCursor.Enable(isOn: true);
		}
	}

	private void OnLastActiveControllerChanged(Rewired.ControllerType controllerType)
	{
		switch (controllerType)
		{
		case Rewired.ControllerType.Joystick:
			if (TPSingleton<CursorView>.Instance.joystickCursor != null)
			{
				joystickCursor.Enable(isOn: true);
			}
			break;
		default:
			if (TPSingleton<CursorView>.Instance.joystickCursor != null)
			{
				joystickCursor.Enable(isOn: false);
			}
			break;
		}
	}

	private void Update()
	{
		if (ApplicationManager.Application.State.GetName() == "Game" && (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Recruitment || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Shopping || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingUpgrade || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.NightReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.HowToPlay || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CutscenePlaying))
		{
			return;
		}
		if (ApplicationManager.Application.State.GetName() == "WorldMap" && (TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null || !WorldMapCityManager.CanSelectAnyCity))
		{
			joystickCursor.Show(show: false);
			return;
		}
		UpdateCursor();
		if (!(ApplicationManager.Application.State.GetName() == "Game"))
		{
			return;
		}
		TheLastStand.Model.Cursor cursor = TPSingleton<GameManager>.Instance.Game.Cursor;
		if (cursor.Tile == null)
		{
			return;
		}
		if (TheLastStand.Manager.InputManager.IsPointerOverWorld || (TheLastStand.Manager.InputManager.IsPointerOverAllowingCursorUI && !TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn))
		{
			if (TheLastStand.Manager.InputManager.IsLastControllerJoystick && cursor.TileHasChanged)
			{
				CursorFeedbacksTilemap.ClearAllTiles();
			}
			if (cursor.BuildingToFit != null || !cursor.TileHasChanged)
			{
				return;
			}
			if (cursor.TileHasChanged && cursor.Tile != null)
			{
				if (cursor.Tile.Building != null)
				{
					cursor.Tile.Building.BuildingView.Hovered = true;
					DisplayBuildingShape(cursor.Tile.Building, show: true);
				}
				else if (cursor.Tile.Unit is EnemyUnit enemyUnit)
				{
					enemyUnit.EnemyUnitView.Hovered = true;
				}
			}
			if (cursor.Tile != null && (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingPreparingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.BuildingExecutingSkill || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Wait || (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction && TPSingleton<ConstructionManager>.Instance.Construction.State == Construction.E_State.ChooseBuilding)))
			{
				if (cursor.Tile.Unit is PlayableUnit playableUnit)
				{
					playableUnit.PlayableUnitView.PortraitPanel.OnPointerEnter(null);
				}
				else
				{
					TPSingleton<TileObjectSelectionManager>.Instance.UpdateUnitInfoPanel(cursor.Tile.Unit);
				}
			}
			if (ApplicationManager.Application.State.GetName() == "LevelEditor" && TPSingleton<LevelEditorManager>.Instance.RectFillTileSource != null)
			{
				CursorFeedbacksTilemap.ClearAllTiles();
				TheLastStand.Model.TileMap.Tile[] tilesInRect = TileMapController.GetTilesInRect(TileMapController.GetRectFromTileToTile(TPSingleton<LevelEditorManager>.Instance.RectFillTileSource, cursor.Tile));
				foreach (TheLastStand.Model.TileMap.Tile tile in tilesInRect)
				{
					CursorFeedbacksTilemap.SetTile(new Vector3Int(tile.X, tile.Y, 0), cursorTile);
				}
			}
			else
			{
				if (TheLastStand.Manager.InputManager.IsLastControllerJoystick && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.PlaceUnit)
				{
					CursorFeedbacksTilemap.ClearAllTiles();
				}
				CursorFeedbacksTilemap.SetTile(cursor.TilePosition, cursorTile);
				ApplyValidationColor(cursor.TilePosition);
			}
		}
		else
		{
			ClearTiles(cursor.Tile);
			CursorFeedbacksTilemap.ClearAllTiles();
			cursor.BuildingToFit = null;
			cursor.Tile = null;
		}
	}

	private void RecenterJoystickCursor()
	{
		Vector3 cameraCenterTilePosition = TileMapView.GetCameraCenterTilePosition();
		joystickCursor.SetPosition(cameraCenterTilePosition);
	}

	private void UpdateCursor()
	{
		if (TPSingleton<CursorView>.Instance.joystickCursor == null)
		{
			return;
		}
		switch (TheLastStand.Manager.InputManager.GetLastControllerType())
		{
		case Rewired.ControllerType.Joystick:
		{
			if (TPSingleton<HUDJoystickNavigationManager>.Exist() && TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn)
			{
				joystickCursor.Enable(isOn: false);
				break;
			}
			if (TheLastStand.Manager.InputManager.GetButtonDown(81))
			{
				FocusCameraOnCursorTile();
				SnapJoystickViewToCursorTile();
			}
			if (TheLastStand.Manager.InputManager.JoystickConfig.Cursor.CanHoldCameraFocusDown && TheLastStand.Manager.InputManager.GetButton(81))
			{
				FocusCameraOnJoystickCursor();
			}
			if (!joystickCursor.Enabled)
			{
				Vector3 position = ACameraView.MainCam.ScreenToWorldPoint(TheLastStand.Manager.InputManager.MousePosition);
				position.z = -9f;
				joystickCursor.Enable(isOn: true);
				joystickCursor.SetPosition(ClampToScreen(position));
			}
			Vector3 vector = new Vector3(TheLastStand.Manager.InputManager.GetAxis(20), TheLastStand.Manager.InputManager.GetAxis(21));
			float magnitude = vector.magnitude;
			if (magnitude < TheLastStand.Manager.InputManager.JoystickConfig.DefaultDeadZone)
			{
				vector = Vector3.zero;
			}
			bool flag = vector == Vector3.zero;
			if (flag)
			{
				joystickNoMovementTimer += Time.unscaledDeltaTime;
				joystickCursor.Show(joystickNoMovementTimer < TheLastStand.Manager.InputManager.JoystickConfig.Cursor.HideDelay);
			}
			else
			{
				joystickNoMovementTimer = 0f;
				joystickCursor.Show(show: true);
				if (CameraView.CameraUIMasksHandler != null && ((previousNullJoystickInput && TheLastStand.Manager.InputManager.JoystickConfig.Cursor.UseCameraUIMask) ? CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(joystickCursor.transform.position) : CameraView.CameraUIMasksHandler.IsPointOffscreen(joystickCursor.transform.position)))
				{
					RecenterJoystickCursor();
				}
			}
			if (flag)
			{
				if (joystickNoMovementTimer > TheLastStand.Manager.InputManager.JoystickConfig.Cursor.TileSnapDelay && ApplicationManager.Application.State.GetName() == "Game" && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != null)
				{
					SnapJoystickViewToCursorTile();
				}
			}
			else
			{
				float num;
				if (magnitude < TheLastStand.Manager.InputManager.JoystickConfig.Cursor.FastSpeedStartInclination)
				{
					num = TheLastStand.Manager.InputManager.JoystickConfig.Cursor.SlowSpeed;
					joystickFastSpeedTimer = 0f;
				}
				else
				{
					num = Mathf.Lerp(TheLastStand.Manager.InputManager.JoystickConfig.Cursor.FastSpeedMinMax.x, TheLastStand.Manager.InputManager.JoystickConfig.Cursor.FastSpeedMinMax.y, joystickFastSpeedTimer);
					if (TheLastStand.Manager.InputManager.JoystickConfig.Cursor.FastSpeedTransitionDuration == 0f)
					{
						joystickFastSpeedTimer = 1f;
					}
					else
					{
						joystickFastSpeedTimer += Time.unscaledDeltaTime / TheLastStand.Manager.InputManager.JoystickConfig.Cursor.FastSpeedTransitionDuration;
					}
				}
				if (ACameraView.IsZoomedIn)
				{
					num *= TheLastStand.Manager.InputManager.JoystickConfig.Cursor.FreeSpeedZoomedInMultiplier;
				}
				Vector3 position2 = joystickCursor.transform.position + (TheLastStand.Manager.InputManager.JoystickConfig.Cursor.NormalizeInput ? vector.normalized : vector) * num * Time.unscaledDeltaTime;
				joystickCursor.SetPosition(ClampToScreen(position2));
				joystickCursorSnapTween?.Kill();
				joystickCursorSnapTween = null;
			}
			previousNullJoystickInput = flag;
			break;
		}
		default:
		{
			Vector3 position = ACameraView.MainCam.ScreenToWorldPoint(TheLastStand.Manager.InputManager.MousePosition);
			position.z = 0f;
			joystickCursor.Enable(isOn: false);
			joystickCursor.SetPosition(ClampToScreen(position));
			break;
		}
		}
	}
}
