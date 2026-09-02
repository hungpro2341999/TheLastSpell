using System.Linq;
using TMPro;
using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Definition;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Model.LevelEditor.Conversation;
using TheLastStand.Model.TileMap;
using TheLastStand.Serialization.LevelEditor;
using TheLastStand.View.Camera;
using TheLastStand.View.Cursor;
using TheLastStand.View.LevelEditor;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.Manager.LevelEditor;

public class LevelEditorManager : Manager<LevelEditorManager>
{
	public delegate void LevelEditorStateChangedEventHandler(E_State previousState, E_State newState);

	public enum E_State
	{
		Undefined,
		Default,
		SelectGroundType,
		SelectGroundColor,
		PlaceGround,
		SetGroundColor,
		SelectBuilding,
		PlaceBuilding,
		DestroyBuilding,
		SetTileFlag
	}

	private static class Constants
	{
		public const string BucketActionTextFormat = "\n({0} + click to use bucket tool)";

		public const string CancelActionText = "\n(Right click to cancel)";

		public const string DestroyBuildingText = "Left click on a building to destroy it";

		public const string EraseTileFlagTextFormat = "({0} + left click to erase)";

		public const string PickBuildingTextFormat = "({0} + left click on a building to pick it)";

		public const string SetGroundColorTextFormat = "Pick a color for <b>{0}</b> ground";

		public const string PickGroundTextFormat = "({0} + left click on a ground to pick it)";

		public const string PlaceBuildingTextFormat = "\n({0} to force placing)";

		public const string RectFillActionTextFormat = "\n({0} + drag to use rect fill)";

		public const string SelectFlagText = "Select a flag to start painting";

		public const string SetTileFlagText = "Set Tile Flag <b>{0}</b>";
	}

	[SerializeField]
	private KeyCode bucketFillKey = KeyCode.LeftShift;

	[SerializeField]
	private KeyCode rectFillKey = KeyCode.LeftAlt;

	[SerializeField]
	private KeyCode pickKey = KeyCode.LeftAlt;

	[SerializeField]
	private KeyCode eraseFlagKey = KeyCode.LeftShift;

	[SerializeField]
	private KeyCode forcePlaceBuildingKey = KeyCode.LeftControl;

	[SerializeField]
	private string saveId = string.Empty;

	[SerializeField]
	private string cityToLoadId = string.Empty;

	[SerializeField]
	[Min(0f)]
	private int cityToLoadLevel;

	[SerializeField]
	private string loadedCityIdPreview = string.Empty;

	[SerializeField]
	private GameObject savingFeedbackCanvas;

	[SerializeField]
	private LevelEditorPanel levelEditorPanel;

	[SerializeField]
	private SelectGroundPanel placeGroundPanel;

	[SerializeField]
	private SelectBuildingPanel placeBuildingPanel;

	[SerializeField]
	private SelectTileFlagPanel tileFlagPanel;

	[SerializeField]
	private TextMeshProUGUI currentActionText;

	[SerializeField]
	private GameObject colorPickerPanel;

	[SerializeField]
	private ColorPicker colorPicker;

	[SerializeField]
	private LevelEditorButton levelEditorButtonPrefab;

	[SerializeField]
	private LevelEditorButton levelEditorButtonTogglePrefab;

	[SerializeField]
	private Tilemap feedbackTilemap;

	[SerializeField]
	private TileBase levelCenterTile;

	private int fogPreviewMax;

	private int fogPreviewMin;

	private E_State state = E_State.Default;

	private readonly CompensationConversation compensationConversation = new CompensationConversation();

	private GroundDefinition currentGround;

	private BuildingDefinition currentBuilding;

	private TheLastStand.Model.TileMap.Tile ghostedTile;

	private TheLastStand.Model.TileMap.Tile lastFlagTile;

	private bool lastFlagEraseState;

	public TheLastStand.Model.TileMap.Tile RectFillTileSource { get; private set; }

	public TileFlagDefinition.E_TileFlagTag CurrentTileFlag { get; private set; }

	public bool IsCurrentTileFlagSelected => CurrentTileFlag != TileFlagDefinition.E_TileFlagTag.None;

	public static CompensationConversation CompensationConversation => TPSingleton<LevelEditorManager>.Instance.compensationConversation;

