using System;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedSkill : ISerializedData
{
	public int BonusUses;

	public int? BonusUsesRemaining;

	public string Id;

	public int OverallUsesRemaining;

	public int OverallUsesRemainingBase;

	public int UsesPerTurnRemaining = -1;
}
