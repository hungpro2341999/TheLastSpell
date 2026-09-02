using TPLib;
using TheLastStand.Definition.Building;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Building.Construction;

public class ConstructionStateButton : MonoBehaviour
{
	[SerializeField]
	private TheLastStand.Model.Building.Construction.E_State constructionState;

	[SerializeField]
	private bool productionBuildings;

	[SerializeField]
	private Button button;

	private bool PlaceBuildingState => constructionState == TheLastStand.Model.Building.Construction.E_State.ChooseBuilding;

	public void Init(UnityAction callback)
	{
		button.onClick.AddListener(callback);
	}

	public void Refresh()
	{
		TheLastStand.Model.Building.Construction.E_State state = TPSingleton<ConstructionManager>.Instance.Construction.State;
		switch (state)
		{
		case TheLastStand.Model.Building.Construction.E_State.ChooseBuilding:
			if (!ConstructionManager.DebugForceConstructionAllowed)
			{
				if (productionBuildings)
				{
					Toggle(constructionState == state && TPSingleton<ConstructionView>.Instance.CurrentlyDisplayedCategory != BuildingDefinition.E_ConstructionCategory.Defensive);
				}
				else
				{
					Toggle(constructionState == state && TPSingleton<ConstructionView>.Instance.CurrentlyDisplayedCategory != BuildingDefinition.E_ConstructionCategory.Production);
				}
				break;
			}
			goto case TheLastStand.Model.Building.Construction.E_State.Repair;
		case TheLastStand.Model.Building.Construction.E_State.Repair:
		case TheLastStand.Model.Building.Construction.E_State.Destroy:
			Toggle(constructionState == state);
			break;
		default:
			Toggle(state: false);
			break;
		}
	}

	public void Toggle(bool state)
	{
		button.interactable = !state;
	}

	private void OnDestroy()
	{
		button.onClick.RemoveAllListeners();
	}

	private void Reset()
	{
		button = GetComponent<Button>();
	}
}
