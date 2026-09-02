using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SpawnWaveDefinition : TheLastStand.Framework.Serialization.Definition
{
	public bool DisplayBossInSeer
	{
		get
		{
			if (IsBossWave)
			{
				return WaveEnemiesDefinition.BossWaveSettings.DisplayBossInSeer;
			}
			return false;
		}
	}

	public int Duration { get; private set; }

	public SpawnWaveEnemiesDefinition WaveEnemiesDefinition { get; private set; }

	public string Id { get; private set; }

	public bool IsBossWave => WaveEnemiesDefinition.BossWaveSettings != null;

	public BossWaveSettings BossWaveSettings => WaveEnemiesDefinition.BossWaveSettings;

	public bool IsInfinite
	{
		get
		{
			if (IsBossWave)
			{
				return WaveEnemiesDefinition.BossWaveSettings.IsInfiniteWave;
			}
			return false;
		}
	}

	public float SpawnsCountMultiplier { get; private set; }

	public Dictionary<int, float> TemporalDistribution { get; private set; } = new Dictionary<int, float>();

	public float TemporalDistributionTotalWeight { get; private set; }

	public SpawnWaveDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XAttribute xAttribute = xElement.Attribute("Id");
		if (xAttribute.IsNullOrEmpty())
		{
			CLoggerManager.Log("SpawnWaveDefinition has no Id!", LogType.Error);
			return;
		}
		Id = xAttribute.Value;
		WaveEnemiesDefinition = new SpawnWaveEnemiesDefinition(container);
		XElement xElement2 = xElement.Element("TemporalDistribution");
		if (xElement2 == null)
		{
			CLoggerManager.Log("SpawnWaveDefinition has no TemporalDistribution!", LogType.Error);
			return;
		}
		foreach (XElement item in xElement2.Elements("Turn"))
		{
			XAttribute xAttribute2 = item.Attribute("Id");
			if (xAttribute2.IsNullOrEmpty())
			{
				CLoggerManager.Log("Turn has no Id!", LogType.Error);
				continue;
			}
			if (!int.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("Turn has an invalid Id!", LogType.Error);
				continue;
			}
			if (result > Duration)
			{
				Duration = result;
			}
			XAttribute xAttribute3 = item.Attribute("Weight");
			if (xAttribute3.IsNullOrEmpty())
			{
				CLoggerManager.Log("Turn has no Weight!", LogType.Error);
				continue;
			}
			if (!float.TryParse(xAttribute3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
			{
				CLoggerManager.Log("Turn has an invalid Weight!", LogType.Error);
				continue;
			}
			TemporalDistribution.Add(result, result2);
			TemporalDistributionTotalWeight += result2;
		}
		float result3 = 1f;
		XElement xElement3 = xElement.Element("SpawnsCountMultiplier");
		if (!xElement3.IsNullOrEmpty() && !float.TryParse(xElement3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result3))
		{
			CLoggerManager.Log("SpawnWaveDefinition has an invalid SpawnsCountMultiplier!", LogType.Error);
		}
		else
		{
			SpawnsCountMultiplier = result3;
		}
	}
}
