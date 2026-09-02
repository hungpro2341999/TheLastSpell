using System;
using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.View.Cutscene;

public class GenericCutsceneView : CutsceneView
{
	public override IEnumerator PlayCutscene(Action callback = null)
	{
		if (base.CutsceneDefinition == null)
		{
			TPSingleton<CutsceneManager>.Instance.LogError("UnitCutsceneDefinition missing, skipping.", this, CLogLevel.NORMAL, forcePrintInUnity: true, printStackTrace: false);
			yield break;
		}
		bool shouldSetState = base.CutsceneDefinition.ShouldSetState;
		bool shouldHideHUD = base.CutsceneDefinition.ShouldHideHUD;
		base.IsPlaying = true;
		Game.E_State previousState = TPSingleton<GameManager>.Instance.Game.State;
		if (shouldSetState)
		{
			GameController.SetState(Game.E_State.CutscenePlaying);
		}
		if (shouldHideHUD)
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.ForEach(delegate(EnemyUnit o)
			{
				o.UnitView.UnitHUD.DisplayHealthIfNeeded();
			});
			TPSingleton<BossManager>.Instance.BossUnits.ForEach(delegate(BossUnit o)
			{
				o.UnitView.UnitHUD.DisplayHealthIfNeeded();
			});
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
			{
				o.UnitView.UnitHUD.DisplayHealthIfNeeded();
			});
			TPSingleton<BuildingManager>.Instance.Buildings.ForEach(delegate(TheLastStand.Model.Building.Building o)
			{
				o.BuildingView.BuildingHUD.DisplayIfNeeded();
			});
		}
		yield return PlayCutsceneDefinition(base.CutsceneDefinition, base.CutsceneData);
		if (shouldSetState)
		{
			GameController.SetState(previousState);
		}
		if (shouldHideHUD)
		{
			TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.ForEach(delegate(EnemyUnit o)
			{
				o.UnitView.UnitHUD.DisplayHealthIfNeeded();
			});
			TPSingleton<BossManager>.Instance.BossUnits.ForEach(delegate(BossUnit o)
			{
				o.UnitView.UnitHUD.DisplayHealthIfNeeded();
			});
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.ForEach(delegate(PlayableUnit o)
			{
				o.UnitView.UnitHUD.DisplayHealthIfNeeded();
			});
			TPSingleton<BuildingManager>.Instance.Buildings.ForEach(delegate(TheLastStand.Model.Building.Building o)
			{
				o.BuildingView.BuildingHUD.DisplayIfNeeded();
			});
		}
		base.IsPlaying = false;
		callback?.Invoke();
		TPSingleton<CutsceneManager>.Instance.UnregisterGenericCutscene(this);
	}
}
