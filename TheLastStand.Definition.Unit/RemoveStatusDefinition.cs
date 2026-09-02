using System;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class RemoveStatusDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string Id = "RemoveStatus";

		public const string DispelId = "Dispel";

		public const string DischargeId = "Discharge";
	}

	public float BaseChance { get; private set; } = 1f;

	public string Id { get; private set; } = "RemoveStatus";

	public Status.E_StatusType Status { get; private set; }

	public RemoveStatusDefinition(XContainer container)
		: base(container)
	{
	}

	public static string GetRemovedStatusIconName(Status.E_StatusType status)
	{
		return status switch
		{
			TheLastStand.Model.Status.Status.E_StatusType.Buff => "Buff", 
			TheLastStand.Model.Status.Status.E_StatusType.Debuff => "Debuff", 
			TheLastStand.Model.Status.Status.E_StatusType.Poison => "Poison", 
			TheLastStand.Model.Status.Status.E_StatusType.Stun => "Stun", 
			TheLastStand.Model.Status.Status.E_StatusType.Charged => "Charged", 
			TheLastStand.Model.Status.Status.E_StatusType.AllNegative => "NegativeAlterations", 
			TheLastStand.Model.Status.Status.E_StatusType.RemovablePositive => "PositiveAlterations", 
			_ => string.Empty, 
		};
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		XElement xElement2 = xElement.Element("BaseChance");
		if (xElement2 != null)
		{
			if (!float.TryParse(xElement2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("Could not parse RemoveStatus BaseChance element value " + xElement2.Value + " to a valid float value.", LogType.Error);
				return;
			}
			BaseChance = result;
		}
		XElement xElement3 = xElement.Element("Status");
		if (Enum.TryParse<Status.E_StatusType>(xElement3.Value, out var result2))
		{
			Status = result2;
			if (Status == TheLastStand.Model.Status.Status.E_StatusType.Charged)
			{
				Id = "Discharge";
			}
			else if (TheLastStand.Model.Status.Status.E_StatusType.AllNegative.HasFlag(Status))
			{
				Id = "Dispel";
			}
		}
		else
		{
			CLoggerManager.Log("Incorrect Status used for " + Id + " definition! : unable to parse " + xElement3.Value, LogType.Error);
		}
	}
}
