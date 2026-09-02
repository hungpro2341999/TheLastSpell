using TPLib;
using TheLastStand.Controller;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Unit;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet;

namespace TheLastStand.Manager.Unit;

public class CharacterSheetManager : Manager<CharacterSheetManager>
{
	public static bool CanOpenCharacterSheetPanel()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits)
		{
			return false;
		}
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management)
		{
			return TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Construction;
		}
		return true;
	}

	public static void OpenCharacterSheetPanel(bool fromAnotherPopup = false, int selectedUnit = -1)
	{
		if (selectedUnit != -1)
		{
			TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[selectedUnit]);
		}
		if (TPSingleton<ConstructionManager>.Instance.Construction.State != Construction.E_State.None)
		{
			ConstructionManager.ExitConstructionMode();
		}
		TPSingleton<CharacterSheetPanel>.Instance.Open(fromAnotherPopup);
		GameController.SetState(Game.E_State.CharacterSheet);
	}

	public static void OpenCharacterSheetPanelWith(PlayableUnit playableUnit, bool fromAnotherPopup = false)
	{
		TileObjectSelectionManager.SetSelectedPlayableUnit(playableUnit);
		if (playableUnit == null)
		{
			TPSingleton<CharacterSheetManager>.Instance.LogError("Cannot open character sheet panel with a null playableUnit.");
			return;
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
		{
			TPSingleton<GameOverPanel>.Instance.Canvas.enabled = false;
		}
		TPSingleton<CharacterSheetPanel>.Instance.Open(fromAnotherPopup, playableUnit);
	}

	public static void CloseCharacterSheetPanel(bool toAnotherPopup = false)
	{
		toAnotherPopup |= TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver;
		TPSingleton<CharacterSheetPanel>.Instance.Close(toAnotherPopup);
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
		{
			TPSingleton<GameOverPanel>.Instance.Canvas.enabled = true;
			TPSingleton<GameOverPanel>.Instance.SelectPlayablePanelJoystick();
		}
		if (!toAnotherPopup)
		{
			GameController.SetState(Game.E_State.Management);
		}
	}
}