	public static string CityToLoadId => TPSingleton<LevelEditorManager>.Instance.cityToLoadId;

	public static int CityToLoadLevel => TPSingleton<LevelEditorManager>.Instance.cityToLoadLevel;

	public static bool FogDisplayed { get; private set; }

	public static LevelEditorButton LevelEditorButtonPrefab => TPSingleton<LevelEditorManager>.Instance.levelEditorButtonPrefab;

	public static LevelEditorButton LevelEditorButtonTogglePrefab => TPSingleton<LevelEditorManager>.Instance.levelEditorButtonTogglePrefab;

	public static string SaveId => TPSingleton<LevelEditorManager>.Instance.saveId;

	public event LevelEditorStateChangedEventHandler LevelEditorStateChanged;

	public static void CenterLevelFeedback()
	{
		TPSingleton<LevelEditorManager>.Instance.feedbackTilemap.ClearAllTiles();
		TPSingleton<LevelEditorManager>.Instance.feedbackTilemap.SetTile(new Vector3Int(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2, 0), TPSingleton<LevelEditorManager>.Instance.levelCenterTile);
	}

	public static void ClearLastFlagTile()
	{
		TPSingleton<LevelEditorManager>.Instance.lastFlagTile = null;
	}

	public static void DecreaseLevelSize()
	{
		CompensationConversation.Execute(new DecreaseLevelSizeCommand());
		TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
	}

	public static void IncreaseLevelSize()
	{
		CompensationConversation.Execute(new IncreaseLevelSizeCommand());
		TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
	}

	public static void OnSaveBegan()
	{
		TPSingleton<LevelEditorManager>.Instance.savingFeedbackCanvas.SetActive(value: true);
	}

	public static void OnSaveComplete()
	{
		TPSingleton<LevelEditorManager>.Instance.savingFeedbackCanvas.SetActive(value: false);
	}

	public static void Redo()
	{
		CompensationConversation.Redo();
		TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
	}

	public static void RefreshFogPreview(bool show)
	{
		TPSingleton<LevelEditorManager>.Instance.fogPreviewMax = Mathf.Min(TPSingleton<LevelEditorManager>.Instance.fogPreviewMax, TPSingleton<TileMapManager>.Instance.TileMap.Height);
		TPSingleton<LevelEditorManager>.Instance.fogPreviewMin = Mathf.Min(TPSingleton<LevelEditorManager>.Instance.fogPreviewMin, TPSingleton<LevelEditorManager>.Instance.fogPreviewMax);
		FogDisplayed = show;
		if (FogDisplayed)
		{
			TileMapView.FogMinMaxTilemap.ClearAllTiles();
		}
		TPSingleton<TileMapView>.Instance.DisplayFogMinMax(show, TPSingleton<LevelEditorManager>.Instance.fogPreviewMin, TPSingleton<LevelEditorManager>.Instance.fogPreviewMax, forceRecompute: true);
	}

	public static void RefreshFogPreview(bool show, int min, int max)
	{
		TPSingleton<LevelEditorManager>.Instance.fogPreviewMax = max;
		TPSingleton<LevelEditorManager>.Instance.fogPreviewMin = min;
		RefreshFogPreview(show);
	}

	public static void SelectBuilding(string buildingDefinitionId)
	{
		TPSingleton<LevelEditorManager>.Instance.currentBuilding = BuildingDatabase.BuildingDefinitions[buildingDefinitionId];
		SetState(E_State.PlaceBuilding);
	}

	public static void SelectGround(string groundDefinitionId)
	{
		TPSingleton<LevelEditorManager>.Instance.currentGround = TileDatabase.GroundDefinitions[groundDefinitionId];
		SetState((TPSingleton<LevelEditorManager>.Instance.state == E_State.SelectGroundType) ? E_State.PlaceGround : E_State.SetGroundColor);
	}

	public static void SetGroundColor(Color color)
	{
		CompensationConversation.Execute(new SetGroundColorCommand(TPSingleton<LevelEditorManager>.Instance.currentGround, color));
	}

