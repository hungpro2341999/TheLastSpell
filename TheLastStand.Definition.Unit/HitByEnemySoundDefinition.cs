using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class HitByEnemySoundDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Consts
	{
		public static class Targets
		{
			public const string PlayingUnits = "PlayingUnits";

			public const string Buildings = "Buildings";
		}
	}

	public string Target { get; private set; }

	public Dictionary<Vector2Int, List<string>> SoundsPerNumberOfEnemies { get; private set; }

	public HitByEnemySoundDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement obj = container as XElement;
		XAttribute xAttribute = obj.Attribute("Target");
		if (xAttribute == null)
		{
			Debug.LogError("HitByEnemySoundDefinition must have Target");
		}
		Target = xAttribute.Value;
		SoundsPerNumberOfEnemies = new Dictionary<Vector2Int, List<string>>();
		foreach (XElement item in obj.Elements("SoundsPerNumberOfEnemies"))
		{
			XAttribute xAttribute2 = item.Attribute("Min");
			if (xAttribute2 == null)
			{
				Debug.LogError("SoundsPerNumberOfEnemies must have Min");
			}
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				Debug.LogError("SoundsPerNumberOfEnemies must have valid Min");
			}
			int result2 = -1;
			XAttribute xAttribute3 = item.Attribute("Max");
			if (xAttribute3 != null && !int.TryParse(xAttribute3.Value, out result2))
			{
				Debug.LogError("SoundsPerNumberOfEnemies Max is invalid");
			}
			List<string> list = new List<string>();
			foreach (XElement item2 in item.Elements("SoundId"))
			{
				XAttribute xAttribute4 = item2.Attribute("Value");
				if (xAttribute4 == null)
				{
					Debug.LogError("SoundId must have value");
				}
				list.Add(xAttribute4.Value);
			}
			SoundsPerNumberOfEnemies.Add(new Vector2Int(result, result2), list);
		}
	}

	public string GetSoundId(int targetsCount)
	{
		foreach (KeyValuePair<Vector2Int, List<string>> soundsPerNumberOfEnemy in SoundsPerNumberOfEnemies)
		{
			if (targetsCount >= soundsPerNumberOfEnemy.Key.x && (targetsCount <= soundsPerNumberOfEnemy.Key.y || soundsPerNumberOfEnemy.Key.y == -1))
			{
				return soundsPerNumberOfEnemy.Value[RandomManager.GetRandomRange(this, 0, soundsPerNumberOfEnemy.Value.Count)];
			}
		}
		Debug.LogError($"No sound found, {targetsCount} targets)");
		return string.Empty;
	}
}
