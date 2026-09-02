using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.TileMap;
using TheLastStand.Manager;
using TheLastStand.Manager.LevelEditor;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class SelectTileFlagPanel : MonoBehaviour
{
	private struct TileFlagButton
	{
		public TileFlagDefinition TileFlagDefinition;

		public LevelEditorButton LevelEditorButton;
	}

	[SerializeField]
	private Transform tileFlagButtonsContainer;

	[SerializeField]
	private Toggle tileFlagToggle;

	[SerializeField]
	private TextMeshProUGUI selectedTileFlagTilesNbText;

	[SerializeField]
	private Toggle usedTileFlagsFilterToggle;

	[SerializeField]
	private LevelEditorButton addFilterButton;

	[SerializeField]
	private LevelEditorButton clearFiltersButton;

	[SerializeField]
	private Transform filtersContainer;

	[SerializeField]
	private TMP_Dropdown filtersDropDown;

	[SerializeField]
	private LevelEditorRemoveButton removeFilterButtonPrefab;

	[SerializeField]
	private TileMapFiltersDefinition allFiltersDefinition;

	private Dictionary<TileFlagDefinition.E_TileFlagTag, LevelEditorButton> buttonsByFlag = new Dictionary<TileFlagDefinition.E_TileFlagTag, LevelEditorButton>();

	private List<TileFlagButton> tileFlagsButtons = new List<TileFlagButton>();

	private bool onlyShowUsedTileFlags;

	private HashSet<TileMapFilterDefinition> activeFilters = new HashSet<TileMapFilterDefinition>();

	private List<TMP_Dropdown.OptionData> filtersOptionsData = new List<TMP_Dropdown.OptionData>();

	private List<LevelEditorRemoveButton> removeFilterButtons = new List<LevelEditorRemoveButton>();

	public void ToggleOffFlag(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		buttonsByFlag[tileFlag].ToggleOff();
	}

	public void RefreshText()
	{
		RefreshTileFlagTilesNbText();
	}

	private void OnAddFilterButtonClick()
	{
		int value = filtersDropDown.value;
		TileMapFilterDefinition item = allFiltersDefinition.FiltersDefinitions[value];
		if (!activeFilters.Contains(item))
		{
			activeFilters.Add(item);
		}
		RefreshRemoveFilterButtons();
		RefreshTileFlagsButtons();
	}

	private void OnBackButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.Default);
	}

	private void OnClearFiltersButtonClick()
	{
		activeFilters.Clear();
		RefreshRemoveFilterButtons();
		RefreshTileFlagsButtons();
	}

	protected void OnRemoveFilterClick(TileMapFilterDefinition filterDefinition)
	{
		activeFilters.Remove(filterDefinition);
		RefreshRemoveFilterButtons();
		RefreshTileFlagsButtons();
	}

	private void OnTileFlagButtonClick(TileFlagDefinition.E_TileFlagTag tileFlag)
	{
		TileMapView.ToggleTilesFlag(tileFlag, true, clearPreviousState: false);
		LevelEditorManager.SelectTileFlag(tileFlag);
		RefreshText();
	}

	private void OnTileFlagToggleValueChanged(TileFlagDefinition.E_TileFlagTag tileFlag, bool state)
	{
		TileMapView.ToggleTilesFlag(tileFlag, state, clearPreviousState: false);
	}

	private void Awake()
	{
		List<TileFlagDefinition> list = new List<TileFlagDefinition>(TileMapManager.TileFlagDefinitions);
		list.Sort((TileFlagDefinition a, TileFlagDefinition b) => a.TileFlagTag.ToString().CompareTo(b.TileFlagTag.ToString()));
		foreach (TileFlagDefinition tileFlagDefinition in list)
		{
			LevelEditorButton levelEditorButton = Object.Instantiate(LevelEditorManager.LevelEditorButtonTogglePrefab, tileFlagButtonsContainer);
			buttonsByFlag.Add(tileFlagDefinition.TileFlagTag, levelEditorButton);
			levelEditorButton.Init(tileFlagDefinition.TileFlagTag.ToString(), delegate
			{
				OnTileFlagButtonClick(tileFlagDefinition.TileFlagTag);
			}, delegate(bool value)
			{
				OnTileFlagToggleValueChanged(tileFlagDefinition.TileFlagTag, value);
			});
			levelEditorButton.Button.colors = new ColorBlock
			{
				normalColor = levelEditorButton.Button.colors.normalColor,
				highlightedColor = tileFlagDefinition.DebugColor,
				pressedColor = levelEditorButton.Button.colors.pressedColor,
				disabledColor = levelEditorButton.Button.colors.disabledColor,
				colorMultiplier = levelEditorButton.Button.colors.colorMultiplier,
				fadeDuration = levelEditorButton.Button.colors.fadeDuration
			};
			tileFlagsButtons.Add(new TileFlagButton
			{
				LevelEditorButton = levelEditorButton,
				TileFlagDefinition = tileFlagDefinition
			});
		}
		Object.Instantiate(LevelEditorManager.LevelEditorButtonPrefab, tileFlagButtonsContainer).Init("BACK (Esc)", OnBackButtonClick);
		usedTileFlagsFilterToggle.onValueChanged.AddListener(OnUsedTileFlagsFilterToggleValueChanged);
		InitFiltersDropDown();
	}

	private void InitFiltersDropDown()
	{
		filtersDropDown.ClearOptions();
		filtersOptionsData.Clear();
		if (allFiltersDefinition != null && allFiltersDefinition.FiltersDefinitions.Length != 0)
		{
			int num = allFiltersDefinition.FiltersDefinitions.Length;
			for (int i = 0; i < num; i++)
			{
				filtersOptionsData.Add(new TMP_Dropdown.OptionData
				{
					text = allFiltersDefinition.FiltersDefinitions[i].Id
				});
			}
			filtersDropDown.AddOptions(filtersOptionsData);
		}
		addFilterButton.InitOnClick(OnAddFilterButtonClick);
		clearFiltersButton.InitOnClick(OnClearFiltersButtonClick);
	}

	private void OnEnable()
	{
		tileFlagToggle.gameObject.SetActive(value: false);
		TileMapView.ToggleTilesFlagAll(false);
		selectedTileFlagTilesNbText.gameObject.SetActive(value: true);
		usedTileFlagsFilterToggle.gameObject.SetActive(value: true);
		if (TPSingleton<LevelEditorManager>.Instance.CurrentTileFlag != TileFlagDefinition.E_TileFlagTag.None)
		{
			TileFlagDefinition.E_TileFlagTag currentTileFlag = TPSingleton<LevelEditorManager>.Instance.CurrentTileFlag;
			if (buttonsByFlag.ContainsKey(currentTileFlag))
			{
				buttonsByFlag[currentTileFlag].Button.onClick.Invoke();
			}
		}
	}

	private void OnDisable()
	{
		tileFlagToggle.gameObject.SetActive(value: true);
		tileFlagToggle.isOn = false;
		TileMapView.ToggleTilesFlagAll(false);
		selectedTileFlagTilesNbText.gameObject.SetActive(value: false);
		usedTileFlagsFilterToggle.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		usedTileFlagsFilterToggle.onValueChanged.RemoveListener(OnUsedTileFlagsFilterToggleValueChanged);
	}

	private void OnUsedTileFlagsFilterToggleValueChanged(bool isToggled)
	{
		onlyShowUsedTileFlags = isToggled;
		RefreshTileFlagsButtons();
	}

	private void RefreshTileFlagsButtons()
	{
		if (tileFlagsButtons.Count <= 0)
		{
			return;
		}
		foreach (TileFlagButton tileFlagsButton in tileFlagsButtons)
		{
			bool flag = true;
			if (onlyShowUsedTileFlags)
			{
				flag = TPSingleton<TileMapManager>.Instance.TileMap.TilesWithFlag.ContainsKey(tileFlagsButton.TileFlagDefinition.TileFlagTag);
			}
			if (flag)
			{
				flag = tileFlagsButton.TileFlagDefinition.HasFilters(activeFilters);
			}
			tileFlagsButton.LevelEditorButton.gameObject.SetActive(flag);
		}
	}

	private void RefreshRemoveFilterButtons()
	{
		int num = activeFilters.Count - removeFilterButtons.Count;
		if (num > 0)
		{
			while (num > 0)
			{
				LevelEditorRemoveButton item = Object.Instantiate(removeFilterButtonPrefab, filtersContainer);
				removeFilterButtons.Add(item);
				num--;
			}
		}
		int num2 = 0;
		foreach (TileMapFilterDefinition filterDefinition in activeFilters)
		{
			LevelEditorRemoveButton levelEditorRemoveButton = removeFilterButtons[num2];
			levelEditorRemoveButton.gameObject.SetActive(value: true);
			levelEditorRemoveButton.Init(filterDefinition.Id, delegate
			{
				OnRemoveFilterClick(filterDefinition);
			});
			num2++;
		}
		if (num2 < removeFilterButtons.Count)
		{
			for (; num2 < removeFilterButtons.Count; num2++)
			{
				removeFilterButtons[num2].gameObject.SetActive(value: false);
			}
		}
	}

	private void RefreshTileFlagTilesNbText()
	{
		if (selectedTileFlagTilesNbText != null && TPSingleton<TileMapManager>.Exist() && TPSingleton<LevelEditorManager>.Exist())
		{
			selectedTileFlagTilesNbText.text = TileMapManager.DebugGetFormattedTileFlagTilesNb(TPSingleton<LevelEditorManager>.Instance.CurrentTileFlag);
		}
	}
}