	public static void SelectTileFlag(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		if (TPSingleton<LevelEditorManager>.Instance.CurrentTileFlag != TileFlagDefinition.E_TileFlagTag.None)
		{
			TPSingleton<LevelEditorManager>.Instance.tileFlagPanel.ToggleOffFlag(TPSingleton<LevelEditorManager>.Instance.CurrentTileFlag);
		}
		TPSingleton<LevelEditorManager>.Instance.CurrentTileFlag = tileFlag;
		ClearLastFlagTile();
		TPSingleton<LevelEditorManager>.Instance.currentActionText.text = $"Set Tile Flag <b>{tileFlag}</b>" + " " + $"({TPSingleton<LevelEditorManager>.Instance.eraseFlagKey} + left click to erase)" + $"\n({TPSingleton<LevelEditorManager>.Instance.rectFillKey} + drag to use rect fill)" + "\n(Right click to cancel)";
		if (TPSingleton<LevelEditorManager>.Instance.state != E_State.SetTileFlag)
		{
			SetState(E_State.SetTileFlag);
		}
	}

	public static void SetState(E_State state)
	{
		TPSingleton<LevelEditorManager>.Instance.currentActionText.text = string.Empty;
		E_State previousState = TPSingleton<LevelEditorManager>.Instance.state;
		OnStateExit(TPSingleton<LevelEditorManager>.Instance.state);
		TPSingleton<LevelEditorManager>.Instance.state = state;
		OnStateEnter(TPSingleton<LevelEditorManager>.Instance.state);
		TPSingleton<LevelEditorManager>.Instance.LevelEditorStateChanged?.Invoke(previousState, TPSingleton<LevelEditorManager>.Instance.state);
	}

	public static void Undo()
	{
		CompensationConversation.Undo();
		TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
	}

	private static void OnStateEnter(E_State state)
	{
		switch (state)
		{
		case E_State.Default:
			TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.BottomPanelHub.SetActive(value: true);
			break;
		case E_State.SelectGroundType:
			TPSingleton<LevelEditorManager>.Instance.placeGroundPanel.gameObject.SetActive(value: true);
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>" + $"({TPSingleton<LevelEditorManager>.Instance.pickKey} + left click on a ground to pick it)" + "</b>";
			break;
		case E_State.SelectGroundColor:
			TPSingleton<LevelEditorManager>.Instance.placeGroundPanel.gameObject.SetActive(value: true);
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>" + $"({TPSingleton<LevelEditorManager>.Instance.pickKey} + left click on a ground to pick it)" + "</b>";
			break;
		case E_State.SelectBuilding:
			TPSingleton<LevelEditorManager>.Instance.placeBuildingPanel.gameObject.SetActive(value: true);
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>" + $"({TPSingleton<LevelEditorManager>.Instance.pickKey} + left click on a building to pick it)" + "</b>";
			break;
		case E_State.SetGroundColor:
			TPSingleton<LevelEditorManager>.Instance.colorPicker.OnColorClicked.AddListener(SetGroundColor);
			TPSingleton<LevelEditorManager>.Instance.colorPickerPanel.SetActive(value: true);
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>" + $"Pick a color for <b>{TPSingleton<LevelEditorManager>.Instance.currentGround.Id}</b> ground" + "</b>";
			break;
		case E_State.SetTileFlag:
			TPSingleton<LevelEditorManager>.Instance.tileFlagPanel.gameObject.SetActive(value: true);
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>Select a flag to start painting</b>";
			TPSingleton<LevelEditorManager>.Instance.tileFlagPanel.RefreshText();
			break;
		case E_State.PlaceGround:
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>" + TPSingleton<LevelEditorManager>.Instance.currentGround.Id + "</b>" + $"\n({TPSingleton<LevelEditorManager>.Instance.bucketFillKey} + click to use bucket tool)" + $"\n({TPSingleton<LevelEditorManager>.Instance.rectFillKey} + drag to use rect fill)" + "\n(Right click to cancel)";
			break;
		case E_State.PlaceBuilding:
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>" + TPSingleton<LevelEditorManager>.Instance.currentBuilding.Id + "</b>" + $"\n({TPSingleton<LevelEditorManager>.Instance.forcePlaceBuildingKey} to force placing)" + "\n(Right click to cancel)";
			break;
		case E_State.DestroyBuilding:
			TPSingleton<LevelEditorManager>.Instance.currentActionText.text = "<b>Left click on a building to destroy it</b>\n(Right click to cancel)";
			break;
		}
	}

