using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheLastStand.Manager;

public class RandomManager : Manager<RandomManager>, ISerializable, IDeserializable
{
	private static class Constants
	{
		public static Vector2Int TilesNoiseOffsetRange = new Vector2Int(-10000, 10000);
	}

	[SerializeField]
	[Tooltip("Seed used to init the PRNG for the whole game. Use 0 for a random seed.")]
	private int baseSeed;

	private string lastCaller = string.Empty;

	private Dictionary<string, System.Random> randomLibrary = new Dictionary<string, System.Random>();

	private Dictionary<string, byte[]> savedStates = new Dictionary<string, byte[]>();

	[SerializeField]
	private float debugPerlinRemapOffset = 0.2f;

	[DevConsoleCommand("RandomSeed", Options = DevConsoleCommandOptions.CanRead)]
	public static int BaseSeed => TPSingleton<RandomManager>.Instance.baseSeed;

	public static int TilesNoiseRandomOffset { get; protected set; }

	public static int TilesNoiseRandomOffsetBis { get; protected set; }

	[DevConsoleCommand("TilesNoiseOffset", Options = DevConsoleCommandOptions.CanReadWrite)]
	public static int DebugTilesNoiseOffset
	{
		get
		{
			return TilesNoiseRandomOffset;
		}
		protected set
		{
			TilesNoiseRandomOffset = value;
			Tilemap[] array = UnityEngine.Object.FindObjectsOfType<Tilemap>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RefreshAllTiles();
			}
		}
	}

	[DevConsoleCommand("TilesNoiseOffsetBis", Options = DevConsoleCommandOptions.CanReadWrite)]
	public static int DebugTilesNoiseOffsetBis
	{
		get
		{
			return TilesNoiseRandomOffsetBis;
		}
		protected set
		{
			TilesNoiseRandomOffsetBis = value;
			Tilemap[] array = UnityEngine.Object.FindObjectsOfType<Tilemap>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RefreshAllTiles();
			}
		}
	}

	public static void ClearSavedState(string id)
	{
		if (TPSingleton<RandomManager>.Instance.savedStates.ContainsKey(id))
		{
			TPSingleton<RandomManager>.Instance.Log("Cleared saved random state for ID " + id);
			TPSingleton<RandomManager>.Instance.savedStates.Remove(id);
		}
	}

	public static void ClearSavedState(object caller)
	{
		ClearSavedState(caller.GetType().Name);
	}

	public static float GetHashedWhiteNoise(float x, float y, int offset)
	{
		x = ((x + (float)offset) * 0.1031f).Frac();
		y = ((y + (float)offset) * 0.1031f).Frac();
		Vector3 vector = new Vector3(x, y, x);
		vector = vector.Add(Vector3.Dot(vector, new Vector3(vector.y + 19.19f, vector.z + 19.19f, vector.x + 19.19f)));
		return ((vector.x + vector.y) * vector.z).Frac();
	}

	public static float GetHashedWhiteNoise(float x, float y)
	{
		return GetHashedWhiteNoise(x, y, useTilesNoiseRandomOffsetBis: false);
	}

	public static float GetHashedWhiteNoise(float x, float y, bool useTilesNoiseRandomOffsetBis)
	{
		return GetHashedWhiteNoise(x, y, useTilesNoiseRandomOffsetBis ? TilesNoiseRandomOffsetBis : TilesNoiseRandomOffset);
	}

	public static float GetPerlinValue(Vector3Int position, float scale, float offset)
	{
		return Mathf.PerlinNoise(((float)position.x + offset) * scale, ((float)position.y + offset) * scale).Remap(0f, 1f, 0f - TPSingleton<RandomManager>.Instance.debugPerlinRemapOffset, 1f + TPSingleton<RandomManager>.Instance.debugPerlinRemapOffset);
	}

	public static int GetPositionBasedRandomIndex(Vector3Int position, int maxVal, bool useAltNoiseOffset = false)
	{
		return Mathf.Clamp(Mathf.RoundToInt(GetHashedWhiteNoise(position.x, position.y, useAltNoiseOffset) * (float)maxVal), 0, maxVal);
	}

	public static float GetPositionBasedRandomRange(Vector3Int position, float min, float max)
	{
		return GetHashedWhiteNoise(position.x, position.y) * (max - min) + min;
	}

	public static bool GetRandomBool(string id)
	{
		return GetRandomRange(id, 0f, 1f) > 0.5f;
	}

	public static bool GetRandomBool(object caller)
	{
		return GetRandomRange(caller, 0f, 1f) > 0.5f;
	}

	public static T GetRandomElement<T>(object caller, IEnumerable<T> list)
	{
		return list.ElementAt(GetRandomRange(caller, 0, list.Count()));
	}

	public static T GetRandomElement<T>(string id, IEnumerable<T> list)
	{
		return list.ElementAt(GetRandomRange(id, 0, list.Count()));
	}

	public static int GetRandomRange(string id, int min, int max)
	{
		TPSingleton<RandomManager>.Instance.lastCaller = id;
		return GetRandomRange(GetRandomForCaller(id), min, max);
	}

	public static float GetRandomRange(string id, float min, float max)
	{
		TPSingleton<RandomManager>.Instance.lastCaller = id;
		return GetRandomRange(GetRandomForCaller(id), min, max);
	}

	public static int GetRandomRange(object caller, int min, int max)
	{
		return GetRandomRange(caller.GetType().Name, min, max);
	}

	public static float GetRandomRange(object caller, float min, float max)
	{
		return GetRandomRange(caller.GetType().Name, min, max);
	}

	public static void SaveState(object caller)
	{
		SaveState(caller.GetType().Name);
	}

	public static void SaveState(string id)
	{
		TPSingleton<RandomManager>.Instance.Log("Saved random state for ID " + id);
		System.Random randomForCaller = GetRandomForCaller(id);
		TPSingleton<RandomManager>.Instance.savedStates[id] = SerializeRandom(randomForCaller);
	}

	public static IEnumerable<T> Shuffle<T>(object caller, IEnumerable<T> enumerable)
	{
		return Shuffle(caller.GetType().Name, enumerable);
	}

	public static IEnumerable<T> Shuffle<T>(string id, IEnumerable<T> enumerable)
	{
		System.Random random = GetRandomForCaller(id);
		IEnumerable<T> enumerable2 = from o in enumerable
			select (o) into o
			orderby random.Next()
			select o;
		TPSingleton<RandomManager>.Instance.Log(id + ":SHUFFLE [" + enumerable.ToString() + " => " + string.Join(", ", enumerable2) + "]", CLogLevel.DETAILED);
		return enumerable2;
	}

	public static System.Random GetRandomForCaller(string id)
	{
		if (!TPSingleton<RandomManager>.Instance.randomLibrary.TryGetValue(id, out var value))
		{
			value = new System.Random(TPSingleton<RandomManager>.Instance.baseSeed + id.Length);
			TPSingleton<RandomManager>.Instance.randomLibrary[id] = value;
		}
		return value;
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		randomLibrary = new Dictionary<string, System.Random>();
		savedStates = new Dictionary<string, byte[]>();
		if (baseSeed != 0)
		{
			TPSingleton<RandomManager>.Instance.LogWarning("BaseSeed has been defined, it is not random.", CLogLevel.MAJOR);
		}
		if (!(container is SerializedRandoms serializedRandoms))
		{
			baseSeed = (int)((baseSeed == 0) ? DateTime.Now.Ticks : baseSeed);
		}
		else
		{
			baseSeed = serializedRandoms.BaseSeed;
			foreach (SerializedRandom item in serializedRandoms.RandomLibrary)
			{
				using MemoryStream serializationStream = new MemoryStream(item.SavedState ?? item.CurrentState);
				System.Random value = new BinaryFormatter().Deserialize(serializationStream) as System.Random;
				randomLibrary.Add(item.CallerID, value);
			}
		}
		TPSingleton<RandomManager>.Instance.Log($"GAME BASE SEED: {baseSeed}", CLogLevel.MAJOR);
		SaveState(this);
		TilesNoiseRandomOffset = GetRandomRange(this, Constants.TilesNoiseOffsetRange.x, Constants.TilesNoiseOffsetRange.y);
		TilesNoiseRandomOffsetBis = GetRandomRange(this, Constants.TilesNoiseOffsetRange.x, Constants.TilesNoiseOffsetRange.y);
	}

	public ISerializedData Serialize()
	{
		return new SerializedRandoms
		{
			BaseSeed = baseSeed,
			RandomLibrary = randomLibrary.Select((KeyValuePair<string, System.Random> o) => new SerializedRandom
			{
				CallerID = o.Key,
				SavedState = (savedStates.ContainsKey(o.Key) ? savedStates[o.Key] : null),
				CurrentState = SerializeRandom(GetRandomForCaller(o.Key))
			}).ToList()
		};
	}

	private static int GetRandomRange(System.Random random, int min, int max)
	{
		return random.Next(min, max);
	}

	private static float GetRandomRange(System.Random random, float min, float max)
	{
		return (float)random.NextDouble() * (max - min) + min;
	}

	private static byte[] SerializeRandom(System.Random random)
	{
		using MemoryStream memoryStream = new MemoryStream();
		new BinaryFormatter().Serialize(memoryStream, random);
		return memoryStream.ToArray();
	}

	[ContextMenu("Refresh Tiles Noise Offset")]
	[DevConsoleCommand("RefreshTilesNoiseOffset", Options = DevConsoleCommandOptions.ForceStatic)]
	public void DebugRefreshTilesPerlinOffset()
	{
		TPSingleton<RandomManager>.Instance.Log("Refreshed tiles' Noise offset");
		DebugTilesNoiseOffset = GetRandomRange(this, Constants.TilesNoiseOffsetRange.x, Constants.TilesNoiseOffsetRange.y);
		DebugTilesNoiseOffsetBis = GetRandomRange(this, Constants.TilesNoiseOffsetRange.x, Constants.TilesNoiseOffsetRange.y);
	}
}
