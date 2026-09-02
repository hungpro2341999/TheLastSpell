using System.Collections.Generic;
using System.Linq;
using TMPro;
using TheLastStand.Database.Building;
using TheLastStand.Manager;
using TheLastStand.Manager.LevelEditor;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class SelectBuildingPanel : MonoBehaviour
{
	[SerializeField]
	private Transform content;

	[SerializeField]
	private TMP_InputField buildingSearchBar;

	[SerializeField]
	private Scrollbar scrollbar;

	private List<LevelEditorButton> buildingsButtons = new List<LevelEditorButton>();

	private string buildingIdFilter = string.Empty;

	private void OnBackButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.Default);
	}

	private void OnBuildingButtonClick(string buildingDefinitionId)
	{
		LevelEditorManager.SelectBuilding(buildingDefinitionId);
	}

	private void Awake()
	{
		List<string> list = BuildingDatabase.BuildingDefinitions.Keys.ToList();
		list.Sort();
		foreach (string buildingDefinitionId in list)
		{
			LevelEditorButton levelEditorButton = Object.Instantiate(LevelEditorManager.LevelEditorButtonPrefab, content);
			levelEditorButton.Init(buildingDefinitionId, delegate
			{
				OnBuildingButtonClick(buildingDefinitionId);
			});
			buildingsButtons.Add(levelEditorButton);
		}
		Object.Instantiate(LevelEditorManager.LevelEditorButtonPrefab, content).Init("BACK (Esc)", OnBackButtonClick);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		buildingSearchBar.onValueChanged.AddListener(OnBuildingSearchBarValueChanged);
		buildingSearchBar.onSelect.AddListener(OnBuildingSearchBarSelected);
		buildingSearchBar.onDeselect.AddListener(OnBuildingSearchBarDeselected);
	}

	private void OnBuildingSearchBarValueChanged(string value)
	{
		buildingIdFilter = value;
		RefreshBuildingsButtons();
	}

	private void OnBuildingSearchBarSelected(string value)
	{
		InputManager.SetLevelEditorMapsEnabled(areEnabled: false);
	}

	private void OnBuildingSearchBarDeselected(string value)
	{
		InputManager.SetLevelEditorMapsEnabled(areEnabled: true);
	}

	private void OnDestroy()
	{
		buildingSearchBar.onValueChanged.RemoveListener(OnBuildingSearchBarValueChanged);
		buildingSearchBar.onSelect.RemoveListener(OnBuildingSearchBarSelected);
		buildingSearchBar.onDeselect.RemoveListener(OnBuildingSearchBarDeselected);
	}

	private void OnDisable()
	{
		InputManager.SetLevelEditorMapsEnabled(areEnabled: true);
	}

	private void OnEnable()
	{
		scrollbar.value = 0f;
	}

	private void RefreshBuildingsButtons()
	{
		if (buildingsButtons.Count <= 0)
		{
			return;
		}
		bool flag = string.IsNullOrEmpty(buildingIdFilter);
		foreach (LevelEditorButton buildingsButton in buildingsButtons)
		{
			if (flag)
			{
				buildingsButton.gameObject.SetActive(value: true);
			}
			else
			{
				buildingsButton.gameObject.SetActive(buildingsButton.Button.GetText().ToLower().Contains(buildingIdFilter.ToLower()));
			}
		}
	}
}
