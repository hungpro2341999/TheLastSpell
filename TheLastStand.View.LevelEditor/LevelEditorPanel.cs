using System.Linq;
using TMPro;
using TPLib;
using TheLastStand.Database.Fog;
using TheLastStand.Database.WorldMap;
using TheLastStand.Definition.Fog;
using TheLastStand.Manager;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.Serialization.LevelEditor;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class LevelEditorPanel : MonoBehaviour
{
	[SerializeField]
	private GameObject bottomPanelHub;

	[SerializeField]
	private GameObject viewOptionToggles;

	[SerializeField]
	private LevelEditorButton undoButton;

	[SerializeField]
	private LevelEditorButton redoButton;

	[SerializeField]
	private LevelEditorButton saveButton;

	[SerializeField]
	private LevelEditorButton increaseLevelSizeButton;

	[SerializeField]
	private LevelEditorButton decreaseLevelSizeButton;

	[SerializeField]
	private LevelEditorButton placeGroundButton;

	[SerializeField]
	private LevelEditorButton setGroundColorButton;

	[SerializeField]
	private LevelEditorButton setTileFlagButton;

	[SerializeField]
	private LevelEditorButton placeBuildingButton;

	[SerializeField]
	private LevelEditorButton destroyBuildingButton;

	[SerializeField]
	private Toggle allTogglesToggle;

	[SerializeField]
	private Toggle levelArtToggle;

	[SerializeField]
	private Toggle tilesFlagToggle;

	[SerializeField]
	private Toggle fogMinMaxToggle;

	[SerializeField]
	private Toggle groundCountsToggle;

	[SerializeField]
	private Slider groundTypeAlphaSlider;

	[SerializeField]
	private InputField inputFieldFogMin;

	[SerializeField]
	private InputField inputFieldFogMax;

	[SerializeField]
	private TextMeshProUGUI mapSizeText;

	[SerializeField]
	private GameObject groundCountsPanel;

	[SerializeField]
	private GroundCounterView[] groundCounterViews;

	public GameObject BottomPanelHub => bottomPanelHub;

	public void OnDestroyBuildingButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.DestroyBuilding);
	}

	public void OnLevelSizeDecreaseButtonClick()
	{
		LevelEditorManager.DecreaseLevelSize();
	}

	public void OnLevelSizeIncreaseButtonClick()
	{
		LevelEditorManager.IncreaseLevelSize();
	}

	public void OnPlaceBuildingButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.SelectBuilding);
	}

	public void OnPlaceGroundButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.SelectGroundType);
	}

	public void OnSetGroundColorButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.SelectGroundColor);
	}

	public void OnSetTileFlagButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.SetTileFlag);
	}

	public void OnRedoButtonClick()
	{
		LevelEditorManager.Redo();
	}

	public void OnSaveButtonClick()
	{
		LevelEditorSaverLoader.Save();
	}

	private void OnAllTogglesToggleValueChanged(bool value)
	{
		viewOptionToggles.SetActive(value);
		if (!value)
		{
			groundCountsToggle.isOn = false;
			OnGroundCountsToggleValueChanged(value: false);
		}
	}

	public void OnLevelArtToggleValueChanged(bool value)
	{
		TileMapView.ToggleLevelArt();
	}

	public void OnTilesFlagToggleValueChanged(bool value)
	{
		TileMapView.ToggleTilesFlagAll();
	}

	public void OnFogMinMaxToggleValueChanged(bool value)
	{
		int result;
		int max = (int.TryParse(inputFieldFogMax.text, out result) ? result : TPSingleton<TileMapManager>.Instance.TileMap.Height);
		int result2;
		int min = (int.TryParse(inputFieldFogMin.text, out result2) ? result2 : 0);
		LevelEditorManager.RefreshFogPreview(value, min, max);
	}

	public void OnGroundCountsToggleValueChanged(bool value)
	{
		groundCountsPanel.SetActive(value);
	}

	public void OnGroundTypeAlphaSliderValueChanged(float value)
	{
		TPSingleton<TileMapView>.Instance.SetGroundTilemapsAlpha(value);
	}

	public void OnUndoButtonClick()
	{
		LevelEditorManager.Undo();
	}

	public void Refresh()
	{
		undoButton.InitText("UNDO (U)");
		if (LevelEditorManager.CompensationConversation.UndoStack.Count > 0)
		{
			undoButton.Button.Interactable = true;
			undoButton.AppendText($": {LevelEditorManager.CompensationConversation.UndoStack.Peek()}");
		}
		else
		{
			undoButton.Button.Interactable = false;
		}
		redoButton.InitText("REDO (R)");
		if (LevelEditorManager.CompensationConversation.RedoStack.Count > 0)
		{
			redoButton.Button.Interactable = true;
			redoButton.AppendText($": {LevelEditorManager.CompensationConversation.RedoStack.Peek()}");
		}
		else
		{
			redoButton.Button.Interactable = false;
		}
		mapSizeText.text = $"Map Size: {TPSingleton<TileMapManager>.Instance.TileMap.Width}x{TPSingleton<TileMapManager>.Instance.TileMap.Height}";
		GroundCounterView[] array = groundCounterViews;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Refresh();
		}
	}

	public void ResetFogMinMaxValues()
	{
		if (CityDatabase.CityDefinitions.TryGetValue(LevelEditorManager.CityToLoadId, out var value))
		{
			FogDefinition fogDefinition = FogDatabase.FogsDefinitions[value.FogDefinitionId];
			inputFieldFogMin.text = fogDefinition.FogDensities.Last().Value.ToString();
			inputFieldFogMax.text = fogDefinition.FogDensities.First().Value.ToString();
		}
		else
		{
			inputFieldFogMin.text = "0";
			inputFieldFogMax.text = "0";
		}
		OnFogMinMaxToggleValueChanged(value: true);
	}

	public void ToggleLevelArt()
	{
		levelArtToggle.isOn = !levelArtToggle.isOn;
	}

	private void Awake()
	{
		undoButton.InitOnClick(OnUndoButtonClick);
		redoButton.InitOnClick(OnRedoButtonClick);
		saveButton.InitOnClick(OnSaveButtonClick);
		placeBuildingButton.InitOnClick(OnPlaceBuildingButtonClick);
		destroyBuildingButton.InitOnClick(OnDestroyBuildingButtonClick);
		increaseLevelSizeButton.InitOnClick(OnLevelSizeIncreaseButtonClick);
		decreaseLevelSizeButton.InitOnClick(OnLevelSizeDecreaseButtonClick);
		placeGroundButton.InitOnClick(OnPlaceGroundButtonClick);
		setGroundColorButton.InitOnClick(OnSetGroundColorButtonClick);
		setTileFlagButton.InitOnClick(OnSetTileFlagButtonClick);
		allTogglesToggle.onValueChanged.AddListener(OnAllTogglesToggleValueChanged);
		levelArtToggle.onValueChanged.AddListener(OnLevelArtToggleValueChanged);
		tilesFlagToggle.onValueChanged.AddListener(OnTilesFlagToggleValueChanged);
		fogMinMaxToggle.onValueChanged.AddListener(OnFogMinMaxToggleValueChanged);
		groundCountsToggle.onValueChanged.AddListener(OnGroundCountsToggleValueChanged);
		groundTypeAlphaSlider.onValueChanged.AddListener(OnGroundTypeAlphaSliderValueChanged);
		ResetFogMinMaxValues();
		groundTypeAlphaSlider.value = TileMapView.GroundCityTilemap.color.a;
		inputFieldFogMin.onValueChanged.AddListener(delegate
		{
			OnFogMinMaxToggleValueChanged(value: true);
		});
		inputFieldFogMax.onValueChanged.AddListener(delegate
		{
			OnFogMinMaxToggleValueChanged(value: true);
		});
	}
}