	private static void OnStateExit(E_State state)
	{
		switch (state)
		{
		case E_State.Default:
			TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.BottomPanelHub.SetActive(value: false);
			break;
		case E_State.SelectGroundType:
			TPSingleton<LevelEditorManager>.Instance.placeGroundPanel.gameObject.SetActive(value: false);
			break;
		case E_State.SelectGroundColor:
			TPSingleton<LevelEditorManager>.Instance.placeGroundPanel.gameObject.SetActive(value: false);
			break;
		case E_State.SelectBuilding:
			TPSingleton<LevelEditorManager>.Instance.placeBuildingPanel.gameObject.SetActive(value: false);
			break;
		case E_State.SetGroundColor:
			TPSingleton<LevelEditorManager>.Instance.colorPicker.OnColorClicked.RemoveListener(SetGroundColor);
			TPSingleton<LevelEditorManager>.Instance.colorPickerPanel.SetActive(value: false);
			break;
		case E_State.SetTileFlag:
			TPSingleton<LevelEditorManager>.Instance.tileFlagPanel.gameObject.SetActive(value: false);
			break;
		case E_State.PlaceGround:
			TPSingleton<LevelEditorManager>.Instance.currentGround = null;
			break;
		case E_State.PlaceBuilding:
			TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearBuildingGhost(TPSingleton<LevelEditorManager>.Instance.ghostedTile, TPSingleton<LevelEditorManager>.Instance.currentBuilding);
			TPSingleton<LevelEditorManager>.Instance.ghostedTile = null;
			TPSingleton<LevelEditorManager>.Instance.currentBuilding = null;
			break;
		case E_State.DestroyBuilding:
			break;
		}
	}

	private void ClearRectFill()
	{
		RectFillTileSource = null;
		CursorView.CursorFeedbacksTilemap.ClearAllTiles();
	}

	private void OnValidate()
	{
		loadedCityIdPreview = cityToLoadId;
		if (cityToLoadLevel > 0)
		{
			loadedCityIdPreview += cityToLoadLevel;
		}
	}

