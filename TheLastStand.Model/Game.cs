using TPLib;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization;

namespace TheLastStand.Model;

public class Game : ISerializable, IDeserializable
{
	public enum E_Cycle
	{
		Undefined,
		Day,
		Night
	}

	public enum E_DayTurn
	{
		Undefined,
		Production,
		Deployment
	}

	public enum E_NightTurn
	{
		Undefined,
		PlayableUnits,
		EnemyUnits,
		FinalBossDeath
	}

	public enum E_State
	{
		Off,
		Management,
		CharacterSheet,
		UnitPreparingSkill,
		UnitExecutingSkill,
		BuildingPreparingAction,
		BuildingExecutingAction,
		BuildingPreparingSkill,
		BuildingExecutingSkill,
		Construction,
		Recruitment,
		PlaceUnit,
		Shopping,
		BuildingUpgrade,
		Wait,
		LevelEdition,
		NightReport,
		ProductionReport,
		Settings,
		HowToPlay,
		GameOver,
		MetaShops,
		CutscenePlaying,
		UnitCustomisation,
		ConsentPopup,
		BlockingPopup,
		SaveSlots,
		ApocalypseMoreInfo
	}

	public enum E_GameOverCause
	{
		None,
		HeroesDeath,
		MagicCircleDestroyed,
		MagicSealsCompleted,
		Abandon
	}

	public static class Constants
	{
		public class PhaseNames
		{
			public const string Production = "Production";

			public const string Deployment = "Deployment";

			public const string Night = "Night";
		}
	}

	public int LastNightLastHour { get; set; }

	public int CurrentNightHour { get; set; }

	public Cursor Cursor { get; set; }

	public E_Cycle Cycle { get; set; }

	public int DayNumber { get; set; }

	public E_DayTurn DayTurn { get; set; }

	public E_GameOverCause GameOverCause { get; set; }

	public bool IsNightEnd
	{
		get
		{
			if (TPSingleton<GameManager>.Instance.Game.Cycle == E_Cycle.Night)
			{
				SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
				if (currentSpawnWave == null || currentSpawnWave.SpawnMightBeStuck)
				{
					for (int num = TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.Count - 1; num >= 0; num--)
					{
						if (!TPSingleton<EnemyUnitManager>.Instance.EnemyUnits[num].IsDead && !TPSingleton<EnemyUnitManager>.Instance.EnemyUnits[num].IgnoreFromEnemyUnitsCount)
						{
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}
	}

	public bool IsDayCycle => Cycle == E_Cycle.Day;

	public bool IsNightCycle => Cycle == E_Cycle.Night;

	public E_NightTurn NightTurn { get; set; }

	public E_State State { get; set; }

	public bool IsVictory => GameOverCause == E_GameOverCause.MagicSealsCompleted;

	public bool IsDefeat
	{
		get
		{
			if (GameOverCause != E_GameOverCause.HeroesDeath)
			{
				return GameOverCause == E_GameOverCause.MagicCircleDestroyed;
			}
			return true;
		}
	}

	public E_State PreviousState { get; set; }

	public Game()
	{
	}

	public Game(SerializedGame container)
	{
		Deserialize(container);
	}

	public void Deserialize(ISerializedData game = null, int saveVersion = -1)
	{
		if (game is SerializedGame serializedGame)
		{
			DayNumber = serializedGame.DayNumber;
			DayTurn = serializedGame.DayTurn;
			Cycle = serializedGame.Cycle;
			CurrentNightHour = serializedGame.NightHour;
			LastNightLastHour = serializedGame.LastNightLastHour;
		}
		else
		{
			DayNumber = 0;
			Cycle = E_Cycle.Day;
			DayTurn = GameManager.StartingDayTurn;
		}
		NightTurn = ((Cycle == E_Cycle.Night) ? E_NightTurn.PlayableUnits : E_NightTurn.Undefined);
	}

	public ISerializedData Serialize()
	{
		return new SerializedGame
		{
			Cycle = Cycle,
			DayNumber = DayNumber,
			DayTurn = DayTurn,
			NightHour = CurrentNightHour,
			LastNightLastHour = LastNightLastHour
		};
	}
}
