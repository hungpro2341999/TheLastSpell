using TPLib;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Building.Construction;

[RequireComponent(typeof(Button))]
public class DestroyModeButton : ConstructionModeButton
{
	[SerializeField]
	private TheLastStand.Model.Building.Construction.E_DestroyMode destroyMode;

	public TheLastStand.Model.Building.Construction.E_DestroyMode DestroyMode => destroyMode;

	public void Init(UnityAction onClick)
	{
		button.onClick.AddListener(onClick);
		TPSingleton<ConstructionManager>.Instance.Construction.DestroyModeChanged += OnDestroyModeChanged;
	}

	private void OnDestroyModeChanged(TheLastStand.Model.Building.Construction.E_DestroyMode previousMode, TheLastStand.Model.Building.Construction.E_DestroyMode newMode)
	{
		selector.SetActive(DestroyMode == newMode);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (TPSingleton<ConstructionManager>.Exist())
		{
			TPSingleton<ConstructionManager>.Instance.Construction.DestroyModeChanged -= OnDestroyModeChanged;
		}
	}
}