	private void Update()
	{
		if ((InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137)) && (state == E_State.SelectBuilding || state == E_State.SelectGroundType || state == E_State.SelectGroundColor || state == E_State.SetTileFlag || state == E_State.DestroyBuilding))
		{
			ClearRectFill();
			SetState(E_State.Default);
			return;
		}
		if (InputManager.GetButtonDown(32))
		{
			SetState(E_State.SelectGroundType);
			return;
		}
		if (InputManager.GetButtonDown(33))
		{
			SetState(E_State.SelectBuilding);
			return;
		}
		if (InputManager.GetButtonDown(34))
		{
			SetState(E_State.DestroyBuilding);
			return;
		}
		if (InputManager.GetButtonDown(65))
		{
			SetState(E_State.SetTileFlag);
			return;
		}
		if (InputManager.GetButtonDown(25))
		{
			Undo();
			return;
		}
		if (InputManager.GetButtonDown(26))
		{
			Redo();
			return;
		}
		if (InputManager.GetButtonDown(35))
		{
			LevelEditorSaverLoader.Save();
			return;
		}
		if (InputManager.GetButtonDown(57))
		{
			levelEditorPanel.ToggleLevelArt();
			return;
		}
		TheLastStand.Model.TileMap.Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		switch (state)
		{
		case E_State.SelectGroundType:
		case E_State.SelectGroundColor:
			if (Input.GetKey(pickKey) && tile != null && InputManager.GetButtonDown(24))
			{
				SelectGround(tile.GroundDefinition.Id);
			}
			break;
		case E_State.SelectBuilding:
			if (Input.GetKey(pickKey) && tile?.Building != null && InputManager.GetButtonDown(24))
			{
				SelectBuilding(tile.Building.BuildingDefinition.Id);
			}
			break;
		case E_State.PlaceGround:
			if (Input.GetKeyUp(rectFillKey) && RectFillTileSource != null)
			{
				ClearRectFill();
			}
			if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				ClearRectFill();
				SetState(E_State.SelectGroundType);
			}
			else if (InputManager.GetButton(24) && tile != null)
			{
				if (Input.GetKey(bucketFillKey))
				{
					CompensationConversation.Execute(new PlaceGroundCommand(currentGround, tile, useBucketTool: true));
					TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
				}
				else if (!Input.GetKey(rectFillKey))
				{
					CompensationConversation.Execute(new PlaceGroundCommand(currentGround, tile));
					TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
				}
				else if (RectFillTileSource == null)
				{
					RectFillTileSource = tile;
				}
			}
			else if (InputManager.GetButtonUp(24) && tile != null && RectFillTileSource != null)
			{
				TheLastStand.Model.TileMap.Tile[] tilesInRect2 = TileMapController.GetTilesInRect(TileMapController.GetRectFromTileToTile(RectFillTileSource, tile));
				CompensationConversation.Execute(new PlaceGroundCommand(currentGround, tilesInRect2));
				TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
				ClearRectFill();
			}
			break;
		case E_State.PlaceBuilding:
		{
			bool flag = TileMapController.CanPlaceBuilding(currentBuilding, tile);
			bool key2 = Input.GetKey(KeyCode.LeftControl);
			TPSingleton<TileMapView>.Instance.ChangeBuildingGhostTileMapsColor(flag || key2);
			if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				SetState(E_State.SelectBuilding);
				break;
			}
			if (InputManager.GetButton(24) && tile != null && (key2 || TileMapController.CanPlaceBuilding(currentBuilding, tile)))
			{
				CompensationConversation.Execute(new PlaceBuildingCommand(currentBuilding, tile));
				TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
			}
			if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
			{
				if (ghostedTile != null)
				{
					TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.ClearBuildingGhost(ghostedTile, currentBuilding);
					ghostedTile = null;
				}
				if (tile != null)
				{
					ghostedTile = tile;
					TPSingleton<TileMapManager>.Instance.TileMap.TileMapView.DisplayBuildingGhost(currentBuilding, ghostedTile);
				}
			}
			break;
		}
		case E_State.DestroyBuilding:
			if (InputManager.GetButton(24) && tile?.Building != null)
			{
				CompensationConversation.Execute(new DestroyBuildingCommand(tile));
				TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
			}
			break;
		case E_State.SetGroundColor:
			if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				SetState(E_State.SelectGroundColor);
			}
			break;
		case E_State.SetTileFlag:
		{
			if (Input.GetKeyUp(rectFillKey) && RectFillTileSource != null)
			{
				ClearRectFill();
			}
			bool key = Input.GetKey(eraseFlagKey);
			if (InputManager.GetButton(24) && tile != null && IsCurrentTileFlagSelected)
			{
				if (!Input.GetKey(rectFillKey))
				{
					if (lastFlagTile != tile || key != lastFlagEraseState)
					{
						CompensationConversation.Execute(new SetTileFlagCommand(TileMapManager.TileFlagDefinitions.FirstOrDefault((TileFlagDefinition o) => o.TileFlagTag == CurrentTileFlag), tile, key));
						TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
						TPSingleton<LevelEditorManager>.Instance.tileFlagPanel.RefreshText();
						lastFlagTile = tile;
						lastFlagEraseState = key;
					}
				}
				else if (RectFillTileSource == null)
				{
					RectFillTileSource = tile;
				}
			}
			else if (InputManager.GetButtonUp(24) && tile != null && IsCurrentTileFlagSelected && RectFillTileSource != null)
			{
				TheLastStand.Model.TileMap.Tile[] tilesInRect = TileMapController.GetTilesInRect(TileMapController.GetRectFromTileToTile(RectFillTileSource, tile));
				CompensationConversation.Execute(new SetTileFlagCommand(TileMapManager.TileFlagDefinitions.FirstOrDefault((TileFlagDefinition o) => o.TileFlagTag == CurrentTileFlag), tilesInRect, key));
				TPSingleton<LevelEditorManager>.Instance.levelEditorPanel.Refresh();
				TPSingleton<LevelEditorManager>.Instance.tileFlagPanel.RefreshText();
				lastFlagTile = tile;
				lastFlagEraseState = key;
				ClearRectFill();
			}
			break;
		}
		}
	}

	private void Start()
	{
		levelEditorPanel.Refresh();
		currentActionText.text = string.Empty;
		ACameraView.Zoom(zoomIn: false, instant: true);
		CenterLevelFeedback();
		TileMapView.ToggleTilesFlagAll(false);
	}
}
