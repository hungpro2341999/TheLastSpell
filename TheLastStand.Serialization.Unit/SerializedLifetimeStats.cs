using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Unit;

[Serializable]
public class SerializedLifetimeStats : ISerializedData
{
	[Serializable]
	public struct DamageInflictedToEnemy
	{
		[XmlAttribute]
		public string EnemyID;

		public float DamageInflicted;
	}

	[Serializable]
	public struct DamageTakenFromEnemy
	{
		[XmlAttribute]
		public string EnemyID;

		public float DamageTaken;
	}

	[Serializable]
	public struct WeaponUses
	{
		[XmlAttribute]
		public string WeaponID;

		public int Uses;
	}

	public float BestBlow;

	public int CriticalHits;

	public int DamagesBlocked;

	public float DamagesInflicted;

	public float DamagesMitigatedByResistance;

	public float DamagesTakenOnArmor;

	public int Dodges;

	public float HealthLost;

	public int JumpsOverWallUsed;

	public int Kills;

	public int IsolatedKills;

	public int ManaSpent;

	public int MostUnitsKilledInOneBlow;

	public int PunchesUsed;

	public int StunnedEnemies;

	public int TilesCrossed;

	public List<DamageTakenFromEnemy> DamagesTakenByEnemyType = new List<DamageTakenFromEnemy>();

	public List<WeaponUses> UsesPerWeapon = new List<WeaponUses>();

	public List<DamageInflictedToEnemy> DamagesInflictedToEnemies = new List<DamageInflictedToEnemy>();
}
