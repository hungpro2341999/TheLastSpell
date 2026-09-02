using TPLib;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.CharacterSheet;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.Unit.Perk;

public class PerkBookmarkHitbox : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private UnitPerkDisplay unitPerkDisplay;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right && !unitPerkDisplay.Perk.Unlocked)
		{
			unitPerkDisplay.Perk.PerkController.ActiveBookmark(!unitPerkDisplay.Perk.Bookmarked);
		}
	}

	private void Update()
	{
		if (!(ApplicationManager.Application.State is GameState) || TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CharacterSheet || !InputManager.GetButtonDown(138))
		{
			return;
		}
		TheLastStand.Model.Unit.Perk.Perk perk = unitPerkDisplay.Perk;
		if (perk != null && !perk.Unlocked)
		{
			UnitPerkDisplay selectedPerk = TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.UnitPerkTree.UnitPerkTreeView.SelectedPerk;
			if (selectedPerk != null && selectedPerk.Perk == unitPerkDisplay.Perk)
			{
				unitPerkDisplay.Perk.PerkController.ActiveBookmark(!unitPerkDisplay.Perk.Bookmarked);
			}
		}
	}
}
