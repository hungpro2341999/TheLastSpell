using TPLib;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Building.Construction;

[RequireComponent(typeof(Button))]
public class RepairModeButton : ConstructionModeButton
{
	[SerializeField]
	private TheLastStand.Model.Building.Construction.E_RepairMode repairMode;

	public TheLastStand.Model.Building.Construction.E_RepairMode RepairMode => repairMode;

	public void Init(UnityAction onClick)
	{
		button.onClick.AddListener(onClick);
		TPSingleton<ConstructionManager>.Instance.Construction.RepairModeChanged += OnRepairModeChanged;
	}

	private void OnRepairModeChanged(TheLastStand.Model.Building.Construction.E_RepairMode previousMode, TheLastStand.Model.Building.Construction.E_RepairMode newMode)
	{
		selector.SetActive(RepairMode == newMode);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<ConstructionManager>.Exist())
		{
			TPSingleton<ConstructionManager>.Instance.Construction.RepairModeChanged -= OnRepairModeChanged;
		}
	}
}
