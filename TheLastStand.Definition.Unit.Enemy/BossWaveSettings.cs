using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class BossWaveSettings
{
	public bool AutoUpdateNightSlider { get; }

	public string BossUnitTemplateId { get; }

	public bool DisplayBossInSeer { get; }

	public bool DisplayEnemiesAmount { get; }

	public bool IsInfiniteWave { get; }

	public bool UseDefaultProgressBar { get; }

	public string SpecificPlaylistId { get; }

	public BossWaveSettings(XElement xBossElement)
	{
		AutoUpdateNightSlider = xBossElement.Element("AutoUpdateNightSlider") != null;
		XAttribute xAttribute = xBossElement.Attribute("BossId");
		BossUnitTemplateId = xAttribute.Value;
		DisplayBossInSeer = true;
		XAttribute xAttribute2 = xBossElement.Attribute("DisplayBossInSeer");
		if (xAttribute2 != null)
		{
			if (!bool.TryParse(xAttribute2.Value, out var result))
			{
				CLoggerManager.Log("Could not parse attribute DisplayInSeer into a bool : '" + xAttribute2.Value + "'.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
			}
			else
			{
				DisplayBossInSeer = result;
			}
		}
		DisplayEnemiesAmount = xBossElement.Element("DisplayEnemiesAmount") != null;
		IsInfiniteWave = xBossElement.Element("InfiniteWave") != null;
		XAttribute xAttribute3 = xBossElement.Attribute("SpecificPlaylistId");
		if (!string.IsNullOrEmpty(xAttribute3?.Value))
		{
			SpecificPlaylistId = xAttribute3.Value;
		}
		UseDefaultProgressBar = xBossElement.Element("UseDefaultProgressBar") != null;
	}
}
