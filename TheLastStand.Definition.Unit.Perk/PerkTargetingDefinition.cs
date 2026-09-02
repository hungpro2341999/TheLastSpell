using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class PerkTargetingDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_TargetingMethod
	{
		Self,
		ClosestTarget,
		AdjacentDamageables,
		DamageablesInRange,
		PlayableUnitsInRange,
		ClosestDamageablesInRange
	}

	public enum E_TargetingReference
	{
		Owner,
		Caster,
		Target,
		AllTargets
	}

	public static class Constants
	{
		public const string Id = "PerkTargeting";
	}

	public Node AmountExpression { get; private set; }

	public Node RangeExpression { get; private set; }

	public List<DamageableType> ValidDamageableTypes { get; private set; }

	public E_TargetingMethod TargetingMethod { get; private set; }

	public E_TargetingReference TargetingReference { get; private set; }

	public Status.E_StatusType TargetHasStatus { get; private set; }

	public bool HasStatusInverted { get; private set; }

	public PerkTargetingDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!(container is XElement xElement))
		{
			return;
		}
		XAttribute xAttribute = xElement.Attribute("Amount");
		if (!string.IsNullOrEmpty(xAttribute?.Value))
		{
			string text = xAttribute.Value.Replace(base.TokenVariables);
			if (!string.IsNullOrEmpty(text))
			{
				AmountExpression = Parser.Parse(text);
			}
			else
			{
				CLoggerManager.Log("Could not parse Amount attribute : " + xAttribute.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkTargetingDefinition");
			}
		}
		XAttribute xAttribute2 = xElement.Attribute("Range");
		if (!string.IsNullOrEmpty(xAttribute2?.Value))
		{
			string text2 = xAttribute2.Value.Replace(base.TokenVariables);
			if (!string.IsNullOrEmpty(text2))
			{
				RangeExpression = Parser.Parse(text2);
			}
			else
			{
				CLoggerManager.Log("Could not parse Range attribute : " + xAttribute2.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkTargetingDefinition");
			}
		}
		XAttribute xAttribute3 = xElement.Attribute("TargetingReference");
		if (xAttribute3 != null)
		{
			if (Enum.TryParse<E_TargetingReference>(xAttribute3.Value, out var result))
			{
				TargetingReference = result;
			}
			else
			{
				CLoggerManager.Log("Could not parse TargetingReference attribute into an E_TargetingReference : " + xAttribute3.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkTargetingDefinition");
			}
		}
		XAttribute xAttribute4 = xElement.Attribute("TargetingMethod");
		if (xAttribute4 != null)
		{
			if (Enum.TryParse<E_TargetingMethod>(xAttribute4.Value, out var result2))
			{
				TargetingMethod = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse TargetingMethod attribute into an E_TargetingMethod : " + xAttribute4.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkTargetingDefinition");
			}
		}
		foreach (XElement item in xElement.Elements("DamageableTarget"))
		{
			XAttribute xAttribute5 = item.Attribute("Type");
			if (xAttribute5 == null)
			{
				CLoggerManager.Log("Missing Type attribute in a DamageableTarget element.", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkTargetingDefinition");
			}
			else
			{
				if (string.IsNullOrEmpty(xAttribute5?.Value))
				{
					continue;
				}
				if (Enum.TryParse<DamageableType>(xAttribute5.Value, out var result3))
				{
					if (ValidDamageableTypes == null)
					{
						ValidDamageableTypes = new List<DamageableType>();
					}
					ValidDamageableTypes.Add(result3);
				}
				else
				{
					CLoggerManager.Log("Could not parse Type attribute into a DamageableType : " + xAttribute5.Value + ".", LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "PerkTargetingDefinition");
				}
			}
		}
		XElement xElement2 = xElement.Element("HasStatus");
		XAttribute xAttribute6 = xElement2?.Attribute("StatusType");
		if (xAttribute6 == null)
		{
			return;
		}
		if (Enum.TryParse<Status.E_StatusType>(xAttribute6.Value, out var result4))
		{
			TargetHasStatus = result4;
		}
		else
		{
			CLoggerManager.Log("PerkTargetingDefinition StatusType is incorrect: " + xAttribute6.Value + " is not a valid StatusType", LogType.Error);
		}
		XAttribute xAttribute7 = xElement2.Attribute("Inverted");
		if (xAttribute7 != null)
		{
			if (bool.TryParse(xAttribute7.Value, out var result5))
			{
				HasStatusInverted = result5;
			}
			else
			{
				CLoggerManager.Log("PerkTargetingDefinition StatusType attribute Inverted is incorrect: " + xAttribute7.Value + " is not a valid bool", LogType.Error);
			}
		}
	}
}
