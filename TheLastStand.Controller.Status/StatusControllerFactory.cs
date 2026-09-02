using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Status.Immunity;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Status;

public static class StatusControllerFactory
{
	public static ISkillCaster GetSkillCaster(StatusSourceInfo statusSourceInfo)
	{
		if (statusSourceInfo == null)
		{
			return null;
		}
		return statusSourceInfo.SourceType switch
		{
			DamageableType.Playable => TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.FirstOrDefault((PlayableUnit x) => x.RandomId == statusSourceInfo.SourceRandomId), 
			DamageableType.Enemy => TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.FirstOrDefault((EnemyUnit x) => x.RandomId == statusSourceInfo.SourceRandomId), 
			DamageableType.Boss => TPSingleton<BossManager>.Instance.BossUnits.FirstOrDefault((BossUnit x) => x.RandomId == statusSourceInfo.SourceRandomId), 
			DamageableType.Building => TPSingleton<BuildingManager>.Instance.Buildings.FirstOrDefault((TheLastStand.Model.Building.Building x) => x.RandomId == statusSourceInfo.SourceRandomId)?.BattleModule, 
			_ => null, 
		};
	}

	public static TheLastStand.Model.Status.Status DeserializeStatus(SerializedUnitStatus serializedStatus, TheLastStand.Model.Unit.Unit unit)
	{
		StatusCreationInfo statusCreationInfo = new StatusCreationInfo
		{
			TurnsCount = serializedStatus.RemainingTurns,
			Stat = serializedStatus.Stat,
			Value = serializedStatus.Value,
			IsFromInjury = serializedStatus.FromInjury,
			IsFromPerk = serializedStatus.FromPerk,
			DelayedSourceInfo = serializedStatus.StatusSourceInfo
		};
		object obj = serializedStatus.Type switch
		{
			TheLastStand.Model.Status.Status.E_StatusType.Poison => new PoisonStatusController(unit, statusCreationInfo).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Stun => new StunStatusController(unit, statusCreationInfo).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Buff => new BuffStatusController(unit, statusCreationInfo).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Debuff => new DebuffStatusController(unit, statusCreationInfo).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Contagion => new ContagionStatusController(unit, statusCreationInfo).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Charged => new ChargedStatusController(unit, statusCreationInfo).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Invulnerable => new InvulnerableStatusController(unit, statusCreationInfo).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.PoisonImmunity => new ImmunityStatusController(unit, statusCreationInfo, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.StunImmunity => new ImmunityStatusController(unit, statusCreationInfo, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.DebuffImmunity => new ImmunityStatusController(unit, statusCreationInfo, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.ContagionImmunity => new ImmunityStatusController(unit, statusCreationInfo, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.AllNegativeImmunity => new ImmunityStatusController(unit, statusCreationInfo, serializedStatus.Type).Status, 
			_ => null, 
		};
		if (obj == null)
		{
			CLoggerManager.Log($"No case corresponding to specified Status type! {serializedStatus.Type}", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "StatusControllerFactory");
		}
		return (TheLastStand.Model.Status.Status)obj;
	}
}
