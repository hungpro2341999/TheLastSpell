using System;
using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TheLastStand.Controller.Status;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Status;

public abstract class Status : ISerializable, IDeserializable
{
	public enum E_StatusTime
	{
		Undefined,
		Permanently,
		StartMyTurn,
		StartPlayerTurn,
		StartEnemyTurn,
		EndMyTurn,
		EndPlayerTurn,
		EndEnemyTurn
	}

	[Flags]
	public enum E_StatusType
	{
		None = 0,
		Buff = 1,
		Debuff = 2,
		Poison = 4,
		Stun = 8,
		Charged = 0x10,
		DebuffImmunity = 0x20,
		PoisonImmunity = 0x40,
		StunImmunity = 0x80,
		Contagion = 0x100,
		ContagionImmunity = 0x200,
		Invulnerable = 0x400,
		AllPositive = 0x6F1,
		RemovablePositive = 0x2F1,
		AllNegativeImmunity = 0x2E0,
		AllNegative = 0x10E,
		All = 0x7FF
	}

	public class StringToStatusForDefaultCommand : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>
		{
			E_StatusType.Poison.ToString(),
			E_StatusType.Stun.ToString(),
			E_StatusType.Charged.ToString(),
			E_StatusType.PoisonImmunity.ToString(),
			E_StatusType.StunImmunity.ToString(),
			E_StatusType.DebuffImmunity.ToString(),
			E_StatusType.ContagionImmunity.ToString(),
			E_StatusType.AllNegativeImmunity.ToString(),
			E_StatusType.Contagion.ToString(),
			E_StatusType.Invulnerable.ToString()
		};
	}

	public class StringToStatusWithStatCommand : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>
		{
			E_StatusType.Debuff.ToString(),
			E_StatusType.Buff.ToString()
		};
	}

	private StatusSourceInfo delayedStatusSourceInfo;

	public virtual string Name => Localizer.Get("StatusName_" + StatusType);

	public bool HideDisplayEffect { get; private set; }

	public bool IsFromInjury { get; private set; }

	public bool IsFromPerk { get; private set; }

	public int RemainingTurnsCount { get; set; }

	public ISkillCaster Source { get; private set; }

	public StatusController StatusController { get; private set; }

	public E_StatusTime StatusDestructionTime { get; set; }

	public E_StatusTime StatusEffectTime { get; protected set; }

	public abstract E_StatusType StatusType { get; }

	public virtual string StatusTypeString => StatusType.ToString();

	public TheLastStand.Model.Unit.Unit Unit { get; set; }

	public Status(StatusController statusController, TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		StatusController = statusController;
		Unit = unit;
		RemainingTurnsCount = statusCreationInfo.TurnsCount;
		Source = statusCreationInfo.Source;
		IsFromInjury = statusCreationInfo.IsFromInjury;
		IsFromPerk = statusCreationInfo.IsFromPerk;
		HideDisplayEffect = statusCreationInfo.HideDisplayEffect;
		if (statusCreationInfo.DelayedSourceInfo != null)
		{
			delayedStatusSourceInfo = statusCreationInfo.DelayedSourceInfo;
			TPSingleton<GameManager>.Instance.FinalizeDeserialize += FinalizeDeserialize;
		}
	}

	public virtual void Deserialize(ISerializedData container, int saveVersion = -1)
	{
	}

	public abstract string GetStylizedStatus();

	public virtual ISerializedData Serialize()
	{
		return new SerializedUnitStatus
		{
			StatusSourceInfo = ((Source is IEntity entity) ? new StatusSourceInfo(entity.RandomId, Source.GetDamageableType()) : null),
			RemainingTurns = RemainingTurnsCount,
			Type = StatusType,
			FromInjury = IsFromInjury,
			FromPerk = IsFromPerk
		};
	}

	private void FinalizeDeserialize()
	{
		TPSingleton<GameManager>.Instance.FinalizeDeserialize -= FinalizeDeserialize;
		if (delayedStatusSourceInfo != null)
		{
			Source = StatusControllerFactory.GetSkillCaster(delayedStatusSourceInfo);
			delayedStatusSourceInfo = null;
		}
	}
}
