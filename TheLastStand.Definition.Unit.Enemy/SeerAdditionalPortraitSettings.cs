using System;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SeerAdditionalPortraitSettings
{
	public bool DisplayPortraitAmount { get; }

	public int PortraitAmount { get; }

	public string PortraitTemplateId { get; }

	public DamageableType PortraitType { get; private set; } = DamageableType.Other;

	public SeerAdditionalPortraitSettings(XElement xSeerAdditionalPortrait)
	{
		PortraitType = DamageableType.Enemy;
		XAttribute xAttribute = xSeerAdditionalPortrait.Attribute("Type");
		if (xAttribute != null)
		{
			if (!Enum.TryParse<DamageableType>(xAttribute.Value, out var result))
			{
				CLoggerManager.Log("Could not parse attribute Type into a DamageableType : '" + xAttribute.Value + "'.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
			}
			else
			{
				PortraitType = result;
			}
		}
		XAttribute xAttribute2 = xSeerAdditionalPortrait.Attribute("Id");
		PortraitTemplateId = xAttribute2.Value;
		XAttribute xAttribute3 = xSeerAdditionalPortrait.Attribute("Amount");
		DisplayPortraitAmount = false;
		if (xAttribute3 != null)
		{
			DisplayPortraitAmount = true;
			if (!int.TryParse(xAttribute3.Value, out var result2))
			{
				CLoggerManager.Log("Could not parse attribute Amount into an int : '" + xAttribute3.Value + "'.", TPSingleton<SpawnWaveDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "SpawnWaveDatabase");
			}
			else
			{
				PortraitAmount = result2;
			}
		}
	}
}
