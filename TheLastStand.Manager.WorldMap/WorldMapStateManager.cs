using TPLib;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Apocalypse;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using TheLastStand.View.MetaShops;
using TheLastStand.View.WorldMap;
using TheLastStand.View.WorldMap.Apocalypse;
using TheLastStand.View.WorldMap.Glyphs;
using TheLastStand.View.WorldMap.ItemRestriction;

namespace TheLastStand.Manager.WorldMap;

public class WorldMapStateManager : TPSingleton<WorldMapStateManager>
{
	public enum WorldMapState
	{
		DEFAULT,
		EXPLORATION,
		FOCUSED,
		GLYPHSELECTION,
		APOCALYPSE_SELECTION,
		APOCALYPSE_MAX_LEVEL_FOCUS
	}

	public WorldMapState CurrentState { get; private set; }

	public static void SetState(WorldMapState state)
	{
		TPSingleton<WorldMapStateManager>.Instance.CurrentState = state;
		WorldMapCityManager.OnStateChange();
		WorldMapCameraView.OnStateChange();
		TPSingleton<GameConfigurationsView>.Instance.OnStateChange();
		TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.OnStateChange();
		if (state == WorldMapState.FOCUSED)
		{
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnWorldMapCitySelected);
		}
	}

	private void Update()
	{
		if (ApplicationManager.CurrentStateName != "WorldMap" || !TPSingleton<CanvasFadeManager>.Instance.FadeIsOver || TPSingleton<OraculumView>.Instance.Displayed || TPSingleton<OraculumView>.Instance.OpeningOrClosing || TPSingleton<GlyphSelectionPanel>.Instance.OpeningOrClosing || TPSingleton<ApocalypseSelectionPanel>.Instance.OpeningOrClosing || ACameraView.IsZooming || GenericConsent.IsWaitingForInput())
		{
			return;
		}
		if (InputManager.GetButtonDown(23))
		{
			if (TryClosingWeaponRestrictionsPanel() || TryClosingApocalypseSelectedModifiersPanel() || TryClosingApocalypseEditCodePopup())
			{
				return;
			}
			switch (CurrentState)
			{
			case WorldMapState.FOCUSED:
				TPSingleton<GameConfigurationsView>.Instance.OnCloseButtonClicked();
				return;
			case WorldMapState.GLYPHSELECTION:
				OraculumHub<GlyphSelectionPanel>.Display(show: false);
				FadeInAmbientSounds();
				return;
			case WorldMapState.APOCALYPSE_SELECTION:
				OraculumHub<ApocalypseSelectionPanel>.Display(show: false);
				FadeInAmbientSounds();
				return;
			case WorldMapState.APOCALYPSE_MAX_LEVEL_FOCUS:
				TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.OnExitJoystickFocus(triggerChangeState: true);
				return;
			}
			for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeOut(TPSingleton<CanvasFadeManager>.Instance.FadeDuration);
			}
			TPSingleton<GameConfigurationsView>.Instance.OnBackButtonClicked();
		}
		else if (InputManager.GetButtonDown(137) && !TryClosingWeaponRestrictionsPanel() && !TryClosingApocalypseSelectedModifiersPanel() && !TryClosingApocalypseEditCodePopup())
		{
			switch (CurrentState)
			{
			case WorldMapState.FOCUSED:
				TPSingleton<GameConfigurationsView>.Instance.OnCloseButtonClicked();
				break;
			case WorldMapState.APOCALYPSE_MAX_LEVEL_FOCUS:
				TPSingleton<WorldMapApocalypseMaxLevelView>.Instance.OnExitJoystickFocus(triggerChangeState: true);
				break;
			}
		}
	}

	private bool TryClosingWeaponRestrictionsPanel()
	{
		if (TPSingleton<WeaponRestrictionsPanel>.Instance.Displayed)
		{
			if (TPSingleton<WeaponRestrictionsPanel>.Instance.CanClosePanel)
			{
				TPSingleton<WeaponRestrictionsPanel>.Instance.Close();
			}
			return true;
		}
		return false;
	}

	private bool TryClosingApocalypseEditCodePopup()
	{
		if (TPSingleton<ApocalypseSelectionPanel>.Instance.Displayed && TPSingleton<ApocalypseSelectionPanel>.Instance.CodeSharingView.ApocalypseEditCodePopup.Displayed)
		{
			TPSingleton<ApocalypseSelectionPanel>.Instance.CodeSharingView.ApocalypseEditCodePopup.Close();
			return true;
		}
		return false;
	}

	private bool TryClosingApocalypseSelectedModifiersPanel()
	{
		if (TPSingleton<ApocalypseMoreInfoPanel>.Instance.Displayed)
		{
			if (TPSingleton<ApocalypseMoreInfoPanel>.Instance.CanClosePanel)
			{
				TPSingleton<ApocalypseMoreInfoPanel>.Instance.Close();
			}
			return true;
		}
		if (TPSingleton<ApocalypseSelectionPanel>.Instance.Displayed && TPSingleton<ApocalypseSelectionPanel>.Instance.IsFiltersDropDownOpen)
		{
			if (!TPSingleton<ApocalypseSelectionPanel>.Instance.IsFiltersDropDownSelected)
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.FiltersDropDown.Hide();
			}
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<ApocalypseSelectionPanel>.Instance.SelectBottomPanelAfterAFrame();
			}
			return true;
		}
		return false;
	}

	private void FadeInAmbientSounds()
	{
		for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
		{
			TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeIn();
		}
	}
}
