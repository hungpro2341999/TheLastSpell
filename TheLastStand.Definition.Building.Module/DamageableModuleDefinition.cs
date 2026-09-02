using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class DamageableModuleDefinition : BuildingModuleDefinition
{
	public string DamagedParticlesId { get; private set; } = string.Empty;

	public bool CanPanic => TotalPanicValue > 0f;

	public float TotalPanicValue { get; private set; }

	public bool DisableDestructionSmokeFX { get; private set; }

	public byte FlameCount { get; private set; }

	public List<Vector2> FlamesPositions { get; private set; } = new List<Vector2>();

	public int GlyphHealthTotalPercentageModifier
	{
		get
		{
			int num = 0;
			if (BuildingDefinition.IdListIds != null)
			{
				foreach (string idListId in BuildingDefinition.IdListIds)
				{
					num += TPSingleton<GlyphManager>.Instance.BuildingHealthModifiers.GetValueOrDefault(idListId);
				}
			}
			return num;
		}
	}

	public float HealthTotal => BuildingManager.ComputeBuildingTotalHealth(this);

	public bool KeepHUDDisplayedWhenDamaged { get; private set; }

	public float NativeHealthTotal { get; private set; }

	public DamageableModuleDefinition(BuildingDefinition buildingDefinition, XContainer damageableDefinition)
		: base(buildingDefinition, damageableDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XElement xElement2 = xElement.Element("HealthTotal");
		if (xElement2 != null)
		{
			if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("Building " + BuildingDefinition.Id + "'s Health " + HasAnInvalidFloat(xElement2.Value), LogType.Error);
				return;
			}
			NativeHealthTotal = result;
		}
		KeepHUDDisplayedWhenDamaged = xElement.Element("KeepHUDDisplayedWhenDamaged") != null;
		XElement xElement3 = xElement.Element("FlameCount");
		if (xElement3 != null)
		{
			if (byte.TryParse(xElement3.Value, out var result2))
			{
				FlameCount = result2;
			}
			else
			{
				CLoggerManager.Log("Building " + BuildingDefinition.Id + "'s FlameCount " + HasAnInvalid("byte", xElement3.Value), LogType.Warning);
			}
		}
		XElement xElement4 = xElement.Element("FlameAnchorPoints");
		if (xElement4 != null)
		{
			foreach (XElement item2 in xElement4.DescendantNodes())
			{
				try
				{
					Vector2 item = new Vector2(float.Parse(item2.Attribute("PositionX").Value, NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(item2.Attribute("PositionY").Value, NumberStyles.Float, CultureInfo.InvariantCulture));
					FlamesPositions.Add(item);
				}
				catch (FormatException)
				{
					CLoggerManager.Log("While deserializing building " + BuildingDefinition.Id + ", I found an invalid flame position!", LogType.Warning);
				}
			}
			if (FlameCount > FlamesPositions.Count)
			{
				CLoggerManager.Log("Error while deserializing Building " + BuildingDefinition.Id + ": You cannot add more flames to the building than there are flamed positions.", LogType.Error);
			}
		}
		DisableDestructionSmokeFX = xElement.Element("NoSmokeFXOnDestruction") != null;
		if (xElement.Element("DamagedParticles") != null)
		{
			XAttribute xAttribute = xElement.Element("DamagedParticles").Attribute("Id");
			DamagedParticlesId = xAttribute.Value;
		}
		XElement xElement6 = xElement.Element("TotalPanicValue");
		if (!string.IsNullOrEmpty(xElement6?.Value))
		{
			if (float.TryParse(xElement6.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				TotalPanicValue = result3;
			}
			else
			{
				CLoggerManager.Log("Error while deserializing Building " + BuildingDefinition.Id + ": Unable to parse totalPanicValueElement.Value into float", LogType.Warning);
			}
		}
	}
}
