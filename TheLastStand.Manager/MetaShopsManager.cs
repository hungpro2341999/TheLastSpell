using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TheLastStand.Controller;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Meta;
using TheLastStand.Serialization.Meta;
using TheLastStand.View;
using TheLastStand.View.Camera;
using TheLastStand.View.MetaShops;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.Manager;

public class MetaShopsManager : Manager<MetaShopsManager>, ISerializable, IDeserializable
{
	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioSource audioSourceSecondary;

	[SerializeField]
	private AudioSource audioSourceTransition;

	[SerializeField]
	private AudioSource audioSourceLoop;

	[SerializeField]
	private AudioSource audioSourceSecondaryLoop;

	[SerializeField]
	private AudioClip backToMainScreenTransitionAudioClip;

	[SerializeField]
	private AudioClip darkHoverLoopAudioClip;

	[SerializeField]
	private AudioClip darkShopTransitionAudioClip;

	[SerializeField]
	private AudioClip darkShopTransitionNoGoddessAudioClip;

	[SerializeField]
	private AudioClip darkUpgradeClickAudioClip;

	[SerializeField]
	private AudioClip darkUpgradeFillAudioClip;

	[SerializeField]
	private AudioClip darkUpgradeCompleteAudioClip;

	[SerializeField]
	private AudioClip lightShopTransitionAudioClip;

	[SerializeField]
	private AudioClip lightShopTransitionNoGoddessAudioClip;

	[SerializeField]
	private AudioClip lightHoverAudioClip;

	[SerializeField]
	private AudioClip lightHoverLoopAudioClip;

	[SerializeField]
	private AudioClip lightUpgradeClickAudioClip;

	private List<string> metaUpgradesAlreadySeen = new List<string>();

	public MetaUpgradeDefinition.E_MetaUpgradeFilter CurrentFilter = MetaUpgradeDefinition.E_MetaUpgradeFilter.Acquired | MetaUpgradeDefinition.E_MetaUpgradeFilter.Locked | MetaUpgradeDefinition.E_MetaUpgradeFilter.NotAcquiredYet;

	private bool oraculumForceAccess;

	public static AudioSource AudioSource => TPSingleton<MetaShopsManager>.Instance.audioSource;

	public static AudioSource AudioSourceLoop => TPSingleton<MetaShopsManager>.Instance.audioSourceLoop;

	public static AudioSource AudioSourceSecondary => TPSingleton<MetaShopsManager>.Instance.audioSourceSecondary;

	public static AudioSource AudioSourceSecondaryLoop => TPSingleton<MetaShopsManager>.Instance.audioSourceSecondaryLoop;

	public static AudioSource AudioSourceTransition => TPSingleton<MetaShopsManager>.Instance.audioSourceTransition;

	public static AudioClip BackToMainScreenTransitionAudioClip => TPSingleton<MetaShopsManager>.Instance.backToMainScreenTransitionAudioClip;

	public static AudioClip DarkHoverLoopAudioClip => TPSingleton<MetaShopsManager>.Instance.darkHoverLoopAudioClip;

	public static AudioClip DarkShopTransitionAudioClip => TPSingleton<MetaShopsManager>.Instance.darkShopTransitionAudioClip;

	public static AudioClip DarkShopTransitionNoGoddessAudioClip => TPSingleton<MetaShopsManager>.Instance.darkShopTransitionNoGoddessAudioClip;

	public static AudioClip DarkUpgradeClickAudioClip => TPSingleton<MetaShopsManager>.Instance.darkUpgradeClickAudioClip;

	public static AudioClip DarkUpgradeCompleteAudioClip => TPSingleton<MetaShopsManager>.Instance.darkUpgradeCompleteAudioClip;

	public static AudioClip DarkUpgradeFillAudioClip => TPSingleton<MetaShopsManager>.Instance.darkUpgradeFillAudioClip;

	public static AudioClip HoverAudioClip => TPSingleton<MetaShopsManager>.Instance.lightHoverAudioClip;

	public static AudioClip LightHoverLoopAudioClip => TPSingleton<MetaShopsManager>.Instance.lightHoverLoopAudioClip;

	public static AudioClip LightShopTransitionAudioClip => TPSingleton<MetaShopsManager>.Instance.lightShopTransitionAudioClip;

	public static AudioClip LightShopTransitionNoGoddessAudioClip => TPSingleton<MetaShopsManager>.Instance.lightShopTransitionNoGoddessAudioClip;

	public static AudioClip LightUpgradeClickAudioClip => TPSingleton<MetaShopsManager>.Instance.lightUpgradeClickAudioClip;

	public static bool OraculumForceAccess => TPSingleton<MetaShopsManager>.Instance.oraculumForceAccess;

	public static bool CanLeaveShops()
	{
		if (TPSingleton<OraculumView>.Instance.Displayed && !TPSingleton<OraculumView>.Instance.TransitionRunning && !TPSingleton<OraculumView>.Instance.OpeningOrClosing && !TPSingleton<OraculumView>.Instance.DialogueRunning && !TPSingleton<OraculumView>.Instance.IsInAnyShop && !MetaNarrationsManager.AnyValidMandatoryNarration)
		{
			return !MetaUpgradesManager.AnyAvailableMandatoryUpgrade;
		}
		return false;
	}

