using System.Collections.Generic;
using System.Linq;
using TheLastStand.Controller.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Unit;

namespace TheLastStand.Model.Unit;

public class LifetimeStats : ISerializable, IDeserializable
{
	public float BestBlow { get; set; }

	public Dictionary<string, float> DamagesInflictedToEnemies { get; set; } = new Dictionary<string, float>();

	public int CriticalHits { get; set; }

	public int DamagesBlocked { get; set; }

	public float DamagesInflicted { get; set; }

	public float DamagesMitigatedByResistance { get; set; }

	public Dictionary<string, float> DamagesTakenByEnemyType { get; set; } = new Dictionary<string, float>();

	public float DamagesTakenOnArmor { get; set; }

	public float DamageTakenTotal => DamagesTakenOnArmor + HealthLost;

	public int Dodges { get; set; }

	public float HealthLost { get; set; }

	public int JumpsOverWallUsed { get; set; }

	public int Kills { get; set; }

	public int IsolatedKills { get; set; }

	public LifetimeStatsController LifetimeStatsController { get; set; }

	public int ManaSpent { get; set; }

	public int MostUnitsKilledInOneBlow { get; set; }

	public int PunchesUsed { get; set; }

	public int StunnedEnemies { get; set; }

	public int TilesCrossed { get; set; }

	public Dictionary<string, int> UsesPerWeapon { get; set; } = new Dictionary<string, int>();

	public LifetimeStats(SerializedLifetimeStats container, LifetimeStatsController lifetimeStatsController)
	{
		LifetimeStatsController = lifetimeStatsController;
		Deserialize(container);
	}

	public LifetimeStats(LifetimeStatsController lifetimeStatsController)
	{
		LifetimeStatsController = lifetimeStatsController;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedLifetimeStats serializedLifetimeStats = container as SerializedLifetimeStats;
		BestBlow = serializedLifetimeStats.BestBlow;
		CriticalHits = serializedLifetimeStats.CriticalHits;
		DamagesBlocked = serializedLifetimeStats.DamagesBlocked;
		DamagesInflicted = serializedLifetimeStats.DamagesInflicted;
		DamagesMitigatedByResistance = serializedLifetimeStats.DamagesMitigatedByResistance;
		DamagesTakenOnArmor = serializedLifetimeStats.DamagesTakenOnArmor;
		Dodges = serializedLifetimeStats.Dodges;
		HealthLost = serializedLifetimeStats.HealthLost;
		JumpsOverWallUsed = serializedLifetimeStats.JumpsOverWallUsed;
		Kills = serializedLifetimeStats.Kills;
		IsolatedKills = serializedLifetimeStats.IsolatedKills;
		ManaSpent = serializedLifetimeStats.ManaSpent;
		MostUnitsKilledInOneBlow = serializedLifetimeStats.MostUnitsKilledInOneBlow;
		PunchesUsed = serializedLifetimeStats.PunchesUsed;
		TilesCrossed = serializedLifetimeStats.TilesCrossed;
		StunnedEnemies = serializedLifetimeStats.StunnedEnemies;
		DamagesInflictedToEnemies = new Dictionary<string, float>();
		foreach (SerializedLifetimeStats.DamageInflictedToEnemy damagesInflictedToEnemy in serializedLifetimeStats.DamagesInflictedToEnemies)
		{
			DamagesInflictedToEnemies[damagesInflictedToEnemy.EnemyID] = damagesInflictedToEnemy.DamageInflicted;
		}
		DamagesTakenByEnemyType = new Dictionary<string, float>();
		foreach (SerializedLifetimeStats.DamageTakenFromEnemy item in serializedLifetimeStats.DamagesTakenByEnemyType)
		{
			DamagesTakenByEnemyType[item.EnemyID] = item.DamageTaken;
		}
		UsesPerWeapon = new Dictionary<string, int>();
		foreach (SerializedLifetimeStats.WeaponUses item2 in serializedLifetimeStats.UsesPerWeapon)
		{
			UsesPerWeapon[item2.WeaponID] = item2.Uses;
		}
	}

	public ISerializedData Serialize()
	{
		return new SerializedLifetimeStats
		{
			BestBlow = BestBlow,
			CriticalHits = CriticalHits,
			DamagesBlocked = DamagesBlocked,
			DamagesInflicted = DamagesInflicted,
			DamagesMitigatedByResistance = DamagesMitigatedByResistance,
			DamagesTakenOnArmor = DamagesTakenOnArmor,
			Dodges = Dodges,
			HealthLost = HealthLost,
			JumpsOverWallUsed = JumpsOverWallUsed,
			Kills = Kills,
			IsolatedKills = IsolatedKills,
			ManaSpent = ManaSpent,
			MostUnitsKilledInOneBlow = MostUnitsKilledInOneBlow,
			PunchesUsed = PunchesUsed,
			StunnedEnemies = StunnedEnemies,
			TilesCrossed = TilesCrossed,
			DamagesInflictedToEnemies = DamagesInflictedToEnemies.Select((KeyValuePair<string, float> o) => new SerializedLifetimeStats.DamageInflictedToEnemy
			{
				DamageInflicted = o.Value,
				EnemyID = o.Key
			}).ToList(),
			DamagesTakenByEnemyType = DamagesTakenByEnemyType.Select((KeyValuePair<string, float> o) => new SerializedLifetimeStats.DamageTakenFromEnemy
			{
				DamageTaken = o.Value,
				EnemyID = o.Key
			}).ToList(),
			UsesPerWeapon = UsesPerWeapon.Select((KeyValuePair<string, int> o) => new SerializedLifetimeStats.WeaponUses
			{
				WeaponID = o.Key,
				Uses = o.Value
			}).ToList()
		};
	}
}
