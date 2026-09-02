using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Affix;

public class EnemyAffixDefinition : LocalizableDefinition
{
	public EnemyAffixEffectDefinition.E_EnemyAffixBoxType BoxType { get; private set; }

	public HashSet<string> ExcludedElites { get; private set; }

	public EnemyAffixEffectDefinition EnemyAffixEffectDefinition { get; private set; }

	public string Id { get; private set; }

	public bool IsEliteAffix { get; private set; }

	public string LockedByApocalypseFlag { get; private set; }

	public string UnlockedByApocalypseFlag { get; private set; }

	public float Weight { get; private set; }

	public EnemyAffixDefinition(XContainer container)
		: base(container)
	{
	}

	public string GetAdditionalDescription(InterpreterContext interpreter)
	{
		if (!Localizer.Exists(string.Format("{0}{1}", "EnemyAffix_AdditionalDescription_", EnemyAffixEffectDefinition.EnemyAffixEffect)))
		{
			return null;
		}
		return Localizer.Format(string.Format("{0}{1}", "EnemyAffix_AdditionalDescription_", EnemyAffixEffectDefinition.EnemyAffixEffect), GetArguments(interpreter));
	}

	public string GetDescription(InterpreterContext interpreter)
	{
		return Localizer.Format(string.Format("{0}{1}", "EnemyAffix_Description_", EnemyAffixEffectDefinition.EnemyAffixEffect), GetArguments(interpreter));
	}

	public string GetTitle()
	{
		return Localizer.Get(string.Format("{0}{1}", "EnemyAffix_Name_", EnemyAffixEffectDefinition.EnemyAffixEffect));
	}

	public override void Deserialize(XContainer container)
	{
		XElement xElement = container as XElement;
		Weight = 1f;
		ExcludedElites = new HashSet<string>();
		XAttribute xAttribute = xElement.Attribute("Id");
		Id = xAttribute.Value;
		DeserializeTokenVariables(xElement.Element("TokenVariables"));
		base.Deserialize((XContainer)xElement.Element("LocArguments"));
		XElement xElement2 = xElement.Element("IsEliteAffix");
		IsEliteAffix = xElement2 != null;
		if (IsEliteAffix)
		{
			XAttribute xAttribute2 = xElement2.Attribute("Weight");
			if (!float.TryParse(xAttribute2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log("Could not parse Weight into a float for affix \"" + Id + "\". Value : \"" + xAttribute2.Value + "\". Setting it to 1", TPSingleton<EnemyUnitDatabase>.Instance, LogType.Warning, CLogLevel.MAJOR, forcePrintInUnity: true, "EnemyUnitDatabase");
			}
			else
			{
				Weight = result;
			}
			XElement xElement3 = xElement2.Element("ExcludedElites");
			if (xElement3 != null)
			{
				foreach (XElement item in xElement3.Elements("ExcludedElite"))
				{
					ExcludedElites.Add(item.Value);
				}
			}
			XAttribute xAttribute3 = xElement2.Attribute("LockedByApocalypseFlag");
			if (xAttribute3 != null)
			{
				LockedByApocalypseFlag = xAttribute3.Value;
			}
			XAttribute xAttribute4 = xElement2.Attribute("UnlockedByApocalypseFlag");
			if (xAttribute4 != null)
			{
				UnlockedByApocalypseFlag = xAttribute4.Value;
			}
		}
		XElement xElement4 = xElement.Element("OverrideBoxAsset");
		if (xElement4 != null)
		{
			XAttribute xAttribute5 = xElement4.Attribute("BoxId");
			if (Enum.TryParse<EnemyAffixEffectDefinition.E_EnemyAffixBoxType>(xAttribute5.Value, out var result2))
			{
				BoxType = result2;
			}
			else
			{
				CLoggerManager.Log("Could not parse BoxId into an E_EnemyAffixBoxType : " + xAttribute5.Value);
				BoxType = EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Base;
			}
		}
		else
		{
			BoxType = EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Base;
		}
		XElement xElement5 = xElement.Element("AffixEffect");
		if (xElement5.Element("Reinforced") != null)
		{
			EnemyAffixEffectDefinition = new EnemyReinforcedAffixEffectDefinition(xElement5.Element("Reinforced"), base.TokenVariables);
		}
		else if (xElement5.Element("Aura") != null)
		{
			EnemyAffixEffectDefinition = new EnemyAuraAffixEffectDefinition(xElement5.Element("Aura"), base.TokenVariables);
		}
		else if (xElement5.Element("Mirror") != null)
		{
			EnemyAffixEffectDefinition = new EnemyMirrorAffixEffectDefinition(xElement5.Element("Mirror"), base.TokenVariables);
		}
		else if (xElement5.Element("Misty") != null)
		{
			EnemyAffixEffectDefinition = new EnemyMistyAffixEffectDefinition(xElement5.Element("Misty"), base.TokenVariables);
		}
		else if (xElement5.Element("Regenerative") != null)
		{
			EnemyAffixEffectDefinition = new EnemyRegenerativeAffixEffectDefinition(xElement5.Element("Regenerative"), base.TokenVariables);
		}
		else if (xElement5.Element("Energetic") != null)
		{
			EnemyAffixEffectDefinition = new EnemyEnergeticAffixEffectDefinition(xElement5.Element("Energetic"), base.TokenVariables);
		}
		else if (xElement5.Element("Revenge") != null)
		{
			EnemyAffixEffectDefinition = new EnemyRevengeAffixEffectDefinition(xElement5.Element("Revenge"), base.TokenVariables);
		}
		else if (xElement5.Element("Purge") != null)
		{
			EnemyAffixEffectDefinition = new EnemyPurgeAffixEffectDefinition(xElement5.Element("Purge"), base.TokenVariables);
		}
		else if (xElement5.Element("Barrier") != null)
		{
			EnemyAffixEffectDefinition = new EnemyBarrierAffixEffectDefinition(xElement5.Element("Barrier"), base.TokenVariables);
		}
		else if (xElement5.Element("HigherPlane") != null)
		{
			EnemyAffixEffectDefinition = new EnemyHigherPlaneAffixEffectDefinition(xElement5.Element("HigherPlane"), base.TokenVariables);
		}
		else if (xElement5.Element("HealthChunks") != null)
		{
			EnemyAffixEffectDefinition = new EnemyHealthChunksAffixEffectDefinition(xElement5.Element("HealthChunks"), base.TokenVariables);
		}
		else
		{
			CLoggerManager.Log("There is no effect defined for affix \"" + Id + "\". Did you forget to add the deserialization ?", TPSingleton<EnemyUnitDatabase>.Instance, LogType.Error, CLogLevel.MAJOR, forcePrintInUnity: true, "EnemyUnitDatabase");
		}
	}
}
