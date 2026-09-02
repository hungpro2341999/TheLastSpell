using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using TPLib;
using TheLastStand.Controller.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Dev.View;

public class MetaUpgradeDebugLineView : MonoBehaviour
{
	[SerializeField]
	private MetaUpgradeDebugConditionLineView metaConditionExample;

	[SerializeField]
	private MetaConditionSeparatorDebugView metaConditionsSeparatorExample;

	[SerializeField]
	private RectTransform selfContainer;

	[SerializeField]
	private float baseHeight = 40f;

	[SerializeField]
	private Image status;

	[SerializeField]
	private Sprite activatedStatusIcon;

	[SerializeField]
	private Sprite unlockedStatusIcon;

	[SerializeField]
	private Sprite lockedStatusIcon;

	[SerializeField]
	private Button selfButton;

	[SerializeField]
	private Button activateButton;

	[SerializeField]
	private Button unlockButton;

	[SerializeField]
	private Button lockButton;

	[SerializeField]
	private RectTransform conditionsRect;

	[SerializeField]
	private Canvas conditionsCanvas;

	[SerializeField]
	private TextMeshProUGUI upgradeName;

	[SerializeField]
	private TextMeshProUGUI upgradePrice;

	private bool isDeployed;

	private List<MetaUpgradeDebugConditionLineView> metaConditions = new List<MetaUpgradeDebugConditionLineView>();

	private List<MetaConditionSeparatorDebugView> separators = new List<MetaConditionSeparatorDebugView>();

	public MetaUpgrade MetaUpgrade { get; private set; }

	public MetaUpgradesManager.E_MetaState MetaState { get; private set; }

	public void Activate()
	{
		MetaUpgradeDebugStateManager.ActivateUpgrade(MetaUpgrade);
		RefreshStatus();
	}

	public void Lock()
	{
		MetaUpgradeDebugStateManager.LockUpgrade(MetaUpgrade);
		RefreshStatus();
	}

	public void Refresh()
	{
		RefreshStatus();
		foreach (MetaUpgradeDebugConditionLineView metaCondition in metaConditions)
		{
			metaCondition.Refresh();
		}
	}

	public void Set(MetaUpgrade upgrade)
	{
		MetaUpgrade = upgrade;
		isDeployed = false;
		conditionsCanvas.enabled = false;
		foreach (MetaUpgradeDebugConditionLineView metaCondition in metaConditions)
		{
			Object.Destroy(metaCondition.gameObject);
		}
		foreach (MetaConditionSeparatorDebugView separator in separators)
		{
			Object.Destroy(separator.gameObject);
		}
		if (upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock).Count > 0)
		{
			AddSeparator("Unlock conditions");
			foreach (List<MetaConditionController> condition in upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock))
			{
				foreach (MetaConditionController item in condition)
				{
					AddConditionLine(item.MetaCondition);
				}
			}
		}
		if (upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation).Count > 0)
		{
			AddSeparator("Activation conditions");
			foreach (List<MetaConditionController> condition2 in upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation))
			{
				foreach (MetaConditionController item2 in condition2)
				{
					AddConditionLine(item2.MetaCondition);
				}
			}
		}
		int num = upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Activation).Count + upgrade.GetConditions(MetaCondition.E_MetaConditionCategory.Unlock).Count;
		upgradeName.text = upgrade.MetaUpgradeDefinition.Id + ((num > 0) ? $"  <i>({num} condition(s))</i>" : "");
		base.name = upgrade.MetaUpgradeDefinition.Id;
		upgradePrice.text = ((upgrade.MetaUpgradeDefinition.Price != 0) ? $"{upgrade.InvestedSouls} / {upgrade.MetaUpgradeDefinition.Price}" : string.Empty);
		Refresh();
	}

	public void ToggleDeploy()
	{
		if (isDeployed)
		{
			isDeployed = false;
			selfContainer.DOSizeDelta(new Vector2(selfContainer.sizeDelta.x, baseHeight), 0.3f).OnComplete(delegate
			{
				conditionsCanvas.enabled = false;
			});
		}
		else
		{
			isDeployed = true;
			conditionsCanvas.enabled = true;
			selfContainer.DOSizeDelta(new Vector2(selfContainer.sizeDelta.x, baseHeight + conditionsRect.sizeDelta.y), 0.3f);
		}
	}

	public void Unlock()
	{
		MetaUpgradeDebugStateManager.UnlockUpgrade(MetaUpgrade);
		RefreshStatus();
	}

	private void AddConditionLine(MetaCondition metaCondition)
	{
		MetaUpgradeDebugConditionLineView component = Object.Instantiate(metaConditionExample.gameObject, metaConditionExample.transform.parent).GetComponent<MetaUpgradeDebugConditionLineView>();
		component.Set(metaCondition);
		component.gameObject.SetActive(value: true);
		metaConditions.Add(component);
	}

	private void AddSeparator(string name)
	{
		MetaConditionSeparatorDebugView component = Object.Instantiate(metaConditionsSeparatorExample.gameObject, metaConditionsSeparatorExample.transform.parent).GetComponent<MetaConditionSeparatorDebugView>();
		component.SetText(name);
		component.gameObject.SetActive(value: true);
		separators.Add(component);
	}

	private void Awake()
	{
		metaConditionsSeparatorExample.gameObject.SetActive(value: false);
		metaConditionExample.gameObject.SetActive(value: false);
		selfButton.onClick.AddListener(ToggleDeploy);
		lockButton.onClick.AddListener(Lock);
		unlockButton.onClick.AddListener(Unlock);
		activateButton.onClick.AddListener(Activate);
	}

	private void RefreshStatus()
	{
		if (TPSingleton<MetaUpgradesManager>.Instance.ActivatedUpgrades.Contains(MetaUpgrade))
		{
			MetaState = MetaUpgradesManager.E_MetaState.Activated;
			status.sprite = activatedStatusIcon;
		}
		else if (TPSingleton<MetaUpgradesManager>.Instance.UnlockedUpgrades.Contains(MetaUpgrade))
		{
			MetaState = MetaUpgradesManager.E_MetaState.Unlocked;
			status.sprite = unlockedStatusIcon;
		}
		else if (TPSingleton<MetaUpgradesManager>.Instance.LockedUpgrades.Contains(MetaUpgrade))
		{
			MetaState = MetaUpgradesManager.E_MetaState.Locked;
			status.sprite = lockedStatusIcon;
		}
		else
		{
			MetaState = MetaUpgradesManager.E_MetaState.NA;
			Debug.LogError("Could not find Upgrade with Id " + MetaUpgrade.MetaUpgradeDefinition.Id + " in any upgrades list.");
		}
	}
}
