using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Dev.View;

public class MetaConditionsDebugView : TPSingleton<MetaConditionsDebugView>
{
	public enum E_MetaStatusFilter
	{
		None = 0,
		Activated = 1,
		Unlocked = 2,
		Locked = 4,
		ActivatedUnlocked = 3,
		ActivatedLocked = 5,
		UnlockedLocked = 6,
		All = 7
	}

	[Serializable]
	private class SerializedMetaState
	{
		public SerializedMetaConditions MetaConditions;

		public SerializedMetaUpgrades MetaUpgrades;
	}

	[SerializeField]
	private GameObject contentGameObject;

	[SerializeField]
	private GraphicRaycaster hitbox;

	[SerializeField]
	private TMP_InputField searchBar;

	[SerializeField]
	private MetaUpgradeDebugLineView metaUpgradeViewExample;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private Button unlockAllButton;

	[SerializeField]
	private Button lockAllButton;

	[SerializeField]
	private Button activateAllButton;

	[SerializeField]
	private Button saveStateButton;

	[SerializeField]
	private Button loadStateButton;

	[SerializeField]
	private Button sortByNameButton;

	[SerializeField]
	private Button sortByStatusButton;

	[SerializeField]
	private Button filterActivatedButton;

	[SerializeField]
	private Button filterUnlockedButton;

	[SerializeField]
	private Button filterLockedButton;

	[SerializeField]
	private Image filterActivatedIcon;

	[SerializeField]
	private Image filterUnlockedIcon;

	[SerializeField]
	private Image filterLockedIcon;

	[SerializeField]
	private Toggle refreshToggle;

	[SerializeField]
	private Button refreshStatesButton;

	[SerializeField]
	private TMP_Dropdown dropDown;

	private List<MetaUpgradeDebugLineView> metaUpgradeViews = new List<MetaUpgradeDebugLineView>();

	private bool nameSortReversed = true;

	private bool statusSortReversed = true;

	private IEnumerable<MetaUpgrade> AllUpgrades => TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Concat(TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades).Concat(TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades);

	public E_MetaStatusFilter Filter { get; private set; }

	public bool IsOpen { get; private set; }

	private string StatesExtension => "BMS";

	private string StatesFolderPath => Path.Combine(Application.persistentDataPath, "MetaStates");

	[DevConsoleCommand("MCDW")]
	public static void ToggleMetaConditionsDebugView()
	{
		TPSingleton<MetaConditionsDebugView>.Instance.IsOpen = !TPSingleton<MetaConditionsDebugView>.Instance.IsOpen;
		TPSingleton<MetaConditionsDebugView>.Instance.Refresh();
		InputManager.DebugOnMetaConditionDebugViewToggled(TPSingleton<MetaConditionsDebugView>.Instance.IsOpen);
		if (!TPSingleton<MetaConditionsDebugView>.Instance.IsOpen)
		{
			SaveManager.SaveApp();
		}
	}

	public void ClearSearchBar()
	{
		searchBar.text = string.Empty;
	}

	protected override void Awake()
	{
		base.Awake();
		closeButton.onClick.AddListener(ToggleMetaConditionsDebugView);
		unlockAllButton.onClick.AddListener(UnlockAllUpgrades);
		lockAllButton.onClick.AddListener(LockAllUpgrades);
		activateAllButton.onClick.AddListener(ActivateAllUpgrades);
		sortByNameButton.onClick.AddListener(SortByName);
		sortByStatusButton.onClick.AddListener(SortByStatus);
		filterActivatedButton.onClick.AddListener(delegate
		{
			OnFilterValueChanged(E_MetaStatusFilter.Activated);
		});
		filterUnlockedButton.onClick.AddListener(delegate
		{
			OnFilterValueChanged(E_MetaStatusFilter.Unlocked);
		});
		filterLockedButton.onClick.AddListener(delegate
		{
			OnFilterValueChanged(E_MetaStatusFilter.Locked);
		});
		refreshToggle.onValueChanged.AddListener(ToggleRefresh);
		loadStateButton.onClick.AddListener(LoadState);
		saveStateButton.onClick.AddListener(SaveState);
		RefreshAvailableStates();
	}

