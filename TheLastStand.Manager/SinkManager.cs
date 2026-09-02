using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Meta;
using TheLastStand.Database;
using TheLastStand.Definition.Meta;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Sink;
using TheLastStand.Serialization.Sink;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Unit;

namespace TheLastStand.Manager;

public class SinkManager : Manager<SinkManager>
{
	private bool isUnlocked;

	public SinkData AttributeSinkData { get; private set; }

	public SinkData ItemRewardSinkData { get; private set; }

	public PerkSinkData PerkSinkData { get; private set; }

	public bool IsSinkUnlocked
	{
		get
		{
			if (!isUnlocked)
			{
				return DebugForceSinkUnlocked;
			}
			return true;
		}
	}

	public static bool DebugSinkFree { get; private set; }

	private static bool DebugForceSinkUnlocked { get; set; }

	public void Init()
	{
		AttributeSinkData = new SinkData(SinkDatabase.AttributeSinkDataDefinition);
		ItemRewardSinkData = new SinkData(SinkDatabase.ItemRewardSinkDataDefinition);
		PerkSinkData = new PerkSinkData(SinkDatabase.PerkSinkDataDefinition);
		UpdateSinkUnlock();
	}

	public void StartTurn()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production)
		{
			ItemRewardSinkData.Rerolls = 0;
		}
	}

	private void UpdateSinkUnlock()
	{
		isUnlocked = MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockSinkMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated) && effects.Length != 0;
		TPSingleton<SinkManager>.Instance.Log("Update sink: It is " + (isUnlocked ? "unlocked" : "locked") + ".", CLogLevel.MAJOR);
	}

	private void OnMetaUpgradeActivated(MetaUpgrade metaUpgrade)
	{
		if (!isUnlocked)
		{
			TPSingleton<SinkManager>.Instance.UpdateSinkUnlock();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		MetaUpgradesManager.MetaUpgradeActivated += OnMetaUpgradeActivated;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		MetaUpgradesManager.MetaUpgradeActivated -= OnMetaUpgradeActivated;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		if (container is SerializedSinks serializedSinks)
		{
			ItemRewardSinkData.Deserialize(serializedSinks.ItemRewardSerializedSinkData);
		}
	}

	public SerializedSinks Serialize()
	{
		return new SerializedSinks
		{
			ItemRewardSerializedSinkData = (ItemRewardSinkData.Serialize() as SerializedSinkData)
		};
	}

	[DevConsoleCommand("SinkForceUnlock")]
	public static void DebugSinkForceUnlock(bool forceUnlock = true)
	{
		DebugForceSinkUnlocked = forceUnlock;
		RefreshRerollUI();
	}

	[DevConsoleCommand("SinkForceFree")]
	public static void DebugSinkForceFree(bool forceFree = true)
	{
		DebugSinkFree = forceFree;
		RefreshRerollUI();
	}

	[DevConsoleCommand("SinkResetAllRerolls")]
	public static void DebugSinkResetAllRerolls()
	{
		TPSingleton<SinkManager>.Instance.ItemRewardSinkData.Rerolls = 0;
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].LevelUp.UnitLevelUpController.ResetSinkNbReroll();
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PerkTree.UnitPerkTreeController.ResetSinkNbReroll();
		}
		RefreshRerollUI();
	}

	private static void RefreshRerollUI()
	{
		if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
		{
			TPSingleton<UnitLevelUpView>.Instance.RefreshRerollButton();
		}
		if (TPSingleton<CharacterSheetPanel>.Instance.IsPerksPanelOpened)
		{
			TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView.RefreshTopPanel();
		}
		if (TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
		{
			TPSingleton<ChooseRewardPanel>.Instance.RefreshRerollButton();
		}
	}
}