	public static bool CanOpenShops()
	{
		if (OraculumForceAccess)
		{
			return true;
		}
		if (TPSingleton<OraculumView>.Instance.Displayed)
		{
			return false;
		}
		switch (ApplicationManager.Application.State.GetName())
		{
		case "Settings":
			if (!TPSingleton<GameManager>.Exist())
			{
				break;
			}
			goto case "Game";
		case "Game":
		case "LoadGame":
			if (TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Deployment && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
			{
				return !TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap;
			}
			return false;
		case "WorldMap":
			if (!TPSingleton<OraculumView>.Instance.OpeningOrClosing)
			{
				if (TPSingleton<WorldMapStateManager>.Instance.CurrentState != WorldMapStateManager.WorldMapState.DEFAULT && TPSingleton<WorldMapStateManager>.Instance.CurrentState != WorldMapStateManager.WorldMapState.EXPLORATION)
				{
					return TPSingleton<WorldMapStateManager>.Instance.CurrentState == WorldMapStateManager.WorldMapState.FOCUSED;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void AddMetaUpgradeToAlreadySeen(MetaUpgrade metaUpgradeModel)
	{
		if (!metaUpgradesAlreadySeen.Contains(metaUpgradeModel.MetaUpgradeDefinition.Id))
		{
			metaUpgradesAlreadySeen.Add(metaUpgradeModel.MetaUpgradeDefinition.Id);
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedMetaShops serializedMetaShops = container as SerializedMetaShops;
		metaUpgradesAlreadySeen = serializedMetaShops?.MetaUpgradesAlreadySeen ?? new List<string>();
		CurrentFilter = serializedMetaShops?.CurrentFilter ?? (MetaUpgradeDefinition.E_MetaUpgradeFilter.Acquired | MetaUpgradeDefinition.E_MetaUpgradeFilter.Locked | MetaUpgradeDefinition.E_MetaUpgradeFilter.NotAcquiredYet);
	}

	public bool IsANewUpgrade(MetaUpgrade metaUpgrade)
	{
		return !metaUpgradesAlreadySeen.Contains(metaUpgrade.MetaUpgradeDefinition.Id);
	}

	public void LeaveShops()
	{
		TPSingleton<MetaShopsManager>.Instance.StopAllCoroutines();
		switch (ApplicationManager.Application.State.GetName())
		{
		case "Game":
			OraculumHub<OraculumView>.Display(show: false);
			GameController.SetState(Game.E_State.Management);
			TPSingleton<SoundManager>.Instance.ChangeMusic();
			break;
		case "WorldMap":
		{
			TPSingleton<WorldMapCityManager>.Instance.RefreshCitiesUnlock();
			OraculumHub<OraculumView>.Display(show: false);
			TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<SoundManager>.Instance.WorldMapMusic);
			for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeIn();
			}
			WorldMapUIManager.OnReturnedFromOraculum();
			break;
		}
		case "MetaShops":
			ApplicationManager.Application.ApplicationQuitInOraculum = false;
			SaveManager.SaveApp();
			TPSingleton<OraculumView>.Instance.SetActiveLeaveHubButton(isActive: false);
			ApplicationManager.Application.ApplicationController.SetState("LoadWorldMap");
			break;
		default:
			TPSingleton<MetaShopsManager>.Instance.LogWarning("Invalid state " + ApplicationManager.Application.State.GetName() + " to leave shops.");
			break;
		}
	}

	public void OpenShops()
	{
		switch (ApplicationManager.Application.State.GetName())
		{
		case "Game":
			TileObjectSelectionManager.DeselectAll();
			GameController.SetState(Game.E_State.MetaShops);
			OraculumHub<OraculumView>.Display(show: true);
			break;
		case "WorldMap":
		{
			if (!ACameraView.IsZooming)
			{
				WorldMapStateManager.SetState(WorldMapStateManager.WorldMapState.EXPLORATION);
			}
			OraculumHub<OraculumView>.Display(show: true);
			for (int i = 0; i < TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds.Length; i++)
			{
				TPSingleton<WorldMapRefsManager>.Instance.AmbientSounds[i].FadeOut();
			}
			break;
		}
		default:
			TPSingleton<MetaShopsManager>.Instance.LogWarning("Unhandled Application State " + ApplicationManager.Application.State.GetName() + " to open MetaShops hub! Aborting.");
			break;
		}
	}

	public void OpenMetaShop(bool darkShop)
	{
		string text = ApplicationManager.Application.State.GetName();
		if (text != null && text == "Game")
		{
			TileObjectSelectionManager.DeselectAll();
			GameController.SetState(Game.E_State.MetaShops);
			TPSingleton<OraculumView>.Instance.DisplayToShop(darkShop);
		}
	}

	public void PlayAudioClip(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(AudioSource, audioClip);
	}

	public void PlayAudioClipWithoutInterrupting(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(AudioSource, audioClip, 0f, doNotInterrupt: true);
	}

	public void RemoveMetaUpgradeFromAlreadySeen(MetaUpgrade upgrade)
	{
		metaUpgradesAlreadySeen?.Remove(upgrade.MetaUpgradeDefinition.Id);
	}

	public ISerializedData Serialize()
	{
		return new SerializedMetaShops
		{
			MetaUpgradesAlreadySeen = metaUpgradesAlreadySeen,
			CurrentFilter = CurrentFilter
		};
	}

	public void UpdateMetaUpgradeAlreadySeen()
	{
		List<MetaUpgradeLineView> list = new List<MetaUpgradeLineView>();
		list.AddRange(TPSingleton<DarkShopManager>.Instance.Lines.Values);
		list.AddRange(TPSingleton<LightShopManager>.Instance.Lines.Values);
		foreach (MetaUpgradeLineView item in list.Where((MetaUpgradeLineView x) => x.State != MetaUpgradeLineView.E_State.Locked))
		{
			AddMetaUpgradeToAlreadySeen(item.MetaUpgrade);
		}
	}

	[ContextMenu("Populate DarkShop")]
	private void PopulateDarkShop()
	{
		TPSingleton<DarkShopManager>.Instance.RefreshShop();
	}

	[ContextMenu("Populate LightShop")]
	private void PopulateLightShop()
	{
		TPSingleton<LightShopManager>.Instance.RefreshShop();
	}

	private void Update()
	{
		bool flag = ApplicationManager.Application.State.GetName() == "Game";
		bool flag2 = ApplicationManager.Application.State.GetName() == "WorldMap";
		bool flag3 = ApplicationManager.Application.State.GetName() == "MetaShops";
		if (!flag && !flag3 && !flag2)
		{
			return;
		}
		if (InputManager.GetButtonDown(58))
		{
			if (flag)
			{
				switch (TPSingleton<GameManager>.Instance.Game.State)
				{
				case Game.E_State.Management:
					if (CanOpenShops())
					{
						if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution != null)
						{
							TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution.SkillExecutionController.Reset();
							TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution = null;
						}
						OpenShops();
					}
					break;
				case Game.E_State.MetaShops:
					if (CanLeaveShops())
					{
						LeaveShops();
					}
					break;
				}
			}
			else if (flag2)
			{
				if (CanOpenShops())
				{
					OpenShops();
				}
				else if (CanLeaveShops())
				{
					LeaveShops();
				}
			}
		}
		else if (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(23))
		{
			bool flag4 = false;
			if (OraculumView.CanTransitionFromShopToHub())
			{
				OraculumView.TransitionFromShopToHub();
				flag4 = true;
			}
			else if (CanLeaveShops())
			{
				LeaveShops();
				flag4 = true;
			}
			if (flag4 && InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		else
		{
			if (!TPSingleton<OraculumView>.Instance.IsInAnyShop || !TPSingleton<OraculumView>.Instance.CurrentShop.Canvas.enabled)
			{
				return;
			}
			if (InputManager.GetButtonDown(88))
			{
				if (TPSingleton<OraculumView>.Instance.IsInDarkShop)
				{
					TPSingleton<DarkShopManager>.Instance.SelectNextTab();
				}
				else if (TPSingleton<OraculumView>.Instance.IsInLightShop)
				{
					TPSingleton<LightShopManager>.Instance.SelectNextTab();
				}
				else
				{
					LogError("Trying to select a meta shop previous tab, but nor dark or light shop are open though OraculumView.IsInAnyShop is true!");
				}
			}
			else if (InputManager.GetButtonDown(89) && !TPSingleton<OraculumView>.Instance.DialogueRunning && !TPSingleton<OraculumView>.Instance.TransitionRunning && TPSingleton<OraculumView>.Instance.IsInAnyShop)
			{
				if (TPSingleton<OraculumView>.Instance.IsInDarkShop)
				{
					TPSingleton<DarkShopManager>.Instance.SelectPreviousTab();
				}
				else if (TPSingleton<OraculumView>.Instance.IsInLightShop)
				{
					TPSingleton<LightShopManager>.Instance.SelectPreviousTab();
				}
				else
				{
					LogError("Trying to select a meta shop previous tab, but nor dark or light shop are open though OraculumView.IsInAnyShop is true!");
				}
			}
		}
	}

	[DevConsoleCommand("OraculumForceAccess")]
	public static void DebugForceOraculumAccess(bool forceAccess = true)
	{
		TPSingleton<MetaShopsManager>.Instance.oraculumForceAccess = forceAccess;
		GameView.BottomScreenPanel.BottomLeftPanel.Refresh();
	}

	[DevConsoleCommand("SaveMetaUpgradesAlreadySeen")]
	public static void DebugSaveMetaUpgradesAlreadySeen()
	{
		TPSingleton<MetaShopsManager>.Instance.UpdateMetaUpgradeAlreadySeen();
		SaveManager.SaveApp();
	}
}