	[ContextMenu("Activate all upgrades")]
	private void ActivateAllUpgrades()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Activate();
		}
	}

	private void ApplySearchFilters()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.gameObject.SetActive(Filter == E_MetaStatusFilter.All || Filter == E_MetaStatusFilter.None || ((Filter & E_MetaStatusFilter.Activated) == E_MetaStatusFilter.Activated && metaUpgradeView.MetaState == MetaUpgradesManager.E_MetaState.Activated) || ((Filter & E_MetaStatusFilter.Unlocked) == E_MetaStatusFilter.Unlocked && metaUpgradeView.MetaState == MetaUpgradesManager.E_MetaState.Unlocked) || ((Filter & E_MetaStatusFilter.Locked) == E_MetaStatusFilter.Locked && metaUpgradeView.MetaState == MetaUpgradesManager.E_MetaState.Locked));
		}
		foreach (MetaUpgradeDebugLineView metaUpgradeView2 in metaUpgradeViews)
		{
			if (metaUpgradeView2.gameObject.activeSelf)
			{
				string text = metaUpgradeView2.MetaUpgrade.MetaUpgradeDefinition.Id.ToLower();
				metaUpgradeView2.gameObject.SetActive(string.IsNullOrEmpty(searchBar.text) || text.Contains(searchBar.text.ToLower()));
			}
		}
	}

	private void ClearUpgrades()
	{
		Transform parent = metaUpgradeViewExample.transform.parent;
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			if (parent.GetChild(num) != metaUpgradeViewExample.transform)
			{
				UnityEngine.Object.Destroy(parent.GetChild(num).gameObject);
			}
		}
	}

	private void CreateUpgrades()
	{
		metaUpgradeViewExample.gameObject.SetActive(value: true);
		metaUpgradeViews.Clear();
		foreach (MetaUpgrade allUpgrade in AllUpgrades)
		{
			MetaUpgradeDebugLineView component = UnityEngine.Object.Instantiate(metaUpgradeViewExample.gameObject, metaUpgradeViewExample.transform.parent).GetComponent<MetaUpgradeDebugLineView>();
			component.Set(allUpgrade);
			metaUpgradeViews.Add(component);
		}
		metaUpgradeViewExample.gameObject.SetActive(value: false);
	}

	private IReadOnlyList<string> ListStates()
	{
		if (!Directory.Exists(StatesFolderPath))
		{
			Directory.CreateDirectory(StatesFolderPath);
		}
		return (from o in Directory.GetFiles(StatesFolderPath)
			select Path.GetFileNameWithoutExtension(o)).ToList();
	}

	private void LoadState()
	{
		LoadState(dropDown.options[dropDown.value].text);
	}

	private void LoadState(string stateName)
	{
		string text = Path.Combine(StatesFolderPath, stateName + "." + StatesExtension);
		if (!File.Exists(text))
		{
			Debug.LogError("??? " + text + " does not exist!");
		}
		SerializedMetaState serializedMetaState;
		using (FileStream serializationStream = File.OpenRead(text))
		{
			serializedMetaState = (SerializedMetaState)new BinaryFormatter().Deserialize(serializationStream);
		}
		MetaConditionManager.DebugClearConditionControllers();
		TPSingleton<MetaUpgradesManager>.Instance.Deserialize(serializedMetaState.MetaUpgrades);
		TPSingleton<MetaConditionManager>.Instance.DeserializeFromAppSave(serializedMetaState.MetaConditions);
		ClearUpgrades();
		CreateUpgrades();
		Refresh();
	}

	[ContextMenu("Lock all upgrades")]
	private void LockAllUpgrades()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Lock();
		}
	}

	private void OnDestroy()
	{
		MetaConditionManager.OnConditionsRefreshed -= Refresh;
		searchBar.onValueChanged.RemoveListener(OnSearchBarValueChanged);
	}

	private void OnFilterValueChanged(E_MetaStatusFilter filter)
	{
		Filter ^= filter;
		Color color = filterActivatedIcon.color;
		color.a = (((Filter & E_MetaStatusFilter.Activated) == E_MetaStatusFilter.Activated) ? 1f : 0.25f);
		filterActivatedIcon.color = color;
		color = filterUnlockedIcon.color;
		color.a = (((Filter & E_MetaStatusFilter.Unlocked) == E_MetaStatusFilter.Unlocked) ? 1f : 0.25f);
		filterUnlockedIcon.color = color;
		color = filterLockedIcon.color;
		color.a = (((Filter & E_MetaStatusFilter.Locked) == E_MetaStatusFilter.Locked) ? 1f : 0.25f);
		filterLockedIcon.color = color;
		ApplySearchFilters();
	}

	private void OnSearchBarValueChanged(string value)
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			string text = metaUpgradeView.MetaUpgrade.MetaUpgradeDefinition.Id.ToLower();
			metaUpgradeView.gameObject.SetActive(string.IsNullOrEmpty(value) || text.Contains(value.ToLower()));
		}
		ApplySearchFilters();
	}

	private void Refresh()
	{
		contentGameObject.SetActive(IsOpen);
		hitbox.enabled = IsOpen;
		if (!IsOpen)
		{
			return;
		}
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Refresh();
		}
		RefreshAvailableStates();
	}

	private void RefreshAvailableStates()
	{
		dropDown.options = (from o in ListStates()
			select new TMP_Dropdown.OptionData(o)).ToList();
	}

	private void SaveState()
	{
		SerializedMetaState graph = new SerializedMetaState
		{
			MetaConditions = (TPSingleton<MetaConditionManager>.Instance.SerializeToAppSave() as SerializedMetaConditions),
			MetaUpgrades = (TPSingleton<MetaUpgradesManager>.Instance.Serialize() as SerializedMetaUpgrades)
		};
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		string text = Path.Combine(StatesFolderPath, Environment.UserName.Substring(0, 3) + "_" + DateTime.Now.ToString("dd-MM-yy HH.mm.ss") + "." + StatesExtension);
		using (FileStream serializationStream = File.OpenWrite(text))
		{
			binaryFormatter.Serialize(serializationStream, graph);
		}
		Debug.Log("Saved Meta state under " + text);
		RefreshAvailableStates();
	}

	private void SortByName()
	{
		statusSortReversed = true;
		nameSortReversed = !nameSortReversed;
		metaUpgradeViews.Sort((MetaUpgradeDebugLineView lineA, MetaUpgradeDebugLineView lineB) => lineA.MetaUpgrade.MetaUpgradeDefinition.Id.CompareTo(lineB.MetaUpgrade.MetaUpgradeDefinition.Id));
		if (nameSortReversed)
		{
			metaUpgradeViews.Reverse();
		}
		for (int num = 0; num < metaUpgradeViews.Count; num++)
		{
			metaUpgradeViews[num].transform.SetSiblingIndex(num);
		}
	}

	private void SortByStatus()
	{
		nameSortReversed = true;
		statusSortReversed = !statusSortReversed;
		metaUpgradeViews.Sort((MetaUpgradeDebugLineView lineA, MetaUpgradeDebugLineView lineB) => lineA.MetaState.CompareTo(lineB.MetaState));
		if (statusSortReversed)
		{
			metaUpgradeViews.Reverse();
		}
		for (int num = 0; num < metaUpgradeViews.Count; num++)
		{
			metaUpgradeViews[num].transform.SetSiblingIndex(num);
		}
	}

	private void Start()
	{
		MetaConditionManager.OnConditionsRefreshed += Refresh;
		searchBar.onValueChanged.AddListener(OnSearchBarValueChanged);
		refreshStatesButton.onClick.AddListener(RefreshAvailableStates);
		filterActivatedIcon.color = new Color(filterActivatedIcon.color.r, filterActivatedIcon.color.g, filterActivatedIcon.color.b, 0.25f);
		filterUnlockedIcon.color = new Color(filterUnlockedIcon.color.r, filterUnlockedIcon.color.g, filterUnlockedIcon.color.b, 0.25f);
		filterLockedIcon.color = new Color(filterLockedIcon.color.r, filterLockedIcon.color.g, filterLockedIcon.color.b, 0.25f);
		CreateUpgrades();
	}

	private void ToggleRefresh(bool isEnabled)
	{
		TPSingleton<MetaConditionManager>.Instance.DisableRefreshes = !isEnabled;
	}

	[ContextMenu("Unlock all upgrades")]
	private void UnlockAllUpgrades()
	{
		foreach (MetaUpgradeDebugLineView metaUpgradeView in metaUpgradeViews)
		{
			metaUpgradeView.Unlock();
		}
	}
}
