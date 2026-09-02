using System;
using TheLastStand.Model;

namespace TheLastStand.Serialization;

[Serializable]
public class SerializedGame : ISerializedData
{
	public int DayNumber;

	public Game.E_Cycle Cycle;

	public Game.E_DayTurn DayTurn;

	public int NightHour;

	public int LastNightLastHour;
}
